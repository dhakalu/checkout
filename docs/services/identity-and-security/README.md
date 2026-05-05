# Identity & Security Service (STS)

> **Bounded context:** Authentication, token lifecycle, authorization policy, secret management, and compliance for all human and machine identities in the Checkout platform.

---

## Overview

The STS is the cryptographic root of trust for the entire platform. Every human login, every service-to-service call, and every drone operator action is gated through a token issued or validated here. It is built on **OpenIddict** (OIDC/OAuth2), backed by **PostgreSQL** for identity storage, and integrated with **Redis** for token revocation and session state.

> **Scope of this document:** This spec extends the high-level README to cover the gaps in authorization, token lifecycle, secret management, multi-tenancy, and compliance that are not addressed in the top-level architecture overview.

---

## Responsibilities

| Responsibility | In scope | Out of scope |
|---|---|---|
| User authentication (passwords, MFA, SSO) | ✅ | |
| Machine authentication (Client Credentials) | ✅ | |
| JWT issuance and signing key management | ✅ | |
| Token revocation | ✅ | |
| RBAC role & scope definition | ✅ | |
| Authorization *decisions* at runtime | ✅ (via claims) | Fine-grained resource checks (delegated to each service) |
| Secret distribution to other services | ✅ | |
| Audit event emission | ✅ | Audit log storage (→ Audit Log Service) |
| Fraud / bot detection signals | ✅ (consumed) | Fraud model training (→ external) |

---

## Authentication

### Supported flows

| Flow | Used by | Notes |
|---|---|---|
| Authorization Code + PKCE | Web & mobile clients | Required for all human logins |
| Client Credentials | Service-to-service | Each service has a dedicated credential |
| Refresh Token | Web & mobile | Sliding expiry; rotated on every use |
| Device Authorization | Drone operator terminals | Headless devices without a browser |

### Multi-Factor Authentication

- TOTP (RFC 6238) via authenticator app — required for all human accounts
- SMS OTP as fallback — **not** permitted for operator or admin roles due to SIM-swap risk
- WebAuthn / FIDO2 hardware keys — optional, required for `admin` role

### Identity Federation (Enterprise SSO)

Enterprise B2B customers may bring their own IdP. The STS acts as a federation hub:

- **Inbound OIDC** — external IdP authenticates the user; the STS issues a first-party JWT with normalized claims
- **Inbound SAML 2.0** — for legacy enterprise IdPs; assertion mapped to internal claim schema
- Tenant-specific federation configuration stored per `tenant_id`
- JIT (Just-In-Time) provisioning: accounts are created on first successful federated login within defined role constraints

---

## Token Design

### JWT claim schema

Every access token issued by the STS includes the following claims:

```json
{
  "sub":        "usr_01J...",
  "tenant_id":  "ten_01J...",
  "roles":      ["customer"],
  "scope":      "orders:read orders:write notifications:read",
  "jti":        "tok_01J...",
  "iat":        1700000000,
  "exp":        1700000900,
  "iss":        "https://sts.checkout.internal",
  "aud":        ["order-service", "notification-engine"]
}
```

Service credential tokens (Client Credentials) omit `sub` and `roles`; they carry only `scope` and `aud`.

### Token lifetimes

| Token type | Lifetime | Rotation |
|---|---|---|
| Access token (human) | 15 minutes | — |
| Access token (service) | 60 minutes | — |
| Refresh token | 24 hours sliding | Rotated on every use; old token immediately invalidated |
| ID token | 15 minutes | Not used for API calls |
| Authorization code | 60 seconds | Single use |

### Signing keys

- Algorithm: **ES256** (ECDSA P-256) — preferred over RS256 for smaller token size
- Key rotation: every **30 days**, automated via the secret management pipeline
- JWKS endpoint: `https://sts.checkout.internal/.well-known/jwks.json` — cached by all services with a 5-minute TTL
- Old keys remain in the JWKS endpoint for one full rotation cycle to allow in-flight token validation

---

## Token Revocation

JWTs are stateless by default. The STS maintains a **Redis-backed revocation blocklist** to support immediate invalidation.

### When revocation is triggered

- User-initiated logout
- Admin-initiated account suspension or lock
- Detected credential compromise (signal from fraud detection)
- Service credential rotation
- Password reset (all existing refresh tokens revoked)

### Implementation

```
Key:   revoked:{jti}
Value: "1"
TTL:   set to the token's remaining lifetime (exp - now)
```

Every service validates incoming tokens against the blocklist on each request. The blocklist lookup is a single Redis `GET` — sub-millisecond at scale. Tokens past their `exp` are never inserted (the TTL handles natural expiry).

