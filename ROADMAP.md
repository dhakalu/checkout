# Checkout — Engineering Roadmap

> **Philosophy:** Ship a working slice of the system end-to-end before adding breadth. Each phase produces something deployable and demonstrable. No phase exists solely to lay groundwork — every sprint ends with observable, testable behaviour.

---

## Phase 1 — Walking Skeleton (Weeks 1–4)

**Goal:** A customer can place an order and receive a confirmation. No drones, no async messaging, no saga. Just a working vertical slice that proves the foundation.

### What gets built

- **Identity (STS)** — bare minimum: user registration, login, JWT issuance, refresh token. No MFA, no federation yet. OpenIddict wired up with PostgreSQL.
- **API Gateway (YARP)** — single entry point, JWT validation at the edge, routes to Order Service and STS. TLS via dev cert locally.
- **Order & Transaction** — accept an order payload, validate it, persist to PostgreSQL, return a confirmation. No payment, no fulfillment — synchronous response only.
- **Customer / Profile** — store name, email, and one delivery address. Used by Order Service to validate the recipient.
- **.NET Aspire scaffold** — service discovery, local orchestration, health checks, and a working `dotnet run` experience across all services.
- **Docker Compose** — all services runnable locally with a single command.

### Definition of done

- `POST /orders` returns a confirmed order ID
- JWT is required; requests without a valid token are rejected at the gateway
- All services have health check endpoints
- CI pipeline runs on every PR (build + unit tests)

### What is deliberately excluded

Payment, async messaging, drones, fulfillment, notifications. These are not deferred forever — they are deferred until the foundation is solid.

---

## Phase 2 — Payment & Real Fulfillment (Weeks 5–8)

**Goal:** Orders actually charge a card and enter a fulfillment workflow. Introduce the message bus and the outbox pattern.

### What gets built

- **Payment / Billing** — integrate a payment gateway (Stripe recommended for dev). Charge on order placement. Issue refunds via API. Circuit breaker via Polly around all external calls.
- **RabbitMQ + MassTransit** — broker running in Docker Compose. First topics wired: `order.created`, `payment.captured`, `payment.failed`.
- **Transactional Outbox** — implemented in Order & Transaction. Orders are never lost even if the broker is temporarily unavailable.
- **Fulfillment Orchestrator** — stub implementation. Subscribes to `order.created`, advances through a simple two-step saga (reserve inventory → confirm). No drones yet — fulfillment is manual/simulated.
- **Inventory Service** — track stock for a small set of test products. Reserve on order, release on cancellation. Optimistic concurrency to prevent oversell.
- **Dead Letter Queue** — configured for `order.created`, `payment.captured`, `saga.compensate`. Alerts on DLQ depth > 0.
- **Notification Engine** — subscribe to `payment.captured` and `payment.failed`, send email via SMTP (Mailpit locally). Single channel only.

### Definition of done

- A placed order charges the card and sends a confirmation email
- A failed payment sends a failure email and does not create a fulfillment record
- Killing the broker mid-flow does not lose the order (outbox test)
- Saga compensates correctly when inventory reservation fails

---

## Phase 3 — Drone Foundation (Weeks 9–13)

**Goal:** The system knows about drones. A delivery can be assigned to a drone and tracked through to completion — simulated flight, not real hardware.

### What gets built

- **Fleet Management** — drone registry: register a drone, set availability, battery level, home dock. Exposes gRPC API for availability queries.
- **Fulfillment Orchestrator** — extend saga: after inventory reservation, query Fleet Management for an available drone, publish `drone.assigned`. Add compensating transactions for drone assignment failure.
- **Drone Telemetry Hub (Go)** — WebSocket server that accepts simulated GPS pings. Buffer to Redis. Publish `telemetry.ping` to RabbitMQ. No Kafka yet — volume is simulated and low.
- **Shipping & Tracking** — subscribe to `drone.assigned` and `telemetry.ping`. Compute ETA. Expose a `GET /shipments/{id}` endpoint returning current location and ETA.
- **New topics wired:** `drone.assigned`, `drone.departed`, `drone.returned`, `drone.fault`, `delivery.completed`, `delivery.failed`.
- **Drone simulator** — a small CLI tool or background worker that produces realistic GPS ping sequences for test drones. Essential for development without real hardware.

