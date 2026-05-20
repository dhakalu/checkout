"""
C4 Level 1 — System Context Diagram
Drone Delivery Platform

Run:  uv run python shared/system_context.py
Out:  ../../c4/shared/context/system_context.png
"""

import os
from diagrams import Diagram
from diagrams.c4 import Person, System, Relationship

OUTPUT_DIR = os.path.join(
    os.path.dirname(os.path.abspath(__file__)),
    "..", "..", "c4", "shared", "context"
)
os.makedirs(OUTPUT_DIR, exist_ok=True)

graph_attr = {
    "splines":  "spline",
    "rankdir":  "TB",
    "pad":      "0.8",
    "nodesep":  "0.8",
    "ranksep":  "1.4",
}

with Diagram(
    "System Context — Drone Delivery Platform",
    filename=os.path.join(OUTPUT_DIR, "system_context"),
    outformat="png",
    show=False,
    graph_attr=graph_attr,
):
    # ── People ────────────────────────────────────────────────────────────────
    customer = Person(
        name="Customer",
        description="Places and tracks drone delivery orders via web or mobile.",
    )
    ops_admin = Person(
        name="Operations & Admins",
        description="Monitors fleet health, manages service configuration, reviews audit logs.",
    )
    drone_hw = Person(
        name="Drone Fleet",
        description="Physical drones streaming real-time GPS telemetry over WebSocket.",
    )

    # ── The platform (internal) ───────────────────────────────────────────────
    platform = System(
        name="Drone Delivery Platform",
        description="Orchestrates end-to-end drone deliveries: order intake, fulfillment saga, fleet dispatch, real-time tracking, and regulatory compliance.",
    )

    # ── External systems ──────────────────────────────────────────────────────
    payment_gw = System(
        name="Payment Gateway",
        description="Processes card charges, holds, captures, and refunds. (e.g. Stripe)",
        external=True,
    )
    maps_api = System(
        name="Maps API",
        description="Provides optimal drone flight path computation and map tile data.",
        external=True,
    )
    faa_api = System(
        name="FAA / Airspace Authority APIs",
        description="Supplies real-time no-fly zone data, weather holds, and regulatory constraints.",
        external=True,
    )
    smtp_sms = System(
        name="SMTP / SMS / Push Providers",
        description="Delivers customer and operator notifications across email, SMS, and push channels.",
        external=True,
    )

    # ── Relationships ─────────────────────────────────────────────────────────
    customer  >> Relationship("Places orders and tracks deliveries [REST/HTTPS]")   >> platform
    ops_admin >> Relationship("Monitors fleet and manages config [REST/HTTPS]")     >> platform
    drone_hw  >> Relationship("Streams GPS telemetry [WebSocket]")                  >> platform

    platform  >> Relationship("Charges and refunds customers [HTTPS]")              >> payment_gw
    platform  >> Relationship("Computes optimal flight paths [HTTPS]")              >> maps_api
    platform  >> Relationship("Checks no-fly zones and weather holds [HTTPS]")      >> faa_api
    platform  >> Relationship("Sends delivery and alert notifications [HTTPS]")     >> smtp_sms