For service credentials, revocation is handled by issuing a new credential and removing the old one from the JWKS/introspection endpoint.

---

## Authorization

### Role taxonomy

The STS defines roles as coarse-grained claims. Fine-grained resource checks (e.g. "can this user see this specific order") are the responsibility of the downstream service.

| Role | Description |
|---|---|
| `customer` | End user placing and tracking orders |
| `warehouse_operator` | Staff managing inventory and dispatch at a facility |
| `drone_operator` | Certified personnel monitoring and overriding drone flight |
| `fleet_manager` | Manages drone registry, maintenance scheduling |
| `regulatory_auditor` | Read-only access to compliance and audit data |
| `support_agent` | Limited read access for customer service tooling |
| `admin` | Full platform access; requires hardware MFA |
| `service` | Machine identity; no human roles |

Roles are encoded in the `roles` claim array. A user may hold multiple roles. Role assignment is managed by `admin` accounts and stored in the STS database.

### OAuth2 scope matrix

Scopes limit what a bearer token is permitted to do, independent of role. Service credentials are issued with the minimum scope required for their function.

| Scope | Description | Granted to |
|---|---|---|
| `orders:read` | Read order state | customer, support_agent, fulfillment-orchestrator |
| `orders:write` | Create / mutate orders | customer, order-service |
| `fulfillment:assign` | Assign drones to orders | fulfillment-orchestrator |
| `inventory:reserve` | Reserve stock | fulfillment-orchestrator, inventory-service |
| `telemetry:ingest` | Write drone telemetry | drone-telemetry-hub |
| `telemetry:read` | Read flight history | shipping-service, fleet-manager |
| `fleet:manage` | Register and update drone records | fleet-management-service |
| `notifications:send` | Trigger outbound notifications | notification-engine |
| `audit:write` | Append audit events | all services |
| `audit:read` | Read audit log | regulatory_auditor, admin |
| `sts:admin` | Manage users, roles, credentials | admin only |

A token requesting a scope the credential is not authorized for is rejected at issuance — not at the resource server.

### Policy Decision Point (PDP)

For coarse authorization (role + scope checks), each service validates the JWT claims locally — no runtime call to the STS required. This is intentional: it keeps the hot path fast and the STS out of the critical request path.

For cross-tenant or sensitive operations (e.g. a support agent accessing a specific customer record), services may call a lightweight **OPA (Open Policy Agent)** sidecar that evaluates Rego policies. This is optional per service and not centralized in the STS.

---

## Secret Management

Service credentials and signing keys are **never** stored in environment variables or Kubernetes `Secret` manifests in plain form.

### Secret distribution pipeline

```
Vault (HashiCorp) ──► Vault Agent (sidecar) ──► mounted file / env injection
                  └──► K8s External Secrets Operator ──► K8s Secret (encrypted at rest)
```

- Each service has a dedicated Vault role with a policy granting access only to its own credentials
- Client secrets are **not** human-readable after creation — they are stored as hashed values; the plaintext is shown once at issuance
- Secret rotation is automated: Vault generates a new credential, distributes it, and the old credential is invalidated after a 5-minute overlap window to allow in-flight requests to complete

### Credential rotation schedule

| Credential type | Rotation interval | Trigger |
|---|---|---|
| Service Client Secret | 90 days | Automated via Vault lease |
| JWT signing key (ES256) | 30 days | Automated |
| Admin user passwords | 90 days (enforced) | Policy |
| Database credentials | 30 days | Vault dynamic secrets |

---

## Multi-Tenancy

The STS is multi-tenant. All identity data is partitioned by `tenant_id`.

- Every user, role assignment, federation config, and credential is scoped to a tenant
- Tokens carry `tenant_id` as a non-forgeable claim
- Cross-tenant token use is rejected at validation — a token issued for `ten_A` cannot be used against a resource owned by `ten_B`
- Tenant provisioning and deprovisioning is handled via the `sts:admin` scope; deprovisioning triggers cascade revocation of all tokens and credentials for that tenant

---

## Audit Events

The STS emits a structured audit event for every security-relevant action. Events are published to the `audit.security` topic and consumed by the Audit Log Service for immutable storage.

### Event catalog

| Event | Trigger |
|---|---|
| `auth.login.success` | Successful authentication |
| `auth.login.failure` | Failed login attempt (wrong password, MFA fail) |
| `auth.mfa.challenged` | MFA prompt issued |
| `auth.token.issued` | Any token issuance |
| `auth.token.revoked` | Explicit revocation |
| `auth.token.expired` | Token used after expiry (blocked) |
| `auth.credential.rotated` | Service credential rotated |
| `auth.role.assigned` | Role granted to a user |
| `auth.role.revoked` | Role removed from a user |
| `auth.federation.login` | Federated SSO login |
| `auth.account.locked` | Account locked after failed attempts |
| `auth.account.suspended` | Admin-initiated suspension |