### Definition of done

- Full order-to-delivery flow works end-to-end with simulated drones
- Customers can poll for real-time location and ETA
- `drone.fault` mid-delivery triggers saga compensation and reassigns a new drone
- Delivery completion triggers payment capture and a delivery confirmation email

---

## Phase 4 — Observability & Hardening (Weeks 14–16)

**Goal:** The system is production-observable. You can debug a failure without SSH-ing into a pod.

### What gets built

- **OpenTelemetry** — traces wired across all services. Every request carries a `trace-id` from gateway to database. Jaeger running in Docker Compose and Kubernetes.
- **Prometheus + Grafana** — metrics for: request latency per service, message broker lag per topic, DLQ depth, saga completion rate, drone assignment success rate.
- **Structured logging** — all services emit JSON logs with `trace-id`, `service`, `environment`, and `correlation-id`. No `Console.WriteLine`.
- **Kubernetes manifests** — proper `Deployment`, `Service`, `ConfigMap`, `HorizontalPodAutoscaler` for each service. Liveness and readiness probes on all pods.
- **Secrets management** — move all secrets out of environment variables into Kubernetes Secrets (sealed) or Vault. No plaintext credentials in manifests or source control.
- **Load testing** — k6 scripts for the critical paths: order placement, telemetry ingestion, simultaneous drone assignments. Establish baseline performance numbers.

### Definition of done

- A saga failure is traceable end-to-end in Jaeger with no gaps
- Grafana dashboard shows broker lag, DLQ depth, and p95 latency for each service
- All secrets are out of source control
- Load test baseline documented in the repo

---

## Phase 5 — Real-Time & Geospatial (Weeks 17–21)

**Goal:** Customers see their drone move on a map. The system routes around obstacles.

### What gets built

- **Geospatial / Routing Service (Go)** — integrate a maps API (Google Maps Platform or HERE). Compute optimal drone flight paths. Expose gRPC endpoint for route queries by Fulfillment Orchestrator.
- **No-fly zone enforcement** — static no-fly zones (airports, restricted areas) loaded as GeoJSON. Route computation rejects or reroutes paths that intersect a zone.
- **Telemetry Hub → Kafka migration** — replace RabbitMQ for `telemetry.ping` and `telemetry.ping.batch` with a Kafka cluster. Update Shipping, Geospatial, and Regulatory consumers to use Kafka offsets.
- **SignalR real-time tracking** — Shipping & Tracking service pushes live location updates to connected browser clients over WebSocket. Redis backplane for multi-pod SignalR.
- **`delivery.eta.updated` topic** — published by Shipping when ETA changes by more than a threshold. Notification Engine sends a push/SMS update.
- **Geofence events** — `geofence.entered` and `geofence.exited` published as drone approaches the delivery address. Triggers a "your delivery is 2 minutes away" notification.

### Definition of done

- Flight paths avoid known no-fly zones
- A browser client receives live drone position updates over WebSocket
- ETA update notification fires when estimated arrival changes significantly
- Telemetry topics running on Kafka with correct consumer group offsets

---

## Phase 6 — Regulatory & Safety (Weeks 22–25)

**Goal:** The system can respond to airspace events and is safe to operate in a regulated environment.

### What gets built

- **Regulatory / Airspace Service (Go)** — subscribe to `telemetry.ping`. Evaluate each ping against dynamic no-fly zones and weather restrictions. Publish `airspace.violation`, `airspace.hold.issued`, `airspace.hold.lifted`.
- **Emergency recall** — `drone.emergency.recall` topic wired. Fulfillment Orchestrator halts the saga and triggers compensation. Fleet Management commands the drone home.
- **Weather hold** — integrate a weather API. When severe weather is detected in a delivery zone, issue `airspace.hold.issued`. Scheduler pauses dispatch for affected zones. `airspace.hold.lifted` resumes the queue.
- **Regulatory audit trail** — all airspace events flow to `audit.business`. Audit Log Service persists them with timestamp and drone ID.
- **Scheduler / Optimizer** — basic implementation: batch nearby orders to the same drone, respect delivery windows, pause dispatch during airspace holds.

### Definition of done

- An airspace violation mid-flight triggers recall and saga compensation within 5 seconds of detection
- Weather hold pauses dispatch and resumes automatically when hold lifts
- All airspace events appear in the immutable audit log
- Scheduler batches orders in the same postcode to the same drone where possible

