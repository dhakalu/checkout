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
| **Identity(Keycloak)** | Authentication, JWT issuance, token revocation, RBAC, secret management, audit event emission | Keycloak | .NET | REST / OIDC | REST (JWKS, introspection) | ✅ Yes | ✅ Defined |
| **Order & Transaction** | Hot-path checkout: accept orders, process payments, initiate the fulfillment saga, implement transactional outbox | MassTransit, Outbox Pattern, Polly | .NET | REST (via gateway) | Publishes events | ✅ Yes | ✅ Defined |
| **Fulfillment Orchestrator** | Durable fulfillment workflow: consumes `order.created` to start a Temporal Workflow; Activities call downstream services via gRPC; MassTransit consumer bridges bus events to Temporal signals | Temporal Workflow, Temporal .NET SDK, MassTransit (consumer only) | .NET | None | Consumes `order.created` + `dispatch.scheduled` via MassTransit → signals Temporal; Activities call downstream via gRPC | ✅ Yes | ✅ Defined |
| **Inventory Service** | Track stock levels per warehouse, handle reservation and release during fulfillment workflow, prevent oversell across concurrent orders | EF Core, optimistic concurrency | .NET | None | gRPC (called by Temporal Activities) | ✅ Yes | ❌ Missing |
| **Payment / Billing** | Wrap 3rd-party payment gateway, own charge/hold/capture/refund lifecycle, expose a clean internal API to the fulfillment workflow | Polly (circuit breaker), idempotency keys | .NET | None | gRPC (called by Temporal Activities); publishes `payment.failed` on compensation | ✅ Yes | ❌ Missing |
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

---

### 🔄 Fulfillment Orchestration — Temporal

The Fulfillment Orchestrator uses **Temporal** for durable workflow execution (ADR-6). The state machine, retry logic, and compensating transactions live inside a Temporal Workflow; MassTransit is used only at the edges to receive bus events and bridge them into Temporal signals.

#### Workflow lifecycle

1. A MassTransit consumer inside the Fulfillment Orchestrator receives `order.created` from RabbitMQ and calls `temporal.StartWorkflowAsync(...)` to begin a new `FulfillmentWorkflow` instance.
2. The workflow executes a sequence of **Activities**, each of which calls a downstream service via gRPC:
   - `ReserveInventoryActivity` → Inventory Service gRPC
   - `ChargePaymentActivity` → Payment / Billing gRPC
   - `AssignDroneActivity` → Fleet Management gRPC
   - `AwaitDispatchSignal` → workflow pauses; resumes when a `dispatch.scheduled` signal arrives (see below)
   - `TrackDeliveryActivity` → monitors delivery outcome via Shipping & Tracking events
3. On any Activity failure, Temporal drives compensation in reverse order:
   - `UnassignDroneActivity` → Fleet Management gRPC
   - `RefundPaymentActivity` → Payment / Billing gRPC; also **publishes `payment.failed`** to the bus so downstream subscribers (Notification, Audit Log, Order & Transaction) can react
   - `ReleaseInventoryActivity` → Inventory Service gRPC

#### Bridging `dispatch.scheduled`

The Scheduler / Optimizer publishes `dispatch.scheduled` to RabbitMQ. A thin MassTransit consumer inside the Fulfillment Orchestrator receives it and calls `workflow.SignalAsync("DispatchReady", ...)` to resume the waiting workflow. This keeps Temporal internal to the Fulfillment Orchestrator, preserves the bus event for Fleet Management and Audit Log, and avoids coupling the Scheduler to Temporal's API.

```
Scheduler ──publishes──▶ dispatch.scheduled (RabbitMQ)
                                │
              ┌─────────────────┤ also consumed by Fleet Management, Audit Log
              │
Fulfillment Orchestrator (MassTransit consumer)
              │
              └──SignalAsync("DispatchReady")──▶ FulfillmentWorkflow (Temporal)
```

---

### 📨 Event-Driven Architecture — Topic Catalog

All asynchronous communication is handled via **MassTransit / Service Bus (RabbitMQ locally)**. Services communicate by publishing and subscribing to named topics — no direct service-to-service calls for non-critical paths. Every topic that carries financial or safety consequences requires a configured **Dead Letter Queue (DLQ)**.

#### Domain overview

