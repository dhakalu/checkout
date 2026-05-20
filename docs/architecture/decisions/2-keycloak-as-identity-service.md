# ADR-001: Replace custom Identity (STS) service with Keycloak

| Field | Value |
|---|---|
| **ID** | ADR-001 |
| **Date** | 2026-05-18 |
| **Status** | Accepted |
| **Deciders** | Platform Engineering |
| **Domain** | Identity & Access Management |

---

## Context

The original Checkout architecture included a bespoke Identity / STS service built on **OpenIddict** responsible for:

- OIDC / OAuth2 token issuance and validation
- JWT lifecycle management (issuance, refresh, revocation)
- Role-based access control (RBAC)
- Token revocation via a Redis blocklist
- Secret management via HashiCorp Vault integration
- Emission of `audit.security` events for regulatory compliance (7-year retention)

Building and operating this service requires sustained investment in security-critical code that is not core to the business domain. The Checkout platform's differentiated value lies in its fulfillment saga orchestration, real-time drone telemetry pipeline, and geospatial routing — not in identity infrastructure. Maintaining a custom STS creates ongoing risk: any gap in implementation (token revocation edge cases, PKCE handling, key rotation) is a potential security incident.

Keycloak is a mature, battle-tested open-source identity platform that implements the same OIDC / OAuth2 / JWT standards and is widely deployed in enterprise Kubernetes environments.

---

## Decision

**Replace the custom OpenIddict-based Identity (STS) service with a self-hosted Keycloak instance.**

Keycloak will be treated as platform infrastructure — deployed and configured, not built. It owns the full authentication and coarse RBAC surface. All other architectural decisions (API Gateway JWT validation, OPA fine-grained policy, audit event emission) remain structurally unchanged.

---

## Consequences

### Positive

- **Eliminates a high-risk custom build.** Token issuance, revocation, PKCE, key rotation, and OIDC discovery are handled by a hardened, community-audited implementation. Security vulnerabilities in this layer are patched upstream.
- **Reduces time-to-delivery.** Engineering focus shifts to fulfillment saga, drone telemetry, and geospatial routing — the actual product differentiators.
- **Feature completeness out of the box.** Admin UI, user federation (LDAP/AD), social login, MFA, session management, and client scope management are available without custom code.
- **Operational familiarity.** Keycloak has a large ecosystem of Kubernetes operators, Helm charts, monitoring exporters, and runbook documentation. The on-call team inherits a known operational model.
- **Per-service OAuth2 scopes.** Client scopes (e.g. `fulfillment:write`, `drone:telemetry:read`, `inventory:reserve`) can be defined in Keycloak and mapped into JWTs, giving OPA concrete claims to evaluate without any custom token logic.

### Negative / Trade-offs

- **Keycloak is heavyweight infrastructure.** It requires its own dedicated PostgreSQL schema and sufficient memory headroom (minimum ~512MB per pod, 1GB+ recommended for production). This adds to cluster resource requirements.
- **Operational ownership transfers to the platform team.** Realm configuration, client registration, and upgrade management become ongoing ops tasks. Keycloak major version upgrades (e.g. v24 → v25) occasionally require schema migrations.
- **Less code-level control.** Customising token claims beyond standard mappers requires writing Keycloak SPI extensions in Java. For most use cases, built-in mappers are sufficient — but bespoke claim logic cannot be expressed in C#.
- **`audit.security` requires a thin adapter.** Keycloak does not natively publish to RabbitMQ. A lightweight Keycloak Event Listener SPI provider (or community plugin) must be configured to forward authentication events onto the `audit.security` topic. The Audit Log Service consumer is unchanged.

### Neutral / Notes

- HashiCorp Vault is retained for non-identity secrets (database credentials, third-party API keys, inter-service certificates). Keycloak manages its own realm and client secrets internally and does not require Vault integration.
- The API Gateway (YARP) requires only a configuration change: the JWT bearer middleware `Authority` and `Audience` values point to the Keycloak realm endpoint instead of the former STS. No structural gateway changes are needed.
- OPA continues to handle fine-grained, cross-dimensional authorization. Keycloak handles authentication and coarse RBAC; OPA evaluates Rego policies against the JWT claims Keycloak issues. The boundary between the two is clean and unchanged.
- The `audit.security` topic contract (schema, retention policy, consumer) is unchanged. Only the publisher mechanism changes.

---

## Alternatives Considered

### 1. Retain the custom OpenIddict STS

**Rejected.** Requires building and maintaining security-critical infrastructure that is not a product differentiator. Any implementation gap (revocation, key rotation, PKCE edge case) is a direct security risk. Ongoing cost is high relative to benefit.

### 2. Use a managed identity provider (Auth0, Azure AD B2C, Okta)

**Deferred.** Managed providers eliminate operational burden entirely and are a credible long-term option. Rejected at this stage because: (a) the system is self-hosted and the team wants full control over token claims and realm configuration without per-MAU pricing; (b) managed providers introduce an external dependency on a third-party SLA for every authenticated request. Can be revisited if operational Keycloak burden proves unacceptable.

### 3. Use ASP.NET Core's built-in data protection + OpenIddict with managed infrastructure

**Rejected.** Retains the custom build problem. The core issue is not the framework — it is the sustained engineering investment in non-differentiating security infrastructure.

---

## Implementation Notes

| Concern | Approach |
|---|---|
| Deployment | Keycloak Operator on Kubernetes; dedicated PostgreSQL schema in the platform DB cluster |
| Realm setup | Single realm per environment (`checkout-dev`, `checkout-prod`); one confidential client per service that issues tokens |
| Client scopes | Define per-service scopes (`fulfillment:write`, `drone:telemetry:read`, etc.); map into JWT via protocol mappers |
| API Gateway | Update YARP JWT bearer `Authority` to Keycloak realm URL; `Audience` to the gateway client ID |
| Audit bridge | Implement or configure a Keycloak Event Listener SPI provider to publish login, token issuance, and credential-rotation events to the `audit.security` RabbitMQ topic |
| Key rotation | Use Keycloak's built-in RSA key provider; configure rotation schedule in realm settings |
| Monitoring | Deploy `keycloak-metrics-spi` or use the built-in metrics endpoint; scrape with Prometheus; add to existing Grafana stack |

---

## Links

- [Keycloak documentation](https://www.keycloak.org/documentation)
- [Keycloak Operator for Kubernetes](https://www.keycloak.org/operator/installation)
- [Checkout architecture overview](./README.md)
- Related services: API Gateway (YARP), OPA policy engine, Audit Log Service