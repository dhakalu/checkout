# Checkout: Autonomous Logistics & Drone Orchestration Platform

### 🚀 Overview
**Checkout** is an enterprise-grade distributed system designed to handle high-velocity commerce and autonomous fulfillment. While the name is simple, the engine is a complex **global logistics orchestrator** that manages real-time drone telemetry, multi-region inventory, and automated delivery lifecycles.

This project demonstrates **Principal-level software engineering** by tackling the "hard" problems of distributed systems: real-time data at scale, eventual consistency, and self-healing infrastructure.

---

### 🏗️ Architectural Bounded Contexts - Services Reference
The system is partitioned into specialized microservices using **Domain-Driven Design (DDD)** and an **Event-Driven Architecture (EDA)**:

| Service | Primary responsibility | Key patterns / tech | Runtime | External comms | Internal comms | Independently deployed | Status |
|---|---|---|---|---|---|---|---|
| **API Gateway (YARP)** | TLS termination, JWT validation at the edge, rate limiting, routing to downstream services | YARP | .NET | REST (public) | Routes to downstream | ✅ Yes | ✅ Defined |
| **Identity (STS)** | Authentication, JWT issuance, token revocation, RBAC, secret management, audit event emission | OpenIddict, OIDC, OAuth2, Redis blocklist, Vault | .NET | REST / OIDC | REST (JWKS, introspection) | ✅ Yes | ✅ Defined |
| **Order & Transaction** | Hot-path checkout: accept orders, process payments, initiate the fulfillment saga, implement transactional outbox | MassTransit, Outbox Pattern, Polly | .NET | REST (via gateway) | Publishes events | ✅ Yes | ✅ Defined |
| **Fulfillment Orchestrator** | Saga coordinator: assign drones, sequence compensating transactions on failure, own the delivery state machine | Saga Pattern, MassTransit, state machine | .NET | None | Consumes + publishes events | ✅ Yes | ✅ Defined |
| **Inventory Service** | Track stock levels per warehouse, handle reservation and release during saga, prevent oversell across concurrent orders | EF Core, optimistic concurrency, Outbox | .NET | None | gRPC / REST (sync queries) | ✅ Yes | ❌ Missing |
| **Payment / Billing** | Wrap 3rd-party payment gateway, own charge/hold/capture/refund lifecycle, expose a clean internal API to the saga | Polly (circuit breaker), idempotency keys | .NET | None | REST (wraps 3rd-party gateway) | ✅ Yes | ❌ Missing |
| **Customer / Profile** | Store delivery addresses, contact preferences, notification channels, and account history | EF Core, read/write split | .NET | REST (via gateway) | REST | ✅ Yes | ❌ Missing |
| **Drone Telemetry Hub** | High-throughput ingestion of real-time GPS pings from thousands of active drones; buffer to Redis; fan out to subscribers | SignalR, Redis, WebSockets | Go | WebSocket (drones) | Publishes to Redis / MassTransit | ✅ Yes | ✅ Defined |
| **Fleet Management** | Own the drone registry: hardware IDs, battery state, maintenance status, home dock, and availability for assignment | EF Core, scheduled jobs | .NET | None | gRPC / REST | ✅ Yes | ❌ Missing |
| **Shipping & Tracking** | Source of truth for shipment state; bridge telemetry to order; compute real-time ETAs; evaluate geofence boundaries | Redis, time-series reads, geofencing | .NET | REST (via gateway) | Consumes telemetry events | ✅ Yes | ✅ Defined |
| **Geospatial / Routing** | Compute optimal drone flight paths, enforce no-fly zones, integrate with maps API, reroute around dynamic obstacles | H3 / S2, Maps API, Polly | Go | None | gRPC (high call volume) | ✅ Yes | ❌ Missing |
| **Regulatory / Airspace** | Enforce FAA and local authority rules: no-fly zones, weather holds, emergency recall; can halt a delivery mid-saga | External authority APIs, event triggers | Go | None | Consumes + publishes events | ✅ Yes | ❌ Missing |
| **Scheduler / Optimizer** | Determine dispatch timing, batch nearby orders to the same drone, respect delivery windows and charge schedules | Constraint solver, MassTransit | .NET | None | Publishes dispatch commands | ✅ Yes | ❌ Missing |
| **Notification Engine** | Decoupled multi-channel alerting (email, SMS, push) driven entirely by event subscriptions | MassTransit, SMTP/SMS/Push providers | .NET | None | Consumes events only | ✅ Yes | ✅ Defined |
| **Audit Log Service** | Consume events from all services and write to an immutable, append-only store for regulatory and security review | MassTransit consumer, append-only DB | .NET | None | Consumes events only | ✅ Yes | ❌ Missing |
| **OPA (policy engine)** | Evaluate fine-grained authorization policies (Rego) for services that need cross-dimensional access control decisions | Pre-built image, Rego policies in repo | 3rd-party | None | localhost HTTP (sidecar) | ⚠️ Sidecar | ✅ Defined |

 
### 📨 Event-Driven Architecture — Topic Catalog
 