### Event schema (example)

```json
{
  "event_id":   "evt_01J...",
  "event_type": "auth.login.failure",
  "timestamp":  "2024-11-01T12:34:56Z",
  "tenant_id":  "ten_01J...",
  "actor_id":   null,
  "ip_address": "203.0.113.42",
  "user_agent": "Mozilla/5.0 ...",
  "details": {
    "reason": "invalid_password",
    "attempt_count": 3
  }
}
```

---

## Operational Concerns

### Bot & fraud detection

The STS consumes signals from an external fraud detection service (or a lightweight internal heuristic) to apply adaptive authentication:

- Unusual login location or device fingerprint → step-up MFA required
- High-velocity login failures from an IP → temporary IP block + alert
- Credential stuffing patterns → CAPTCHA challenge

The STS does not own the fraud model — it consumes a risk score and acts on thresholds.

### Account lockout policy

| Condition | Action |
|---|---|
| 5 failed login attempts within 10 minutes | Account locked for 15 minutes |
| 10 failed attempts within 1 hour | Account locked; email alert sent to user |
| Admin-level account 3 failed attempts | Immediate lock; alert to security team |

### Break-glass access

For emergency recovery when the STS is degraded or a critical admin account is inaccessible:

- A **break-glass credential** is stored offline in a physical safe and in Vault under a restricted emergency policy
- Use of the break-glass credential triggers an `auth.breakglass.used` audit event and an immediate page to the on-call security team
- Break-glass sessions are time-limited to 2 hours and reviewed post-incident

### High availability

- STS runs as a **minimum 3-replica** Kubernetes deployment across availability zones
- PostgreSQL identity store uses synchronous replication; a read replica services token validation queries
- Redis revocation store uses a Redis Cluster (3 primaries, 3 replicas) — quorum writes required for revocation entries
- JWKS endpoint is cached at the API Gateway (YARP) level; the STS can tolerate brief unavailability without blocking token *validation* on existing tokens

---

## PII & Compliance

The STS stores PII: email addresses, phone numbers (MFA), device fingerprints, and IP addresses in audit logs.

| Data | Retention | Erasure on GDPR request |
|---|---|---|
| Email address | Duration of account | Pseudonymised (hashed) |
| Phone number (MFA) | Duration of account | Deleted |
| Device fingerprint | 90 days rolling | Deleted |
| IP address (audit log) | 1 year | Pseudonymised |
| Audit events | 7 years (regulatory) | Actor ID pseudonymised; event retained |

> **Note:** Audit events are retained for 7 years for regulatory compliance. The `actor_id` field is pseudonymised on erasure request; the event itself is not deleted. This is the standard approach for reconciling GDPR right-to-erasure with immutable audit requirements.

---

## Dependencies

| Dependency | Purpose | Failure mode |
|---|---|---|
| PostgreSQL | Identity, role, and credential storage | STS cannot issue new tokens; existing valid tokens continue to work |
| Redis | Revocation blocklist, session state | Revocation delayed until Redis recovers; new tokens still issued |
| Vault | Secret distribution and rotation | Rotation pauses; existing credentials continue to work until TTL |
| Audit Log Service | Consuming `audit.security` topic | Events buffered in outbox; no data loss |
| Fraud detection service | Risk score signals | Adaptive auth disabled; standard MFA enforced for all logins |
| SMTP / SMS provider | MFA delivery | Fallback to TOTP; SMS OTP temporarily unavailable |

---

## Decision Logs

- [x] **OPA vs in-service policy:** Should all services use an OPA sidecar for authorization, or is JWT claim inspection sufficient for the majority of cases?

  To keep things simple we will start with JWT clam inspection - will add OPA sidecar if necessary in future

- [x] **Refresh token storage:** Should refresh tokens be stored server-side (stateful, revocable instantly) or remain client-side-only (stateless, revocable only via blocklist)?

  Lets keep these stateless to reduce the storage capacity and reduce the cost. 

- [x] **Tenant provisioning workflow:** Is tenant creation self-service (sign-up flow) or admin-initiated only?

    We are not concerned about UI flows at this state of development cycle. Will add self signup capability later. For now make sure only admins can create it. 

- [x] **WebAuthn for customers:** Is hardware key support required for end customers, or only for operator/admin roles?

    No, ideally we would use Azure Entra ID, etc for the user management. The purpose of this service was to reduce external dependency and reduce the cost. 

- [ ] **Rate limiting ownership:** Does the STS enforce login rate limits, or is this delegated to the API Gateway (YARP)?
