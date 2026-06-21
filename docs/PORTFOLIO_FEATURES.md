# Portfolio Features — Implementation Status

## Phase 1 — Identity & Security ✅

Keycloak OIDC + PKCE, SSO clients, fine-grained permissions, route guards, dynamic navigation.

## Phase 2 — Logistics ✅

| Feature | Status | Location |
|---------|--------|----------|
| Shipment tracking timeline | ✅ EF Core + PostgreSQL | `ShipmentDbContext`, tracking API |
| Container tracking | ✅ | `ContainerTrackings` table |
| Tariff versioning | ✅ | `TariffDbContext`, workflow API |
| Tariff approval UI | ✅ | `tariff-approval.component.ts` + workflow API |
| Exchange rates | ✅ | Daily import scheduler + `/api/tariffs/currency-rates` |
| File upload (tariff) | ✅ | `POST /api/tariffs/upload` + Angular progress bar |

## Phase 3 — Event Driven ✅

| Feature | Status | Location |
|---------|--------|----------|
| Kafka producer | ✅ | `KafkaEventPublisher` |
| DB-backed Outbox | ✅ | `OutboxMessages` + `OutboxPublisherService` |
| Billing consumer | ✅ | `ShipmentCreatedConsumer` + `BillingDbContext` |
| SignalR notifications | ✅ | `NotificationHub` + Kafka consumer |
| Real-time Angular | ✅ | `@microsoft/signalr` + `NotificationStore` |

## Phase 4 — Cloud Native ✅

| Feature | Status | Location |
|---------|--------|----------|
| OpenTelemetry → Jaeger | ✅ | OTLP exporter, `http://localhost:16686` |
| Serilog → Seq | ✅ | Structured logs, `http://localhost:8081` |
| Redis cache | ✅ | Tariffs + exchange rates cache-aside |
| Health checks | ✅ | `/health`, `/health/live`, `/health/ready` |
| Rate limiting | ✅ | Gateway YARP |
| Feature flags API | ✅ | `GET /api/feature-flags` |

## Phase 5+ — Enterprise ✅

| Feature | Status | Location |
|---------|--------|----------|
| EF migrations | ✅ | `Migrations/` + `Database.Migrate()` on startup |
| Approval workflow | ✅ | `WorkflowInstance`, `WorkflowService`, `/api/tariffs/workflow` |
| Audit API (IP + TraceId) | ✅ | `AuditController`, `ShipmentService.CreateAsync` |
| AI Assistant | ✅ | `SmartLogistics.Ai.Api`, `/api/ai/chat`, Angular assistant |
| S2S Client Credentials | ✅ | `ClientCredentialsTokenProvider` |
| Key Vault bootstrap | ✅ | `AddSmartLogisticsKeyVault` |
| Elasticsearch + Serilog sink | ✅ | Docker ES/Kibana + `SerilogExtensions` |
| Module Federation shell | ✅ | `federation.manifest.json`, `FederationLoaderService` |
| K8s HPA | ✅ | `infrastructure/k8s/hpa/hpa.yaml` |

## Angular 22 Frontend ✅

| Feature | Status |
|---------|--------|
| NgRx Signal Store | ✅ notifications, feature flags |
| SignalR real-time | ✅ |
| AG Grid | ✅ shipments list |
| Leaflet map | ✅ tracking screen |
| ngx-echarts dashboard | ✅ |
| File upload + progress | ✅ tariffs |
| i18n (EN/TH) | ✅ Transloco |
| Dark mode | ✅ |
| PWA + offline cache | ✅ service worker |
| AI chat UI | ✅ feature-flag gated |
| Live audit + workflow UI | ✅ |

## Infrastructure (Docker)

| Service | URL |
|---------|-----|
| Gateway | http://localhost:5000 |
| Keycloak | http://localhost:8080 |
| Jaeger UI | http://localhost:16686 |
| Seq logs | http://localhost:8081 |
| Elasticsearch | http://localhost:9200 |
| Kibana | http://localhost:5601 |
| Redis | localhost:6379 |

## Run

```powershell
./scripts/docker-deploy.ps1
cd web/smartlogistics-web && npm start
```

Configure Azure OpenAI (optional) via `AzureOpenAI:Endpoint`, `ApiKey`, `Deployment` on `ai-api`.