---

## Phase 7 — Security Hardening (Weeks 26–28)

**Goal:** The system is Zero Trust end-to-end. The STS is production-grade.

### What gets built

- **MFA** — TOTP via authenticator app for all human accounts. SMS fallback for customers.
- **OAuth2 scope enforcement** — every service validates that the incoming token carries the correct scope for the operation, not just a valid JWT.
- **OPA sidecar** — deploy OPA alongside Fleet Management and Regulatory services. Move cross-dimensional authorization rules (operator region checks, tenant isolation) out of service code into Rego policies.
- **Vault integration** — HashiCorp Vault for secret distribution and automated rotation. All service client secrets rotated on 90-day leases.
- **Audit Log Service** — full implementation. Immutable append-only store. Separate `audit.security` vhost in RabbitMQ with restricted ACLs.
- **Penetration test** — run OWASP ZAP against the public API surface. Fix all critical and high findings before phase sign-off.
- **Rate limiting** — enforce per-client rate limits at the API Gateway for all public endpoints.

### Definition of done

- All service-to-service calls use Client Credentials with correct scopes
- A token issued for Service A cannot be used against Service B's protected endpoints
- All secrets rotate automatically with zero downtime
- Audit log is append-only and cannot be modified by any service credential

---

## Phase 8 — Scale & Multi-Region (Weeks 29–34)

**Goal:** The system handles real load and survives a regional failure.

### What gets built

- **Horizontal scaling** — HPA configured for Telemetry Hub, Order Service, and Fulfillment Orchestrator based on CPU and broker lag metrics.
- **Read replicas** — PostgreSQL read replicas for Customer/Profile and Inventory read paths. EF Core configured to route reads to replicas.
- **Multi-region inventory** — Inventory Service made region-aware. Orders fulfilled from the nearest warehouse with stock.
- **Redis Cluster** — upgrade from single Redis to Redis Cluster (3 primary, 3 replica) for revocation store and SignalR backplane.
- **Chaos engineering** — introduce Chaos Monkey-style tests: kill a random pod, partition the broker, saturate the telemetry topic. Verify the system self-heals within defined SLOs.
- **SLO definition and alerting** — define and instrument: order placement p99 < 500ms, saga completion rate > 99.5%, telemetry lag < 2s, DLQ depth = 0 for > 1 hour triggers PagerDuty.

### Definition of done

- System sustains 500 concurrent orders/minute without degradation
- Killing the Telemetry Hub pod recovers within 30 seconds with no data loss
- Multi-region inventory routes orders to correct warehouse
- All SLOs are instrumented and alerting is active

---

## Backlog (Post Phase 8)

These are real features deferred until the core system is stable and proven at scale:

- **Enterprise SSO / SAML federation** — B2B customers bring their own IdP
- **Drone hardware integration** — replace the simulator with real drone SDK (DJI, Autel, or custom)
- **ML-based ETA prediction** — replace heuristic ETA with a model trained on historical delivery data
- **Customer-facing tracking page** — public URL with live drone map, no login required
- **Admin dashboard** — internal tooling for fleet managers and support agents
- **Multi-tenancy** — full tenant isolation for white-label B2B customers
- **GraphQL API** — for the customer-facing tracking and order history surfaces
- **gRPC migration** — move high-frequency internal calls (Fulfillment → Fleet, Fulfillment → Geospatial) from REST to gRPC

---

## Summary

| Phase | Focus | Weeks | Key deliverable |
|---|---|---|---|
| 1 | Walking skeleton | 1–4 | Orders placed and confirmed, auth working |
| 2 | Payment & fulfillment | 5–8 | End-to-end order with card charge and saga |
| 3 | Drone foundation | 9–13 | Simulated drone delivery, tracking API |
| 4 | Observability | 14–16 | Traces, metrics, Kubernetes-ready |
| 5 | Real-time & geospatial | 17–21 | Live map tracking, route planning, Kafka |
| 6 | Regulatory & safety | 22–25 | Airspace compliance, emergency recall |
| 7 | Security hardening | 26–28 | Zero Trust, MFA, OPA, Vault, audit log |
| 8 | Scale & multi-region | 29–34 | Production load, chaos testing, SLOs |