workspace "Drone Delivery Platform" "C4 architecture model for the drone delivery platform." {

    model {

        # ── People ────────────────────────────────────────────────────────────
        customer   = person "Customer"           "Places and tracks drone delivery orders via web or mobile."
        opsAdmin   = person "Operations & Admins" "Monitors fleet health, manages service configuration, reviews audit logs."
        droneFleet = person "Drone Fleet"         "Physical drones streaming real-time GPS telemetry over WebSocket."

        # ── External systems ──────────────────────────────────────────────────
        paymentGateway        = softwareSystem "Payment Gateway"              "Processes card charges, holds, captures, and refunds. (e.g. Stripe)"                                   "External"
        mapsApi               = softwareSystem "Maps API"                     "Provides optimal drone flight path computation and map tile data."                                      "External"
        faaApi                = softwareSystem "FAA / Airspace Authority APIs" "Supplies real-time no-fly zone data, weather holds, and regulatory constraints."                      "External"
        notificationProviders = softwareSystem "SMTP / SMS / Push Providers"  "Delivers customer and operator notifications across email, SMS, and push channels."                   "External"

        # ── Platform ──────────────────────────────────────────────────────────
        platform = softwareSystem "Drone Delivery Platform" "Orchestrates end-to-end drone deliveries: order intake, fulfillment saga, fleet dispatch, real-time tracking, and regulatory compliance." {

            # Edge
            apiGateway = container "API Gateway"  "TLS termination, JWT validation at the edge, rate limiting, and routing to downstream services."                          "YARP / .NET"
            keycloak   = container "Keycloak"     "Authentication, JWT issuance, token revocation, RBAC, and audit event emission."                                          ".NET / OpenIddict / OIDC / OAuth2"
            opa        = container "OPA"          "Evaluates fine-grained Rego authorization policies for services that need cross-dimensional access control."              "Rego / sidecar"

            # Shared infrastructure
            rabbitMq = container "RabbitMQ" "Asynchronous event bus. All non-critical inter-service communication flows through here." "MassTransit"  "MessageBus"
            redis    = container "Redis"    "Buffers high-frequency drone telemetry pings and stores the Keycloak token blocklist."   "Redis"         "Cache"

            # Order domain
            orderService   = container "Order & Transaction" "Hot-path checkout: accepts orders, processes payments, and initiates the fulfillment saga via transactional outbox." ".NET / MassTransit / Polly"
            paymentService = container "Payment / Billing"   "Wraps the payment gateway. Owns the charge / hold / capture / refund lifecycle with idempotency keys."              ".NET / Polly"
            ordersDb       = container "Orders DB"           "Stores orders, payment events, and outbox messages."                                                                "PostgreSQL" "Database"

            # Fulfillment domain
            fulfillment  = container "Fulfillment Orchestrator" "Saga coordinator: assigns drones, sequences compensating transactions on failure, and owns the delivery state machine." ".NET / MassTransit"
            inventory    = container "Inventory Service"        "Tracks stock levels per warehouse. Handles reservation and release during the saga with optimistic concurrency."        ".NET / EF Core"
            scheduler    = container "Scheduler / Optimizer"    "Determines dispatch timing, batches nearby orders to the same drone, and respects delivery windows."                   ".NET / MassTransit"
            inventoryDb  = container "Inventory DB"             "Stores stock levels, reservations, and warehouse state."                                                               "PostgreSQL" "Database"

            # Customer domain
            customerService = container "Customer / Profile" "Stores delivery addresses, contact preferences, notification channels, and account history." ".NET / EF Core"
            customersDb     = container "Customers DB"       "Stores customer profiles, addresses, and preferences."                                        "PostgreSQL" "Database"

            # Drone & Fleet domain
            telemetryHub   = container "Drone Telemetry Hub"   "High-throughput ingestion of real-time GPS pings from thousands of active drones. Buffers to Redis and fans out to subscribers." "Go / SignalR / Redis"
            fleetManagement = container "Fleet Management"     "Owns the drone registry: hardware IDs, battery state, maintenance status, home dock, and availability for assignment."           ".NET / EF Core"
            geospatial     = container "Geospatial / Routing"  "Computes optimal drone flight paths, enforces no-fly zones, integrates with the Maps API, and reroutes around dynamic obstacles." "Go / H3 / S2"
            regulatory     = container "Regulatory / Airspace" "Enforces FAA and local authority rules: no-fly zones, weather holds, and emergency recall. Can halt a delivery mid-saga."        "Go"
            fleetDb        = container "Fleet DB"              "Stores drone registry, battery state, maintenance logs, and dock assignments."                                                    "PostgreSQL" "Database"

            # Shipping domain
            shipping   = container "Shipping & Tracking" "Source of truth for shipment state. Bridges telemetry to orders, computes real-time ETAs, and evaluates geofence boundaries." ".NET / Redis"
            shippingDb = container "Shipping DB"         "Stores shipment state, ETA history, and geofence event log."                                                                  "PostgreSQL" "Database"

            # Platform services
            notifications = container "Notification Engine"  "Decoupled multi-channel alerting (email, SMS, push) driven entirely by event subscriptions."                              ".NET / MassTransit"
            auditLog      = container "Audit Log Service"    "Consumes events from all services and writes to an immutable, append-only store for regulatory and security review."      ".NET / MassTransit"
            auditDb       = container "Audit DB"             "Immutable append-only audit trail of all platform events for regulatory and security review."                             "PostgreSQL (append-only)" "Database"
        }

        # ── L1 System Context relationships ───────────────────────────────────
        customer   -> platform "Places orders and tracks deliveries" "REST / HTTPS"
        opsAdmin   -> platform "Monitors fleet and manages config"   "REST / HTTPS"
        droneFleet -> platform "Streams GPS telemetry"               "WebSocket"

        platform -> paymentGateway        "Charges and refunds customers"              "HTTPS"
        platform -> mapsApi               "Computes optimal flight paths"              "HTTPS"
        platform -> faaApi                "Checks no-fly zones and weather holds"      "HTTPS"
        platform -> notificationProviders "Sends delivery and alert notifications"     "HTTPS"

        # ── L2 Container relationships ─────────────────────────────────────────

        # Inbound through gateway
        customer   -> apiGateway   "Places orders, views tracking"  "REST / HTTPS"
        opsAdmin   -> apiGateway   "Manages config and monitors"    "REST / HTTPS"
        droneFleet -> telemetryHub "Streams GPS pings"              "WebSocket"

        # Edge routing
        apiGateway -> keycloak       "Validates JWT"               "OIDC / JWKS"
        apiGateway -> orderService   "Routes order requests"       "REST"
        apiGateway -> customerService "Routes profile requests"    "REST"
        apiGateway -> shipping       "Routes tracking requests"    "REST"
        apiGateway -> opa            "Evaluates policies"          "HTTP"

        # Auth & policy sidecar
        orderService -> opa   "Evaluates authz policies" "HTTP"
        fulfillment  -> opa   "Evaluates authz policies" "HTTP"
        keycloak     -> redis "Stores token blocklist"   "Redis protocol"

        # Databases
        orderService    -> ordersDb    "Reads/writes orders and outbox" "SQL"
        paymentService  -> ordersDb    "Reads/writes payment events"    "SQL"
        inventory       -> inventoryDb "Reads/writes stock levels"      "SQL"
        customerService -> customersDb "Reads/writes profiles"          "SQL"
        fleetManagement -> fleetDb     "Reads/writes drone registry"    "SQL"
        shipping        -> shippingDb  "Reads/writes shipment state"    "SQL"
        auditLog        -> auditDb     "Appends audit events"           "SQL"

        # Telemetry pipeline
        telemetryHub -> redis    "Buffers high-frequency pings"   "Redis protocol"
        telemetryHub -> rabbitMq "Publishes telemetry events"     "MassTransit"
        redis        -> shipping "Consumed for real-time ETA"     "Redis protocol"

        # Event bus — publishers
        orderService    -> rabbitMq "Publishes order.created, order.cancelled"            "MassTransit"
        fulfillment     -> rabbitMq "Publishes saga steps, drone.assigned"                "MassTransit"
        inventory       -> rabbitMq "Publishes inventory.reserved / released / failed"   "MassTransit"
        paymentService  -> rabbitMq "Publishes payment.captured / failed, order.refunded" "MassTransit"
        fleetManagement -> rabbitMq "Publishes drone.departed / returned / fault"         "MassTransit"
        scheduler       -> rabbitMq "Publishes dispatch.scheduled"                        "MassTransit"
        shipping        -> rabbitMq "Publishes delivery.completed / failed / eta.updated" "MassTransit"
        regulatory      -> rabbitMq "Publishes airspace.hold, emergency.recall"           "MassTransit"

        # Event bus — subscribers
        rabbitMq -> fulfillment     "saga.step, saga.compensate, dispatch commands"  "MassTransit"
        rabbitMq -> inventory       "saga.compensate — release stock"                "MassTransit"
        rabbitMq -> paymentService  "saga.compensate — refund"                       "MassTransit"
        rabbitMq -> fleetManagement "drone.assigned — update registry"               "MassTransit"
        rabbitMq -> scheduler       "delivery.completed — trigger next batch"        "MassTransit"
        rabbitMq -> shipping        "drone.assigned, delivery events"                "MassTransit"
        rabbitMq -> notifications   "order, delivery, and fault events"              "MassTransit"
        rabbitMq -> auditLog        "all domain events"                              "MassTransit"
        rabbitMq -> regulatory      "drone.fault, telemetry events"                  "MassTransit"

        # Sync gRPC paths
        fulfillment     -> inventory   "Reserves / releases stock"  "gRPC"
        fulfillment     -> geospatial  "Computes delivery route"    "gRPC"
        fleetManagement -> geospatial  "Resolves drone routing"     "gRPC"

        # External integrations
        paymentService -> paymentGateway        "Charges / refunds"              "HTTPS"
        geospatial     -> mapsApi               "Fetches flight paths"           "HTTPS"
        regulatory     -> faaApi                "Queries airspace rules"         "HTTPS"
        notifications  -> notificationProviders "Dispatches notifications"       "HTTPS"
    }

    views {

        # C4 L1 — System Context
        systemContext platform "SystemContext" "C4 Level 1 — System Context for the Drone Delivery Platform" {
            include *
            autoLayout tb
        }

        # C4 L2 — Container
        container platform "Containers" "C4 Level 2 — Containers within the Drone Delivery Platform" {
            include *
            autoLayout tb
        }

        styles {
            element "Person" {
                shape Person
                background #08427B
                color #ffffff
            }
            element "Software System" {
                background #1168BD
                color #ffffff
            }
            element "External" {
                background #999999
                color #ffffff
            }
            element "Container" {
                background #438DD5
                color #ffffff
            }
            element "Database" {
                shape Cylinder
                background #438DD5
                color #ffffff
            }
            element "MessageBus" {
                shape Pipe
                background #438DD5
                color #ffffff
            }
            element "Cache" {
                shape Cylinder
                background #85BBF0
                color #000000
            }
            relationship "Relationship" {
                dashed false
            }
        }
    }
}
