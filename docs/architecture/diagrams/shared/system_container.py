"""
C4 Level 2 — Container Diagram
Drone Delivery Platform

Run:  uv run python shared/system_container.py
Out:  ../../c4/shared/container/system_container.png
"""

import os
from diagrams import Diagram
from diagrams.c4 import (
    Person, Container, Database, System,
    SystemBoundary, Relationship,
)

OUTPUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "c4", "shared", "container"
)
os.makedirs(OUTPUT_DIR, exist_ok=True)

graph_attr = {
    "splines":  "spline",
    "rankdir":  "TB",
    "pad":      "0.8",
    "nodesep":  "0.6",
    "ranksep":  "1.0",
}

with Diagram(
    "Container Diagram — Drone Delivery Platform",
    filename=os.path.join(OUTPUT_DIR, "system_container"),
    outformat="png",
    show=False,
    graph_attr=graph_attr,
):
    # ── People ────────────────────────────────────────────────────────────────
    customer  = Person(name="Customer",            description="Places and tracks orders via web or mobile.")
    ops_admin = Person(name="Operations & Admins", description="Monitors fleet health and service configuration.")
    drone_hw  = Person(name="Drone Fleet",         description="Physical drones streaming GPS telemetry.")

    # ── External systems ──────────────────────────────────────────────────────
    payment_gw = System(name="Payment Gateway",            description="e.g. Stripe. Charges, holds, captures, refunds.", external=True)
    maps_api   = System(name="Maps API",                   description="Flight path computation and map tiles.",          external=True)
    faa_api    = System(name="FAA / Airspace Authority",   description="No-fly zones, weather holds, regulatory rules.",  external=True)
    smtp_sms   = System(name="SMTP / SMS / Push",          description="Email, SMS, and push notification delivery.",     external=True)

    # ── Platform boundary ─────────────────────────────────────────────────────
    with SystemBoundary("Drone Delivery Platform"):

        # Edge
        gateway  = Container(name="API Gateway",       technology="YARP / .NET",        description="TLS termination, JWT validation, rate limiting, and routing.")
        keycloak = Container(name="Keycloak",          technology=".NET / OpenIddict",   description="Authentication, JWT issuance, RBAC, token revocation, and audit event emission.")
        opa      = Container(name="OPA",               technology="Rego / sidecar",      description="Fine-grained policy evaluation for cross-dimensional access control decisions.")

        # Shared infrastructure
        bus      = Container(name="RabbitMQ",          technology="MassTransit",         description="Asynchronous event bus. All non-critical inter-service communication flows through here.")
        cache    = Container(name="Redis",             technology="Redis",               description="Telemetry buffer for high-frequency GPS pings and Keycloak token blocklist.")

        # Order domain
        order_svc   = Container(name="Order & Transaction",  technology=".NET",          description="Hot-path checkout: accepts orders, processes payments, initiates fulfillment saga via transactional outbox.")
        payment_svc = Container(name="Payment / Billing",    technology=".NET / Polly",  description="Wraps payment gateway. Owns charge / hold / capture / refund lifecycle with idempotency keys.")
        order_db    = Database(name="Orders DB",             technology="PostgreSQL",    description="Orders, payment events, and outbox messages.")

        # Fulfillment domain
        fulfillment = Container(name="Fulfillment Orchestrator", technology=".NET / MassTransit", description="Saga coordinator: assigns drones, sequences compensating transactions on failure, owns delivery state machine.")
        inventory   = Container(name="Inventory Service",        technology=".NET / EF Core",     description="Tracks stock levels per warehouse. Handles reservation and release during saga with optimistic concurrency.")
        scheduler   = Container(name="Scheduler / Optimizer",    technology=".NET",               description="Determines dispatch timing, batches nearby orders to the same drone, respects delivery windows.")
        inv_db      = Database(name="Inventory DB",              technology="PostgreSQL",         description="Stock levels, reservations, and warehouse state.")

        # Customer domain
        customer_svc = Container(name="Customer / Profile", technology=".NET / EF Core", description="Stores delivery addresses, contact preferences, notification channels, and account history.")
        cust_db      = Database(name="Customers DB",        technology="PostgreSQL",     description="Customer profiles, addresses, and preferences.")

        # Drone & Fleet domain
        telemetry  = Container(name="Drone Telemetry Hub",    technology="Go / SignalR / Redis", description="High-throughput ingestion of GPS pings from thousands of active drones. Buffers to Redis and fans out to subscribers.")
        fleet_mgmt = Container(name="Fleet Management",       technology=".NET / EF Core",       description="Owns the drone registry: hardware IDs, battery state, maintenance status, home dock, and availability.")
        geospatial = Container(name="Geospatial / Routing",   technology="Go / H3 / S2",         description="Computes optimal drone flight paths, enforces no-fly zones, integrates with maps API, reroutes around obstacles.")
        regulatory = Container(name="Regulatory / Airspace",  technology="Go",                   description="Enforces FAA and local authority rules. Can halt a delivery mid-saga via emergency recall events.")
        fleet_db   = Database(name="Fleet DB",                technology="PostgreSQL",           description="Drone registry, battery state, maintenance logs, and dock assignments.")

        # Shipping domain
        shipping = Container(name="Shipping & Tracking", technology=".NET / Redis", description="Source of truth for shipment state. Bridges telemetry to orders, computes real-time ETAs, evaluates geofence boundaries.")
        ship_db  = Database(name="Shipping DB",          technology="PostgreSQL",   description="Shipment state, ETA history, and geofence event log.")

        # Platform services
        notification = Container(name="Notification Engine", technology=".NET / MassTransit", description="Decoupled multi-channel alerting (email, SMS, push) driven entirely by event subscriptions.")
        audit_log    = Container(name="Audit Log Service",   technology=".NET / MassTransit", description="Consumes events from all services and writes to an immutable, append-only store for regulatory review.")
        audit_db     = Database(name="Audit DB",             technology="PostgreSQL (append-only)", description="Immutable audit trail of all platform events for regulatory and security review.")

    # ── Inbound ───────────────────────────────────────────────────────────────
    customer  >> Relationship("Places orders, views tracking [REST/HTTPS]")  >> gateway
    ops_admin >> Relationship("Manages config and monitors [REST/HTTPS]")    >> gateway
    drone_hw  >> Relationship("Streams GPS pings [WebSocket]")               >> telemetry

    gateway >> Relationship("Validates JWT [OIDC/JWKS]")       >> keycloak
    gateway >> Relationship("Routes order requests [REST]")    >> order_svc
    gateway >> Relationship("Routes profile requests [REST]")  >> customer_svc
    gateway >> Relationship("Routes tracking requests [REST]") >> shipping
    gateway >> Relationship("Evaluates policies [HTTP]")       >> opa

    # ── Auth / policy ─────────────────────────────────────────────────────────
    order_svc   >> Relationship("Evaluates authz policies [HTTP]") >> opa
    fulfillment >> Relationship("Evaluates authz policies [HTTP]") >> opa
    keycloak    >> Relationship("Stores token blocklist [Redis]")  >> cache

    # ── Databases ─────────────────────────────────────────────────────────────
    order_svc    >> Relationship("Reads/writes orders and outbox") >> order_db
    payment_svc  >> Relationship("Reads/writes payment events")    >> order_db
    inventory    >> Relationship("Reads/writes stock levels")      >> inv_db
    customer_svc >> Relationship("Reads/writes profiles")         >> cust_db
    fleet_mgmt   >> Relationship("Reads/writes drone registry")   >> fleet_db
    shipping     >> Relationship("Reads/writes shipment state")   >> ship_db
    audit_log    >> Relationship("Appends audit events")          >> audit_db

    # ── Telemetry ─────────────────────────────────────────────────────────────
    telemetry >> Relationship("Buffers high-frequency pings [Redis]")     >> cache
    telemetry >> Relationship("Publishes telemetry events [MassTransit]") >> bus
    cache     >> Relationship("Consumed for real-time ETA [Redis]")       >> shipping

    # ── Async events (publishers → bus) ──────────────────────────────────────
    order_svc   >> Relationship("Publishes order.created, order.cancelled")        >> bus
    fulfillment >> Relationship("Publishes saga steps, drone.assigned")            >> bus
    inventory   >> Relationship("Publishes inventory.reserved/released/failed")   >> bus
    payment_svc >> Relationship("Publishes payment.captured/failed, order.refunded") >> bus
    fleet_mgmt  >> Relationship("Publishes drone.departed/returned/fault")        >> bus
    scheduler   >> Relationship("Publishes dispatch.scheduled")                   >> bus
    shipping    >> Relationship("Publishes delivery.completed/failed/eta.updated") >> bus
    regulatory  >> Relationship("Publishes airspace.hold, emergency.recall")      >> bus

    # ── Async events (bus → subscribers) ─────────────────────────────────────
    bus >> Relationship("saga.step, saga.compensate, dispatch commands") >> fulfillment
    bus >> Relationship("saga.compensate → release stock")               >> inventory
    bus >> Relationship("saga.compensate → refund")                      >> payment_svc
    bus >> Relationship("drone.assigned → update registry")              >> fleet_mgmt
    bus >> Relationship("delivery.completed → trigger next batch")       >> scheduler
    bus >> Relationship("drone.assigned, delivery events")               >> shipping
    bus >> Relationship("order, delivery, fault events")                 >> notification
    bus >> Relationship("all domain events → append-only store")         >> audit_log
    bus >> Relationship("drone.fault, telemetry events")                 >> regulatory

    # ── Sync gRPC ─────────────────────────────────────────────────────────────
    fulfillment >> Relationship("Reserves / releases stock [gRPC]")  >> inventory
    fulfillment >> Relationship("Computes delivery route [gRPC]")     >> geospatial
    fleet_mgmt  >> Relationship("Resolves drone routing [gRPC]")      >> geospatial

    # ── External integrations ─────────────────────────────────────────────────
    payment_svc  >> Relationship("Charges / refunds [HTTPS]")         >> payment_gw
    geospatial   >> Relationship("Fetches flight paths [HTTPS]")      >> maps_api
    regulatory   >> Relationship("Queries airspace rules [HTTPS]")    >> faa_api
    notification >> Relationship("Dispatches notifications [HTTPS]")  >> smtp_sms