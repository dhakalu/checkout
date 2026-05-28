# Architectural Review Document
## Compensating Transaction Patterns
### Drone Delivery Platform
 
> **INTERNAL — ENGINEERING USE ONLY**
 
| Field | Value |
|---|---|
| Document Type | Architectural Review Document (ARD) |
| Version | 1.0 — Initial Draft |
| Status | For Review |
| Date | May 21, 2026 |
| Domain | Distributed Systems / Saga Patterns |
 
---
 
## Table of Contents
 
1. [Executive Summary](#1-executive-summary)
2. [Problem Statement](#2-problem-statement)
3. [Scope & Services Affected](#3-scope--services-affected)
4. [The Oversell Race Condition](#4-the-oversell-race-condition)
5. [Compensating Transaction Strategy](#5-compensating-transaction-strategy)
6. [Saga Design — Happy Path](#6-saga-design--happy-path)
7. [Compensation Flows by Failure Scenario](#7-compensation-flows-by-failure-scenario)
8. [Notification Timing Contract](#8-notification-timing-contract)
9. [Inventory Service — Concurrency Controls](#9-inventory-service--concurrency-controls)
10. [Service Responsibility Matrix](#10-service-responsibility-matrix)
11. [Risk Register](#11-risk-register)
12. [Open Questions & Decisions](#12-open-questions--decisions)
13. [Appendix A — Event Catalogue](#appendix-a--event-catalogue)
---
 
## 1. Executive Summary
 
This document records the architectural decision to implement the Compensating Transaction pattern across the drone delivery fulfillment saga. It captures the problem, the chosen strategy, the failure scenarios we must handle, and the responsibilities of each participating service.
 
**Core finding:** the Order Service must remain a thin write path and saga initiator only. The Fulfillment Orchestrator owns all coordination and compensation logic. Customer-facing confirmations must be decoupled from order creation and driven instead by deterministic saga milestones to prevent false promises when downstream steps fail.
 
---
 
## 2. Problem Statement
 
### 2.1 Context
 
A customer checkout triggers a cross-service fulfillment flow touching at minimum five independent services: Order & Transaction, Inventory, Payment/Billing, Fleet Management, and Fulfillment Orchestrator. Each service has its own database and deployment lifecycle. There is no distributed transaction coordinator.
 
Failures at any stage — hardware faults, network partitions, inventory exhaustion, payment declines, drone unavailability — can leave the system in a partially-committed state where money has moved, stock has been reserved, or a drone has been assigned, but the overall order cannot complete.
 
### 2.2 The Critical Race Condition
 
The immediate trigger for this ARD is the oversell scenario: two concurrent orders both observe available stock, both receive an "Order Confirmed" email, but only one can ultimately be fulfilled. The second customer has been given a false promise with no automated remedy.
 
> **Root Cause**
> - Customer notification fires on `OrderPlaced` (order record created), not on `InventoryReserved`.
> - There is no soft-reservation step before confirmation is issued.
> - The Inventory Service has no optimistic concurrency guard on reservation writes under the current design.
 
---
 
## 3. Scope & Services Affected
 
| Service | Role in Compensation | Change Required |
|---|---|---|
| Order & Transaction | Saga initiator; owns pre-auth guard | Add soft-reserve call; delay notification trigger |
| Fulfillment Orchestrator | Saga coordinator; owns all compensation logic | Define compensation steps for every failure path |
| Inventory Service | Reserve / release stock; prevent oversell | Add optimistic concurrency; expose `ReserveStock` / `ReleaseStock` |
| Payment / Billing | Hold, capture, and refund charge lifecycle | Expose `VoidHold` / `Refund` with idempotency keys |
| Fleet Management | Assign and unassign drones | Expose `AssignDrone` / `UnassignDrone` |
| Notification Engine | Multi-channel customer alerts | Subscribe to correct saga events; suppress premature confirms |
| Shipping & Tracking | Shipment state source of truth | Emit `ShipmentCancelled` on saga rollback |
 
---
 
## 4. The Oversell Race Condition
 
### 4.1 Sequence of Events (Current / Broken)
 
| Step | User A (wins) | User B (loses) |
|---|---|---|
| 1 | Sees "1 unit in stock" | Sees "1 unit in stock" |
| 2 | `POST /orders` → 202 Accepted | `POST /orders` → 202 Accepted |
| 3 | "Order confirmed!" email sent | "Order confirmed!" email sent ← **FALSE PROMISE** |
| 4 | Saga reserves last unit ✓ | Saga attempts reservation → 0 stock ✗ |
| 5 | Fulfillment proceeds normally | No automated compensation; customer unaware |
 
### 4.2 Why This Happens
 
- Notification Engine subscribes to `OrderPlaced`, which fires before any inventory check.
- The Order Service performs no synchronous guard before persisting the order.
- The Inventory Service has no row-level version check on concurrent reservation writes.
---
 
## 5. Compensating Transaction Strategy
 
### 5.1 Decision: Optimistic Confirm with Deterministic Compensation
 
We adopt the optimistic model: orders are accepted immediately for throughput, but the "confirmed" customer notification is decoupled from order creation and fires only after the saga passes the inventory reservation milestone. Failed sagas trigger automated compensation and a corrective customer notification.
 
We do **not** adopt pessimistic two-phase locking across services. That would couple the checkout latency to the slowest downstream service and create distributed deadlock risk.
 
### 5.2 Two-Layer Defence
 
| Layer | Mechanism | Purpose |
|---|---|---|
| Layer 1 | Synchronous pre-auth payment hold in Order Service | Fail fast on bad card before any saga work starts |
| Layer 2 | Optimistic concurrency in Inventory Service (version column) | Exactly one winner under concurrent reservation attempts |
 
> **Why Pre-Auth Is Not a Saga Step**
> A payment pre-auth requires no compensation — it is voided on failure, not reversed. Failing here costs nothing and avoids starting saga work that will immediately be rolled back. This is the only synchronous call the Order Service makes to a downstream service.
 
### 5.3 Revised Order Service Entry Point
 
| # | Action | On Failure |
|---|---|---|
| 1 | Validate request (schema, auth) | Return 400 / 401 — no side effects |
| 2 | Synchronous pre-auth payment hold (Payment Service) | Void hold → return 402 — no order created |
| 3 | Persist Order (status: `PENDING`) + OutboxMessage in one transaction | DB rollback — no event published |
| 4 | Return `202 Accepted` to caller | — |
| 5 | Outbox publishes `OrderPlaced` | Retry via outbox — at-least-once delivery guaranteed |
 
---
 
## 6. Saga Design — Happy Path
 
The Fulfillment Orchestrator drives the following sequence after consuming `OrderPlaced`. Each step emits an event that other services, including the Notification Engine, can subscribe to.
 
| Step | Orchestrator Action | Success Event | Compensation Event |
|---|---|---|---|
| 1 | Reserve inventory (Inventory Service) | `InventoryReserved` | `InventoryReleased` |
| 2 | Capture payment hold (Payment Service) | `PaymentCaptured` | `PaymentRefunded` |
| 3 | Assign drone (Fleet Management) | `DroneAssigned` | `DroneUnassigned` |
| 4 | Create shipment record (Shipping & Tracking) | `ShipmentCreated` | `ShipmentCancelled` |
| 5 | Schedule dispatch (Scheduler / Optimizer) | `DispatchScheduled` | `DispatchCancelled` |
| 6 | Mark order CONFIRMED (Order Service) | `OrderConfirmed` | `OrderFailed` |
 
> **Notification Trigger**
> The "Your order is confirmed" customer email fires on `OrderConfirmed` — step 6 — not on `OrderPlaced`. This is the earliest point at which all compensable steps have succeeded and the promise is safe to make.
 
---
 
## 7. Compensation Flows by Failure Scenario
 
### 7.1 Failure at Step 1 — Inventory Exhausted
 
> **Scenario:** Two concurrent sagas both attempt to reserve the last unit. Optimistic concurrency in Inventory Service allows exactly one to succeed. The losing saga receives a concurrency conflict on its reservation attempt.
 
| # | Compensation Action | Owner |
|---|---|---|
| 1 | Void the pre-auth payment hold | Fulfillment Orchestrator → Payment Service |
| 2 | Mark order FAILED (status: `INVENTORY_UNAVAILABLE`) | Fulfillment Orchestrator → Order Service |
| 3 | Publish `OrderFailed` | Order Service outbox |
| 4 | Send "Out of stock" email with refund ETA | Notification Engine (consumes `OrderFailed`) |
 
### 7.2 Failure at Step 2 — Payment Capture Fails
 
| # | Compensation Action | Owner |
|---|---|---|
| 1 | Release inventory reservation | Fulfillment Orchestrator → Inventory Service |
| 2 | Void the pre-auth hold | Fulfillment Orchestrator → Payment Service |
| 3 | Mark order FAILED (status: `PAYMENT_DECLINED`) | Fulfillment Orchestrator → Order Service |
| 4 | Send "Payment issue" email with retry link | Notification Engine (consumes `OrderFailed`) |
 
### 7.3 Failure at Step 3 — No Drone Available
 
| # | Compensation Action | Owner |
|---|---|---|
| 1 | Release inventory reservation | Fulfillment Orchestrator → Inventory Service |
| 2 | Refund captured payment | Fulfillment Orchestrator → Payment Service |
| 3 | Mark order FAILED (status: `NO_DRONE_AVAILABLE`) | Fulfillment Orchestrator → Order Service |
| 4 | Send "Delivery delayed / refund issued" email | Notification Engine |
 
### 7.4 Mid-Delivery Halt — Regulatory Recall
 
The Regulatory / Airspace service can publish a `DroneRecalled` event at any point during an active delivery. This is a special case: the saga has fully committed, but external authority forces a rollback.
 
| # | Compensation Action | Owner |
|---|---|---|
| 1 | Fulfillment Orchestrator listens for `DroneRecalled` | Fulfillment Orchestrator |
| 2 | Unassign drone, update Fleet Management | Fulfillment Orchestrator → Fleet Management |
| 3 | Cancel shipment, reset inventory if undelivered | Fulfillment Orchestrator → Shipping + Inventory |
| 4 | Issue full refund | Fulfillment Orchestrator → Payment Service |
| 5 | Publish `OrderFailed` (reason: `REGULATORY_HALT`) | Order Service outbox |
| 6 | Send apology + refund notification | Notification Engine |
 
---
 
## 8. Notification Timing Contract
 
The Notification Engine is entirely event-driven. The following table is the binding contract for which event triggers each customer-facing message. No service may send a customer notification except by publishing the correct event.
 
| Trigger Event | Message Type | Message Content |
|---|---|---|
| `OrderPlaced` | Receipt | "We received your order #XXXX" |
| `OrderConfirmed` | Confirmation | "Your order is confirmed and assigned to a drone" |
| `DroneDispatched` | Dispatch | "Your delivery is on its way — live tracking: ..." |
| `DeliveryCompleted` | Completion | "Your order has been delivered" |
| `OrderFailed` (`INVENTORY_UNAVAILABLE`) | Apology + Void | "Item sold out — your hold has been voided" |
| `OrderFailed` (`PAYMENT_DECLINED`) | Apology + Retry | "Payment issue — retry or update card" |
| `OrderFailed` (`NO_DRONE_AVAILABLE`) | Apology + Refund | "No drones available — full refund issued" |
| `OrderFailed` (`REGULATORY_HALT`) | Apology + Refund | "Delivery halted by airspace authority — full refund" |
 
> **Invariant**
> The word "confirmed" **MUST NOT** appear in any message triggered by `OrderPlaced`. "Order received" and "Order confirmed" are distinct messages mapped to distinct events. Any notification template that conflates these two states must be updated as part of this work.
 
---
 
## 9. Inventory Service — Concurrency Controls
 
### 9.1 Required: Optimistic Concurrency on Reservation
 
Every reservation write must include a version predicate. If two saga instances attempt to reserve the last unit simultaneously, the database ensures exactly one succeeds.
 
```sql
-- Reservation write (fails if stock moved since saga read it)
UPDATE inventory
SET    reserved = reserved + @qty,
       version  = version + 1
WHERE  sku     = @sku
  AND  (quantity - reserved) >= @qty   -- sufficient stock
  AND  version = @expectedVersion;     -- nobody beat us
 
-- rows_affected = 0 → throw ConcurrencyException → saga compensates
```
 
### 9.2 Soft Reservation Lifecycle
 
| State | Triggered By | Description |
|---|---|---|
| `SOFT_RESERVED` | `InventoryReserved` | Stock held for 15 min; not yet permanently committed |
| `HARD_RESERVED` | `DroneAssigned` | Commitment confirmed; will not be released except on compensation |
| `RELEASED` | `InventoryReleased` | Compensation path; stock returned to available pool |
| `EXPIRED` | Scheduler job (15 min TTL) | Soft reservation timed out; auto-released; saga must compensate |
 
---
 
## 10. Service Responsibility Matrix
 
| Service | Publishes | Compensating Action Exposed |
|---|---|---|
| Order & Transaction | `OrderPlaced`, `OrderConfirmed`, `OrderFailed` | `UpdateOrderStatus(FAILED)` |
| Fulfillment Orchestrator | Orchestration commands (internal) | Drives all compensation sequences |
| Inventory Service | `InventoryReserved`, `InventoryReleased` | `ReleaseReservation(orderId)` |
| Payment / Billing | `PaymentCaptured`, `PaymentRefunded`, `HoldVoided` | `VoidHold(orderId)`, `Refund(orderId, amount)` |
| Fleet Management | `DroneAssigned`, `DroneUnassigned` | `UnassignDrone(droneId, orderId)` |
| Shipping & Tracking | `ShipmentCreated`, `ShipmentCancelled` | `CancelShipment(shipmentId)` |
| Regulatory / Airspace | `DroneRecalled` | N/A — authority source; cannot compensate itself |
| Notification Engine | *(none — consumer only)* | N/A |
 
---
 
## 11. Risk Register
 
| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Compensation step itself fails (e.g. Payment Service down during refund) | Medium | High | Polly retry + circuit breaker on all compensation calls; dead-letter queue with manual intervention queue |
| Soft reservation expires mid-saga while drone is being assigned | Low | Medium | 15-min TTL is well above P99 saga completion time; Orchestrator checks reservation status before `DroneAssigned` step |
| Duplicate `OrderPlaced` events cause double saga execution | Medium | High | Fulfillment Orchestrator uses `orderId` as idempotency key; duplicate starts are ignored after first state machine instance is created |
| Customer receives both "confirmed" and "failed" emails due to race | Low | Medium | Confirmation only fires on `OrderConfirmed` (step 6); `OrderFailed` can only fire after compensation; the two events are mutually exclusive by state machine design |
 
---
 
## 12. Open Questions & Decisions
 
| # | Question | Owner | Target Date |
|---|---|---|---|
| 1 | Should the soft reservation TTL be 15 min or configurable per SKU category? | Inventory + Product teams | TBD |
| 2 | What is the SLA for manual intervention on dead-lettered compensation failures? | Ops / SRE | TBD |
| 3 | Do we surface saga state to CS agents in an internal dashboard, and if so which service owns that view? | Platform team | TBD |
| 4 | Is a regulatory recall always a full refund, or should partial delivery (item partially shipped) be handled differently? | Legal / Product | TBD |
| 5 | Should Payment pre-auth be removed if the saga already captures synchronously? (Avoids double hold.) | Payments team | TBD |
 
---
 
## Appendix A — Event Catalogue
 
All events referenced in this document. Services must not rely on internal state of other services; only events listed here form the integration contract.
 
| Event | Publisher | Key Payload Fields |
|---|---|---|
| `OrderPlaced` | Order & Transaction | `orderId`, `customerId`, `items[]`, `paymentHoldId` |
| `OrderConfirmed` | Order & Transaction | `orderId`, `droneId`, `eta` |
| `OrderFailed` | Order & Transaction | `orderId`, `reason` (enum), `refundId?` |
| `InventoryReserved` | Inventory Service | `orderId`, `sku`, `qty`, `reservationId`, `expiresAt` |
| `InventoryReleased` | Inventory Service | `orderId`, `reservationId`, `reason` |
| `PaymentCaptured` | Payment / Billing | `orderId`, `chargeId`, `amount`, `currency` |
| `PaymentRefunded` | Payment / Billing | `orderId`, `refundId`, `amount`, `eta` |
| `HoldVoided` | Payment / Billing | `orderId`, `holdId` |
| `DroneAssigned` | Fleet Management | `orderId`, `droneId`, `homeBase`, `batteryPct` |
| `DroneUnassigned` | Fleet Management | `orderId`, `droneId`, `reason` |
| `ShipmentCreated` | Shipping & Tracking | `orderId`, `shipmentId`, `eta` |
| `ShipmentCancelled` | Shipping & Tracking | `shipmentId`, `orderId`, `reason` |
| `DroneRecalled` | Regulatory / Airspace | `droneId`, `reason`, `authority`, `effectiveAt` |
| `DispatchScheduled` | Scheduler / Optimizer | `orderId`, `droneId`, `departureTime` |
 
---
 
*Confidential — Engineering Use Only*
 