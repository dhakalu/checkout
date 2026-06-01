# ADR-001: Temporal for Fulfillment Orchestration

| Field | Value |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-05-29 |
| **Deciders** | Engineering |
| **Domain** | Fulfillment Orchestrator |

---

## Context

The Checkout platform coordinates a multi-step fulfillment workflow that spans several independently deployed services: inventory reservation, payment capture, drone assignment, dispatch scheduling, and delivery tracking. This workflow must handle partial failures gracefully — if drone assignment fails after payment has been captured, the payment must be refunded and inventory released. If a drone is recalled mid-flight by the Regulatory service, the saga must compensate from whatever step it had reached.

The initial design used a **choreography-based saga** over RabbitMQ via MassTransit. Each saga participant published and consumed events, and the Fulfillment Orchestrator maintained a state machine on the bus. This approach was evaluated against an **orchestration-based** model using Temporal before any significant implementation was committed.

The core question was: **who owns the fulfillment state, and how is it made durable?**

---

## Decision

Use **Temporal** (Temporal .NET SDK) as the durable execution engine for the fulfillment workflow. The Fulfillment Orchestrator becomes a Temporal Worker running a `FulfillmentWorkflow`. Step sequencing, retries, timeouts, and compensating transactions are expressed as code inside the workflow and its Activities. MassTransit is retained at the edges only — to receive `order.created` (workflow start) and `dispatch.scheduled` (mid-workflow signal).

---

## Alternatives Considered

### Option A — Choreography-based saga (MassTransit / RabbitMQ) ❌ Rejected

Each saga step is driven by events. The Fulfillment Orchestrator maintains a MassTransit state machine that transitions on `inventory.reserved`, `payment.captured`, `drone.assigned`, and so on. Compensation is triggered by publishing `saga.compensate` to a topic consumed by all relevant participants.

**Strengths:**
- Consistent with the rest of the platform's messaging model
- No additional infrastructure component
- Services remain loosely coupled through the bus

**Weaknesses:**
- Saga state lives in the database and must be manually correlated to in-flight messages; any inconsistency between the two is a silent bug
- Compensating transaction fan-out via a shared `saga.compensate` topic means all participants must interpret the same envelope — adding a new compensation step requires coordinating schema changes across services
- Timeout and retry logic must be hand-rolled per step using Polly or scheduled re-delivery; there is no unified view of "this workflow has been stalled for 10 minutes"
- Distributed tracing across a choreographed saga requires assembling a picture from many independent spans; no single place to inspect the state of one order's fulfillment
- Testing a choreographed saga end-to-end requires a running broker, all participant services, and careful message ordering — unit testing individual steps in isolation is awkward
- Failure modes are subtle: a message can be consumed and acknowledged before the downstream effect is durable, or the state machine can accept a duplicate event and advance incorrectly

### Option B — Temporal durable workflows ✅ Accepted

The fulfillment workflow is expressed as a `FulfillmentWorkflow` class in C#. Each step is a Temporal Activity that calls a downstream service via gRPC. Retries, timeouts, and compensation are configured declaratively per Activity and composed in normal control flow (try/catch/finally). Temporal persists the full workflow event history; if the worker crashes mid-execution, Temporal replays the history and resumes from exactly where it left off.

**Strengths:**
- Workflow state is owned and persisted by Temporal — no bespoke state machine table, no correlation IDs to manage manually
- Compensation is expressed as a try/catch in the workflow code; the sequence is explicit, readable, and testable without a broker
- Retry policy, timeout, and heartbeat are configured per Activity; the platform gets consistent behaviour without per-service Polly policies for saga steps
- Temporal's Web UI provides a complete event history for any workflow instance — debugging a stuck or failed order requires no log aggregation
- Worker replay guarantees exactly-once Activity execution semantics from the workflow's perspective, eliminating the class of duplicate-event bugs present in choreography
- Adding a new step or compensation action is a local change to the workflow code; no new topic, no schema coordination with participants

**Weaknesses:**
- Temporal is an additional infrastructure component (Temporal Server + backing store) that must be operated, monitored, and sized
- The .NET SDK is mature but less battle-tested in the .NET ecosystem than MassTransit
- Workflow versioning (using `Workflow.GetVersion`) requires discipline when deploying changes to long-running workflows
- Activities that need to signal back to the workflow (e.g. `dispatch.scheduled` arriving from the Scheduler) require a thin MassTransit consumer to bridge the bus event into a Temporal signal — a small but real seam

### Option C — Orchestration via MassTransit Courier ❌ Rejected

MassTransit Courier provides a routing-slip pattern for orchestrated sagas without a separate workflow engine. Steps are assembled into a routing slip and executed in sequence; compensating activities are declared alongside each step.

**Weaknesses:**
- Routing-slip state is embedded in the message; long-running workflows that pause to await external events (e.g. waiting for drone dispatch) are not a natural fit
- Less ecosystem momentum and tooling than Temporal for complex, long-running workflows
- Does not solve the observability gap — there is no equivalent to Temporal's workflow history UI

---

## Consequences

### Positive
- The fulfillment workflow is a single, readable unit of code. A new engineer can follow the happy path and all compensation paths by reading one file.
- Operational visibility into in-flight fulfillment is provided out of the box by the Temporal Web UI, without custom dashboards.
- Retry, timeout, and heartbeat configuration is centralised per Activity rather than scattered across services.
- End-to-end testing of the fulfillment workflow is possible with Temporal's test environment without a running broker or dependent services.

### Negative / Trade-offs
- **Operational overhead:** Temporal Server must be deployed, monitored, and backed up. The Temporal backing store (PostgreSQL) is a new stateful dependency with its own sizing and backup requirements separate from the application database.
- **Workflow versioning discipline:** Deployed changes to a workflow that has in-flight instances require versioning via `Workflow.GetVersion`. A versioning mistake can corrupt the replay of live workflows. This must be enforced through code review and deployment policy.
- **Bus-to-Temporal bridge:** The `dispatch.scheduled` event and any future mid-workflow signals require a thin MassTransit consumer in the Fulfillment Orchestrator that calls `SignalAsync`. This is a small coupling point that must be accounted for when adding new signals.
- **Team familiarity:** Temporal's programming model (deterministic workflow code, Activity vs Workflow separation, replay semantics) has a learning curve. Activities must not have non-deterministic side effects in the workflow layer; violations cause silent replay bugs.

### Neutral / Out of scope
- Services that are not saga participants (Notification Engine, Audit Log, Shipping & Tracking) are unaffected. They continue to consume bus events as before.
- The `payment.failed` event is still published by the compensation Activity so that Notification, Audit Log, and Order & Transaction can react — Temporal does not replace the bus for fan-out to non-participants.
- `dispatch.scheduled` is retained as a bus topic published by the Scheduler / Optimizer. Fleet Management and Audit Log continue to consume it; only the Fulfillment Orchestrator's consumption changes (from state machine advance to Temporal signal).

---

## References

- [Temporal documentation — .NET SDK](https://docs.temporal.io/dev-guide/dotnet)
- [Temporal documentation — Workflow versioning](https://docs.temporal.io/workflows#versioning)
- [MassTransit — Saga state machines](https://masstransit.io/documentation/patterns/saga/state-machine)
- [MassTransit — Courier (routing slips)](https://masstransit.io/documentation/patterns/routing-slip)
- [Checkout README](./README.md)