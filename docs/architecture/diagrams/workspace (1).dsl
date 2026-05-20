workspace "Drone Delivery Platform" "C4 architecture model for the drone delivery platform." {

    model {

        # ── People ────────────────────────────────────────────────────────────
        customer   = person "Customer"            "Places and tracks drone delivery orders via web or mobile."
        opsAdmin   = person "Operations & Admins" "Monitors fleet health, manages service configuration, reviews audit logs."
        droneFleet = person "Drone Fleet"         "Physical drones streaming real-time GPS telemetry over WebSocket."

        # ── External systems ──────────────────────────────────────────────────
        paymentGateway        = softwareSystem "Payment Gateway"               "Processes card charges, holds, captures, and refunds. (e.g. Stripe)"                              "External"
        mapsApi               = softwareSystem "Maps API"                      "Provides optimal drone flight path computation and map tile data."                                 "External"
        faaApi                = softwareSystem "FAA / Airspace Authority APIs" "Supplies real-time no-fly zone data, weather holds, and regulatory constraints."                  "External"
        notificationProviders = softwareSystem "SMTP / SMS / Push Providers"   "Delivers customer and operator notifications across email, SMS, and push channels."              "External"

        # ── Platform ──────────────────────────────────────────────────────────
        platform = softwareSystem "Drone Delivery Platform" "Orchestrates end-to-end drone deliveries: order intake, fulfillment saga, fleet dispatch, real-time tracking, and regulatory compliance." {

            # ── Edge ─────────────────────────────────────────────────────────
            apiGateway = container "API Gateway"  "TLS termination, JWT validation, rate limiting, and routing."        "YARP / .NET"
            keycloak   = container "Keycloak"     "Authentication, JWT issuance, token revocation, RBAC."              ".NET / OpenIddict / OIDC"
            opa        = container "OPA"          "Fine-grained Rego policy evaluation. Runs as a sidecar."            "Rego / sidecar"

            # ── Shared infrastructure ─────────────────────────────────────────
            redis = container "Redis" "Buffers high-frequency telemetry pings and stores the Keycloak token blocklist." "Redis" "Cache"

            # ── Order domain topics ───────────────────────────────────────────
            topicOrderCreated   = container "order.created"   "Publisher: Order & Transaction. Subscribers: Fulfillment, Inventory, Audit Log. DLQ: yes."          "RabbitMQ topic" "Topic"
            topicOrderCancelled = container "order.cancelled" "Publisher: Order & Transaction. Subscribers: Fulfillment, Inventory, Notification, Audit Log."      "RabbitMQ topic" "Topic"
            topicOrderRefunded  = container "order.refunded"  "Publisher: Payment/Billing. Subscribers: Order & Transaction, Notification, Audit Log. DLQ: yes."   "RabbitMQ topic" "Topic"
            topicPaymentCaptured = container "payment.captured" "Publisher: Payment/Billing. Subscribers: Order & Transaction, Audit Log. DLQ: yes."               "RabbitMQ topic" "Topic"
            topicPaymentFailed   = container "payment.failed"   "Publisher: Payment/Billing. Subscribers: Order & Transaction, Notification, Audit Log."           "RabbitMQ topic" "Topic"

            # ── Fulfillment domain topics ─────────────────────────────────────
            topicSagaStepCompleted      = container "saga.step.completed"         "Publisher: Fulfillment Orchestrator. Subscriber: self (next step)."                                    "RabbitMQ topic" "Topic"
            topicSagaStepFailed         = container "saga.step.failed"            "Publisher: any saga participant. Subscriber: Fulfillment Orchestrator. DLQ: yes."                     "RabbitMQ topic" "Topic"
            topicSagaCompensate         = container "saga.compensate"             "Publisher: Fulfillment Orchestrator. Subscribers: Inventory, Payment, Fleet Management. DLQ: yes."    "RabbitMQ topic" "Topic"
            topicInventoryReserved      = container "inventory.reserved"          "Publisher: Inventory Service. Subscriber: Fulfillment Orchestrator."                                   "RabbitMQ topic" "Topic"
            topicInventoryResFailed     = container "inventory.reservation.failed" "Publisher: Inventory Service. Subscribers: Fulfillment Orchestrator, Notification."                  "RabbitMQ topic" "Topic"
            topicInventoryReleased      = container "inventory.released"          "Publisher: Inventory Service. Subscribers: Fulfillment Orchestrator, Audit Log."                      "RabbitMQ topic" "Topic"
            topicDispatchScheduled      = container "dispatch.scheduled"          "Publisher: Scheduler/Optimizer. Subscribers: Fulfillment Orchestrator, Fleet Management."             "RabbitMQ topic" "Topic"

            # ── Drone / Telemetry domain topics ───────────────────────────────
            topicDroneAssigned    = container "drone.assigned"      "Publisher: Fulfillment Orchestrator. Subscribers: Fleet Management, Shipping & Tracking, Audit Log."               "RabbitMQ topic" "Topic"
            topicDroneDeparted    = container "drone.departed"      "Publisher: Fleet Management. Subscribers: Shipping & Tracking, Notification, Audit Log."                           "RabbitMQ topic" "Topic"
            topicDroneReturned    = container "drone.returned"      "Publisher: Fleet Management. Subscribers: Scheduler/Optimizer, Audit Log."                                         "RabbitMQ topic" "Topic"
            topicDroneFault       = container "drone.fault"         "Publisher: Fleet Management / Telemetry Hub. Subscribers: Fulfillment, Regulatory, Audit Log. DLQ: yes."          "RabbitMQ topic" "Topic"
            topicTelemetryPing    = container "telemetry.ping"      "Publisher: Drone Telemetry Hub. Subscribers: Shipping & Tracking, Geospatial, Regulatory. High frequency."        "RabbitMQ topic" "Topic"
            topicTelemetryBatch   = container "telemetry.ping.batch" "Publisher: Drone Telemetry Hub. Subscriber: Time-series store. Buffered flush from Redis."                       "RabbitMQ topic" "Topic"

            # ── Shipping domain topics ────────────────────────────────────────
            topicDeliveryCompleted  = container "delivery.completed"  "Publisher: Shipping & Tracking. Subscribers: Order & Transaction, Payment, Notification, Audit Log. DLQ: yes." "RabbitMQ topic" "Topic"
            topicDeliveryFailed     = container "delivery.failed"     "Publisher: Shipping & Tracking. Subscribers: Fulfillment, Notification, Audit Log. DLQ: yes."                  "RabbitMQ topic" "Topic"
            topicEtaUpdated         = container "delivery.eta.updated" "Publisher: Shipping & Tracking. Subscriber: Notification Engine."                                             "RabbitMQ topic" "Topic"
            topicGeofenceEntered    = container "geofence.entered"    "Publisher: Shipping & Tracking. Subscribers: Notification Engine, Regulatory."                                  "RabbitMQ topic" "Topic"
            topicGeofenceExited     = container "geofence.exited"     "Publisher: Shipping & Tracking. Subscribers: Regulatory, Audit Log."                                           "RabbitMQ topic" "Topic"

            # ── Fleet / Airspace domain topics ────────────────────────────────
            topicAirspaceViolation  = container "airspace.violation"     "Publisher: Regulatory/Airspace. Subscribers: Fulfillment, Fleet Management, Audit Log. DLQ: yes."           "RabbitMQ topic" "Topic"
            topicAirspaceHoldIssued = container "airspace.hold.issued"   "Publisher: Regulatory/Airspace. Subscribers: Fulfillment, Scheduler, Fleet Management."                     "RabbitMQ topic" "Topic"
            topicAirspaceHoldLifted = container "airspace.hold.lifted"   "Publisher: Regulatory/Airspace. Subscribers: Scheduler, Fulfillment."                                       "RabbitMQ topic" "Topic"
            topicEmergencyRecall    = container "drone.emergency.recall" "Publisher: Regulatory/Airspace. Subscribers: Fleet Management, Fulfillment, Audit Log. DLQ: yes."           "RabbitMQ topic" "Topic"

            # ── Platform topics ───────────────────────────────────────────────
            topicNotificationTrigger = container "notification.trigger" "Publisher: multiple services. Subscriber: Notification Engine. Generic envelope — channel resolved at runtime." "RabbitMQ topic" "Topic"
            topicAuditSecurity       = container "audit.security"       "Publisher: Keycloak (STS). Subscriber: Audit Log Service. 7-year retention — regulatory requirement."         "RabbitMQ topic" "Topic"
            topicAuditBusiness       = container "audit.business"       "Publisher: all services. Subscriber: Audit Log Service. Covers order, delivery, and role-assignment events."  "RabbitMQ topic" "Topic"

            # ── Services ──────────────────────────────────────────────────────
            orderService    = container "Order & Transaction"      "Hot-path checkout: accepts orders, processes payments, initiates the fulfillment saga via transactional outbox."    ".NET / MassTransit"
            paymentService  = container "Payment / Billing"        "Wraps payment gateway. Owns charge / hold / capture / refund lifecycle with idempotency keys."                     ".NET / Polly"
            fulfillment     = container "Fulfillment Orchestrator" "Saga coordinator: assigns drones, sequences compensating transactions, owns delivery state machine."                ".NET / MassTransit"
            inventory       = container "Inventory Service"        "Tracks stock levels per warehouse. Reservation and release with optimistic concurrency."                           ".NET / EF Core"
            scheduler       = container "Scheduler / Optimizer"    "Dispatch timing, batch optimisation, delivery windows."                                                            ".NET / MassTransit"
            customerService = container "Customer / Profile"       "Delivery addresses, contact preferences, notification channels, and account history."                             ".NET / EF Core"
            telemetryHub    = container "Drone Telemetry Hub"      "High-throughput GPS ping ingestion. Buffers to Redis, fans out to subscribers."                                   "Go / SignalR"
            fleetManagement = container "Fleet Management"         "Drone registry: hardware IDs, battery state, maintenance status, home dock, availability."                        ".NET / EF Core"
            geospatial      = container "Geospatial / Routing"     "Optimal flight paths, no-fly zones, Maps API integration, dynamic obstacle rerouting."                           "Go / H3 / S2"
            regulatory      = container "Regulatory / Airspace"    "FAA and local authority rules: no-fly zones, weather holds, emergency recall. Can halt a delivery mid-saga."      "Go"
            shipping        = container "Shipping & Tracking"      "Source of truth for shipment state. Bridges telemetry to orders, computes ETAs, evaluates geofences."             ".NET / Redis"
            notifications   = container "Notification Engine"      "Multi-channel alerting (email, SMS, push) driven entirely by event subscriptions."                                ".NET / MassTransit"
            auditLog        = container "Audit Log Service"        "Consumes events from all services. Writes to immutable append-only store for regulatory review."                  ".NET / MassTransit"

            # ── Databases ─────────────────────────────────────────────────────
            ordersDb    = container "Orders DB"    "Orders, payment events, outbox messages."             "PostgreSQL" "Database"
            inventoryDb = container "Inventory DB" "Stock levels, reservations, warehouse state."         "PostgreSQL" "Database"
            customersDb = container "Customers DB" "Customer profiles, addresses, preferences."           "PostgreSQL" "Database"
            fleetDb     = container "Fleet DB"     "Drone registry, battery state, maintenance logs."     "PostgreSQL" "Database"
            shippingDb  = container "Shipping DB"  "Shipment state, ETA history, geofence event log."    "PostgreSQL" "Database"
            auditDb     = container "Audit DB"     "Immutable append-only audit trail."                  "PostgreSQL (append-only)" "Database"
        }

        # ── L1 Context relationships ───────────────────────────────────────────
        customer   -> platform "Places orders and tracks deliveries" "REST / HTTPS"
        opsAdmin   -> platform "Monitors fleet and manages config"   "REST / HTTPS"
        droneFleet -> platform "Streams GPS telemetry"               "WebSocket"
        platform -> paymentGateway        "Charges and refunds customers"          "HTTPS"
        platform -> mapsApi               "Computes optimal flight paths"          "HTTPS"
        platform -> faaApi                "Checks no-fly zones and weather holds"  "HTTPS"
        platform -> notificationProviders "Sends delivery and alert notifications" "HTTPS"

        # ── L2 Container relationships ─────────────────────────────────────────

        # Inbound
        customer   -> apiGateway   "Places orders, views tracking" "REST / HTTPS"
        opsAdmin   -> apiGateway   "Manages config and monitors"   "REST / HTTPS"
        droneFleet -> telemetryHub "Streams GPS pings"             "WebSocket"

        # Edge
        apiGateway -> keycloak        "Validates JWT"            "OIDC / JWKS"
        apiGateway -> orderService    "Routes order requests"    "REST"
        apiGateway -> customerService "Routes profile requests"  "REST"
        apiGateway -> shipping        "Routes tracking requests" "REST"
        apiGateway -> opa             "Evaluates policies"       "HTTP"
        orderService -> opa           "Evaluates authz policies" "HTTP"
        fulfillment  -> opa           "Evaluates authz policies" "HTTP"
        keycloak     -> redis         "Stores token blocklist"   "Redis protocol"

        # Databases
        orderService    -> ordersDb    "Reads/writes" "SQL"
        paymentService  -> ordersDb    "Reads/writes" "SQL"
        inventory       -> inventoryDb "Reads/writes" "SQL"
        customerService -> customersDb "Reads/writes" "SQL"
        fleetManagement -> fleetDb     "Reads/writes" "SQL"
        shipping        -> shippingDb  "Reads/writes" "SQL"
        auditLog        -> auditDb     "Appends"      "SQL"

        # Telemetry buffering
        telemetryHub -> redis "Buffers high-frequency pings" "Redis protocol"
        redis -> shipping     "Consumed for real-time ETA"   "Redis protocol"

        # Sync gRPC
        fulfillment     -> inventory  "Reserves / releases stock" "gRPC"
        fulfillment     -> geospatial "Computes delivery route"   "gRPC"
        fleetManagement -> geospatial "Resolves drone routing"    "gRPC"

        # External integrations
        paymentService -> paymentGateway        "Charges / refunds"          "HTTPS"
        geospatial     -> mapsApi               "Fetches flight paths"        "HTTPS"
        regulatory     -> faaApi                "Queries airspace rules"      "HTTPS"
        notifications  -> notificationProviders "Dispatches notifications"    "HTTPS"

        # ── Order topics ──────────────────────────────────────────────────────
        orderService -> topicOrderCreated    "publishes"
        orderService -> topicOrderCancelled  "publishes"
        paymentService -> topicOrderRefunded  "publishes"
        paymentService -> topicPaymentCaptured "publishes"
        paymentService -> topicPaymentFailed   "publishes"

        topicOrderCreated    -> fulfillment  "subscribes"
        topicOrderCreated    -> inventory    "subscribes"
        topicOrderCreated    -> auditLog     "subscribes"
        topicOrderCancelled  -> fulfillment  "subscribes"
        topicOrderCancelled  -> inventory    "subscribes"
        topicOrderCancelled  -> notifications "subscribes"
        topicOrderCancelled  -> auditLog     "subscribes"
        topicOrderRefunded   -> orderService  "subscribes"
        topicOrderRefunded   -> notifications "subscribes"
        topicOrderRefunded   -> auditLog      "subscribes"
        topicPaymentCaptured -> orderService  "subscribes"
        topicPaymentCaptured -> auditLog      "subscribes"
        topicPaymentFailed   -> orderService  "subscribes"
        topicPaymentFailed   -> notifications "subscribes"
        topicPaymentFailed   -> auditLog      "subscribes"

        # ── Fulfillment topics ────────────────────────────────────────────────
        fulfillment -> topicSagaStepCompleted  "publishes"
        fulfillment -> topicSagaStepFailed     "publishes"
        fulfillment -> topicSagaCompensate     "publishes"
        fulfillment -> topicDroneAssigned      "publishes"
        inventory   -> topicInventoryReserved  "publishes"
        inventory   -> topicInventoryResFailed "publishes"
        inventory   -> topicInventoryReleased  "publishes"
        scheduler   -> topicDispatchScheduled  "publishes"
        orderService   -> topicSagaStepFailed  "publishes"
        paymentService -> topicSagaStepFailed  "publishes"
        inventory      -> topicSagaStepFailed  "publishes"
        fleetManagement -> topicSagaStepFailed "publishes"

        topicSagaStepCompleted  -> fulfillment     "subscribes"
        topicSagaStepFailed     -> fulfillment     "subscribes"
        topicSagaCompensate     -> inventory       "subscribes"
        topicSagaCompensate     -> paymentService  "subscribes"
        topicSagaCompensate     -> fleetManagement "subscribes"
        topicInventoryReserved  -> fulfillment     "subscribes"
        topicInventoryResFailed -> fulfillment     "subscribes"
        topicInventoryResFailed -> notifications   "subscribes"
        topicInventoryReleased  -> fulfillment     "subscribes"
        topicInventoryReleased  -> auditLog        "subscribes"
        topicDispatchScheduled  -> fulfillment     "subscribes"
        topicDispatchScheduled  -> fleetManagement "subscribes"
        topicDroneAssigned      -> fleetManagement "subscribes"
        topicDroneAssigned      -> shipping        "subscribes"
        topicDroneAssigned      -> auditLog        "subscribes"

        # ── Drone / Telemetry topics ──────────────────────────────────────────
        fleetManagement -> topicDroneDeparted  "publishes"
        fleetManagement -> topicDroneReturned  "publishes"
        fleetManagement -> topicDroneFault     "publishes"
        telemetryHub    -> topicDroneFault     "publishes"
        telemetryHub    -> topicTelemetryPing  "publishes"
        telemetryHub    -> topicTelemetryBatch "publishes"

        topicDroneDeparted  -> shipping        "subscribes"
        topicDroneDeparted  -> notifications   "subscribes"
        topicDroneDeparted  -> auditLog        "subscribes"
        topicDroneReturned  -> scheduler       "subscribes"
        topicDroneReturned  -> auditLog        "subscribes"
        topicDroneFault     -> fulfillment     "subscribes"
        topicDroneFault     -> regulatory      "subscribes"
        topicDroneFault     -> auditLog        "subscribes"
        topicTelemetryPing  -> shipping        "subscribes"
        topicTelemetryPing  -> geospatial      "subscribes"
        topicTelemetryPing  -> regulatory      "subscribes"
        topicTelemetryBatch -> auditDb         "subscribes (time-series flush)"

        # ── Shipping topics ───────────────────────────────────────────────────
        shipping -> topicDeliveryCompleted "publishes"
        shipping -> topicDeliveryFailed    "publishes"
        shipping -> topicEtaUpdated        "publishes"
        shipping -> topicGeofenceEntered   "publishes"
        shipping -> topicGeofenceExited    "publishes"

        topicDeliveryCompleted -> orderService  "subscribes"
        topicDeliveryCompleted -> paymentService "subscribes"
        topicDeliveryCompleted -> notifications  "subscribes"
        topicDeliveryCompleted -> auditLog       "subscribes"
        topicDeliveryFailed    -> fulfillment    "subscribes"
        topicDeliveryFailed    -> notifications  "subscribes"
        topicDeliveryFailed    -> auditLog       "subscribes"
        topicEtaUpdated        -> notifications  "subscribes"
        topicGeofenceEntered   -> notifications  "subscribes"
        topicGeofenceEntered   -> regulatory     "subscribes"
        topicGeofenceExited    -> regulatory     "subscribes"
        topicGeofenceExited    -> auditLog       "subscribes"

        # ── Fleet / Airspace topics ───────────────────────────────────────────
        regulatory -> topicAirspaceViolation  "publishes"
        regulatory -> topicAirspaceHoldIssued "publishes"
        regulatory -> topicAirspaceHoldLifted "publishes"
        regulatory -> topicEmergencyRecall    "publishes"

        topicAirspaceViolation  -> fulfillment     "subscribes"
        topicAirspaceViolation  -> fleetManagement "subscribes"
        topicAirspaceViolation  -> auditLog        "subscribes"
        topicAirspaceHoldIssued -> fulfillment     "subscribes"
        topicAirspaceHoldIssued -> scheduler       "subscribes"
        topicAirspaceHoldIssued -> fleetManagement "subscribes"
        topicAirspaceHoldLifted -> scheduler       "subscribes"
        topicAirspaceHoldLifted -> fulfillment     "subscribes"
        topicEmergencyRecall    -> fleetManagement "subscribes"
        topicEmergencyRecall    -> fulfillment     "subscribes"
        topicEmergencyRecall    -> auditLog        "subscribes"

        # ── Platform topics ───────────────────────────────────────────────────
        orderService    -> topicNotificationTrigger "publishes"
        paymentService  -> topicNotificationTrigger "publishes"
        fulfillment     -> topicNotificationTrigger "publishes"
        shipping        -> topicNotificationTrigger "publishes"
        inventory       -> topicNotificationTrigger "publishes"
        keycloak        -> topicAuditSecurity       "publishes"
        orderService    -> topicAuditBusiness       "publishes"
        paymentService  -> topicAuditBusiness       "publishes"
        fulfillment     -> topicAuditBusiness       "publishes"
        shipping        -> topicAuditBusiness       "publishes"
        fleetManagement -> topicAuditBusiness       "publishes"

        topicNotificationTrigger -> notifications "subscribes"
        topicAuditSecurity       -> auditLog      "subscribes"
        topicAuditBusiness       -> auditLog      "subscribes"
    }

    views {

        systemContext platform "SystemContext" "C4 Level 1 — System Context" {
            include *
            autoLayout tb
        }

        container platform "Containers" "C4 Level 2 — Containers" {
            include *
            autoLayout tb
        }

        # Focused view — Order domain event flow
        container platform "OrderDomainEvents" "Event flow: Order and Payment topics" {
            include orderService paymentService fulfillment inventory notifications auditLog
            include topicOrderCreated topicOrderCancelled topicOrderRefunded topicPaymentCaptured topicPaymentFailed
            autoLayout tb
        }

        # Focused view — Fulfillment saga
        container platform "FulfillmentSaga" "Event flow: Fulfillment saga topics" {
            include fulfillment inventory scheduler fleetManagement paymentService orderService notifications auditLog
            include topicSagaStepCompleted topicSagaStepFailed topicSagaCompensate topicInventoryReserved topicInventoryResFailed topicInventoryReleased topicDispatchScheduled topicDroneAssigned
            autoLayout tb
        }

        # Focused view — Drone telemetry
        container platform "DroneTelemetry" "Event flow: Drone and telemetry topics" {
            include telemetryHub fleetManagement fulfillment shipping geospatial regulatory scheduler auditLog redis
            include topicDroneAssigned topicDroneDeparted topicDroneReturned topicDroneFault topicTelemetryPing topicTelemetryBatch
            autoLayout tb
        }

        # Focused view — Shipping and airspace
        container platform "ShippingAirspace" "Event flow: Shipping and Fleet/Airspace topics" {
            include shipping fulfillment fleetManagement scheduler regulatory notifications auditLog
            include topicDeliveryCompleted topicDeliveryFailed topicEtaUpdated topicGeofenceEntered topicGeofenceExited
            include topicAirspaceViolation topicAirspaceHoldIssued topicAirspaceHoldLifted topicEmergencyRecall
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
            element "Topic" {
                shape Pipe
                background #e8a020
                color #000000
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