All asynchronous communication is handled via **MassTransit / RabbitMQ**. Services communicate by publishing and subscribing to named topics — no direct service-to-service calls for non-critical paths. Every topic that carries financial or safety consequences requires a configured **Dead Letter Queue (DLQ)**.
 
#### Domain overview
 
| Domain | Topics | Description |
|---|---|---|
| Order | 5 | Payment lifecycle and order state changes |
| Fulfillment | 7 | Saga coordination, inventory reservation, dispatch scheduling |
| Drone / Telemetry | 6 | Real-time flight data, drone state transitions, fault reporting |
| Shipping | 5 | Delivery outcomes, ETA updates, geofence events |
| Fleet / Airspace | 4 | Regulatory holds, airspace violations, emergency recalls |
| Platform | 3 | Notifications, security audit trail, business audit trail |
 
---
 
#### Order topics
 
| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `order.created` | Order & Transaction | Fulfillment Orchestrator, Inventory, Audit Log | ✅ |
| `order.cancelled` | Order & Transaction | Fulfillment Orchestrator, Inventory, Notification, Audit Log | |
| `order.refunded` | Payment / Billing | Order & Transaction, Notification, Audit Log | ✅ |
| `payment.captured` | Payment / Billing | Order & Transaction, Audit Log | ✅ |
| `payment.failed` | Payment / Billing | Order & Transaction, Notification, Audit Log | |
 
#### Fulfillment topics
 
| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `saga.step.completed` | Fulfillment Orchestrator | Fulfillment Orchestrator (self — next step) | |
| `saga.step.failed` | Any saga participant | Fulfillment Orchestrator | ✅ |
| `saga.compensate` | Fulfillment Orchestrator | Inventory, Payment, Fleet Management | ✅ |
| `inventory.reserved` | Inventory Service | Fulfillment Orchestrator | |
| `inventory.reservation.failed` | Inventory Service | Fulfillment Orchestrator, Notification | |
| `inventory.released` | Inventory Service | Fulfillment Orchestrator, Audit Log | |
| `dispatch.scheduled` | Scheduler / Optimizer | Fulfillment Orchestrator, Fleet Management | |
 
#### Drone / Telemetry topics
 
| Topic | Publisher | Subscribers | DLQ | Notes |
|---|---|---|---|---|
| `drone.assigned` | Fulfillment Orchestrator | Fleet Management, Shipping & Tracking, Audit Log | | |
| `drone.departed` | Fleet Management | Shipping & Tracking, Notification, Audit Log | | |
| `drone.returned` | Fleet Management | Scheduler / Optimizer, Audit Log | | |
| `drone.fault` | Fleet Management / Telemetry Hub | Fulfillment Orchestrator, Regulatory, Audit Log | ✅ | |
| `telemetry.ping` | Drone Telemetry Hub | Shipping & Tracking, Geospatial, Regulatory | | High frequency — short retention, consumers must tolerate gaps |
| `telemetry.ping.batch` | Drone Telemetry Hub | Time-series store | | Buffered flush from Redis — not one message per ping |
 