| Domain | Topics | Description |
|---|---|---|
| Order | 5 | Payment lifecycle and order state changes |
| Fulfillment | 1 | Dispatch scheduling (saga coordination is now internal to Temporal) |
| Drone / Telemetry | 6 | Real-time flight data, drone state transitions, fault reporting |
| Shipping | 5 | Delivery outcomes, ETA updates, geofence events |
| Fleet / Airspace | 4 | Regulatory holds, airspace violations, emergency recalls |
| Platform | 3 | Notifications, security audit trail, business audit trail |

---

#### Order topics

| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `order.created` | Order & Transaction | Fulfillment Orchestrator, Audit Log | ✅ |
| `order.cancelled` | Order & Transaction | Fulfillment Orchestrator, Inventory, Notification, Audit Log | |
| `order.refunded` | Payment / Billing | Order & Transaction, Notification, Audit Log | ✅ |
| `payment.captured` | Payment / Billing | Order & Transaction, Audit Log | ✅ |
| `payment.failed` | Payment / Billing · Fulfillment Orchestrator (compensation Activity) | Order & Transaction, Notification, Audit Log | |

#### Fulfillment topics

| Topic | Publisher | Subscribers | DLQ |
|---|---|---|---|
| `dispatch.scheduled` | Scheduler / Optimizer | Fulfillment Orchestrator (bridges to Temporal signal), Fleet Management, Audit Log | |

> **Note:** The former saga coordination topics (`saga.step.completed`, `saga.step.failed`, `saga.compensate`) and inventory reservation topics (`inventory.reserved`, `inventory.reservation.failed`, `inventory.released`) have been removed. Temporal's durable execution and Activity retry model replaces all of them. Inventory reservation and release are now synchronous gRPC calls made by Temporal Activities.

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

**Temporal replaces saga choreography.** The Fulfillment Orchestrator is a Temporal Worker running a durable `FulfillmentWorkflow`. Step sequencing, retry, timeout, and compensation are all handled by Temporal's execution engine rather than the message bus. The bus is used only at the edges: to receive `order.created` (workflow start) and `dispatch.scheduled` (mid-workflow signal).

**Compensation publishes selectively.** Most compensating Activities call downstream services directly via gRPC and produce no bus event — the call either succeeds (with Temporal retrying on failure) or the workflow escalates to a manual intervention state. The exception is `RefundPaymentActivity`, which additionally publishes `payment.failed` so that Notification, Audit Log, and Order & Transaction can react without polling or tight coupling to Temporal.

**Dead Letter Queues** are mandatory on any topic where an unprocessable message has financial or safety consequences — a failed `drone.emergency.recall` is a regulatory incident. All DLQ'd messages trigger an alert to the on-call queue and are replayed manually after root-cause investigation.

**`telemetry.ping`** is the only high-frequency topic in the system. It carries GPS coordinates from every active drone on a sub-second interval. Retention is set in hours rather than days. All consumers on this topic are designed to handle or skip missing messages gracefully — a gap in telemetry does not constitute a workflow failure.

**`telemetry.ping.batch`** exists to decouple the write path to the time-series store from the real-time fan-out. The Drone Telemetry Hub buffers pings in Redis and flushes batches on a rolling interval, keeping the time-series database write volume predictable and independent of drone connection count.

**Audit topics are split by concern.** `audit.security` is owned exclusively by the Identity Service and carries authentication events (logins, token issuance, credential rotation) subject to a 7-year regulatory retention policy. `audit.business` carries all other domain events and can be retained on a shorter cycle. Both feed the Audit Log Service as the single consumer writing to the immutable store.

---

### 💻 Tech Stack
*   **Runtime:** .NET 8/9 (C#), Go
*   **Orchestration:** Temporal (durable workflow execution, .NET SDK)
*   **Messaging:** MassTransit / RabbitMQ (Asynchronous Pub/Sub)
*   **Persistence:** PostgreSQL (Transactional), Redis (State/Caching)
*   **Real-time:** SignalR / WebSockets
*   **Security:** OpenIddict (OIDC), JWT, OAuth2
*   **Infrastructure:** Docker, Kubernetes, YARP (API Gateway), .NET Aspire
*   **Observability:** OpenTelemetry, Jaeger, Prometheus, Grafana

---

### 🛠️ Getting Started
*(Add instructions here for `dotnet aspire` or `docker-compose up` once your environment is set up.)*