#### Shipping topics
 
| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `delivery.completed` | Shipping & Tracking | Order & Transaction, Payment, Notification, Audit Log | ✅ |
| `delivery.failed` | Shipping & Tracking | Fulfillment Orchestrator, Notification, Audit Log | ✅ |
| `delivery.eta.updated` | Shipping & Tracking | Notification Engine | |
| `geofence.entered` | Shipping & Tracking | Notification Engine, Regulatory | |
| `geofence.exited` | Shipping & Tracking | Regulatory, Audit Log | |
 
#### Fleet / Airspace topics
 
| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `airspace.violation` | Regulatory / Airspace | Fulfillment Orchestrator, Fleet Management, Audit Log | ✅ |
| `airspace.hold.issued` | Regulatory / Airspace | Fulfillment Orchestrator, Scheduler, Fleet Management | |
| `airspace.hold.lifted` | Regulatory / Airspace | Scheduler / Optimizer, Fulfillment Orchestrator | |
| `drone.emergency.recall` | Regulatory / Airspace | Fleet Management, Fulfillment Orchestrator, Audit Log | ✅ |
 
#### Platform topics
 
| Topic | Publisher | Subscribers | Notes |
|---|---|---|---|
| `notification.trigger` | Multiple services | Notification Engine | Generic envelope — channel resolved by Notification Engine |
| `audit.security` | Identity (STS) | Audit Log Service | 7-year retention — regulatory requirement |
| `audit.business` | All services | Audit Log Service | Covers order, delivery, and role-assignment events |
 
---
 
#### Design notes
 
**Dead Letter Queues** are mandatory on any topic where an unprocessable message has financial or safety consequences — a failed `saga.compensate` leaves a customer charged for an undelivered order; a failed `drone.emergency.recall` is a regulatory incident. All DLQ'd messages trigger an alert to the on-call queue and are replayed manually after root-cause investigation.
 
**`telemetry.ping`** is the only high-frequency topic in the system. It carries GPS coordinates from every active drone on a sub-second interval. Retention is set in hours rather than days. All consumers on this topic are designed to handle or skip missing messages gracefully — a gap in telemetry does not constitute a saga failure.
 
**`telemetry.ping.batch`** exists to decouple the write path to the time-series store from the real-time fan-out. The Drone Telemetry Hub buffers pings in Redis and flushes batches on a rolling interval, keeping the time-series database write volume predictable and independent of drone connection count.
 
**Saga topics** (`saga.step.completed`, `saga.step.failed`) are internal to the Fulfillment Orchestrator's state machine. Whether these are placed on the shared broker or handled via an in-process bus is an implementation decision — the key constraint is that `saga.step.failed` must be durable and must never be silently dropped.
 
**Audit topics are split by concern.** `audit.security` is owned exclusively by the Identity Service and carries authentication events (logins, token issuance, credential rotation) subject to a 7-year regulatory retention policy. `audit.business` carries all other domain events and can be retained on a shorter cycle. Both feed the Audit Log Service as the single consumer writing to the immutable store.
 

---


### 💻 Tech Stack
*   **Runtime:** .NET 8/9 (C#)
*   **Messaging:** MassTransit / RabbitMQ (Asynchronous Pub/Sub)
*   **Persistence:** PostgreSQL (Transactional), Redis (State/Caching)
*   **Real-time:** SignalR / WebSockets
*   **Security:** OpenIddict (OIDC), JWT, OAuth2
*   **Infrastructure:** Docker, Kubernetes, YARP (API Gateway), .NET Aspire
*   **Observability:** OpenTelemetry, Jaeger, Prometheus, Grafana

---

### 🛠️ Getting Started
*(Add instructions here for `dotnet aspire` or `docker-compose up` once your environment is set up.)*
