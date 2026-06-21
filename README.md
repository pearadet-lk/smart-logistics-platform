# Smart Logistics Platform

Cloud-native logistics platform demonstrating OIDC/OAuth2, microservices, event-driven architecture, and Kubernetes deployment patterns.

> **Status:** Enterprise portfolio — Phases 1–5+ implemented: Keycloak OIDC, EF migrations, workflow, audit, AI assistant, Elasticsearch, Module Federation shell, K8s HPA.

See [docs/PORTFOLIO_FEATURES.md](docs/PORTFOLIO_FEATURES.md) for full feature matrix.

## Business domain

| Entity | Service |
|--------|---------|
| Customers, Shippers, Carriers, Shipments, Tracking | Shipment API |
| Tariffs, Local Charges, Currency Rates | Tariff API |
| Invoices, Payments, Tax | Billing API |
| Event-driven notifications | Notification API |

## Architecture

```
Internet → Ingress/WAF → API Gateway (YARP) → Microservices (.NET)
                              ↓
                         Event Bus (Kafka) → Notification Service
                              ↓
                         Keycloak (SSO) + PostgreSQL (per service)
```

### API Gateway routes

| Route | Backend |
|-------|---------|
| `/api/shipments` | Shipment API |
| `/api/tariffs` | Tariff API |
| `/api/invoices` | Billing API |
| `/api/notifications` | Notification API |
| `/api/audit` | Shipment API (admin) |
| `/api/ai` | AI Assistant API |

## Repository structure

```
smart-logistics-platform/
├── src/
│   ├── Gateway/SmartLogistics.Gateway/          # YARP — JWT, routing, correlation ID
│   ├── Services/
│   │   ├── Shipment/SmartLogistics.Shipment.Api/  # ShipmentDB
│   │   ├── Tariff/SmartLogistics.Tariff.Api/      # TariffDB
│   │   ├── Billing/SmartLogistics.Billing.Api/    # BillingDB
│   │   ├── Notification/SmartLogistics.Notification.Api/
│   │   └── Ai/SmartLogistics.Ai.Api/              # Azure OpenAI / mock assistant
│   └── Shared/SmartLogistics.Shared/              # Audit, events, workflow, auth
├── web/smartlogistics-web/                        # Angular 22 shell (Module Federation host)
├── web/remotes/                                   # Federated micro-frontends (optional)
├── docker/                                        # docker-compose, Dockerfiles
├── scripts/                                       # docker-deploy, minikube-deploy (+ auto port-forward)
├── infrastructure/
│   ├── keycloak/                                  # Realm config (SmartLogistics)
│   ├── k8s/                                       # AKS/EKS + minikube manifests
│   └── observability/                             # OpenTelemetry → Prometheus → Grafana
└── .github/workflows/ci-cd.yml
```

## Keycloak design

- **Realm:** `SmartLogistics` (single realm)
- **Frontend client:** `smartlogistics-web` — Public, Authorization Code + PKCE
- **API clients:** `shipment-api`, `tariff-api`, `billing-api`, `notification-api` — Bearer Only
- **M2M client:** `integration-client` — Client Credentials (service-to-service)

### Realm roles

`Admin`, `Operations`, `Finance`, `Sales`, `Partner`, `Customer`, `Carrier`

### Groups

```
Customers → DHL, FedEx, UPS
Carriers  → Maersk, ONE
```

### Example JWT claims

```json
{
  "sub": "123",
  "preferred_username": "john",
  "company": "DHL",
  "partnerId": "1001",
  "roles": ["Customer"]
}
```

## Local development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Option A — Docker full stack (recommended)

One command from the repository root — infrastructure, all APIs, gateway, and Angular web:

```powershell
docker compose up --build -d
```

Or use the helper script:

```powershell
./scripts/docker-deploy.ps1
```

| Endpoint | URL |
|----------|-----|
| **Web UI (Angular)** | http://localhost:4200 |
| Gateway | http://localhost:5000/health |
| Keycloak | http://localhost:8080 (admin / admin) |
| Jaeger (tracing) | http://localhost:16686 |
| Seq (logs) | http://localhost:8081 |
| Kibana | http://localhost:5601 |
| **Grafana** | http://localhost:3000 (admin / admin) |
| Elasticsearch | http://localhost:9200 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |
| Kafka | localhost:9092 |

Stop the stack:

```powershell
docker compose down
# or
./scripts/docker-teardown.ps1
```

Infrastructure only (no .NET / web builds):

```powershell
docker compose -f docker/docker-compose.yml up -d
```

### Option B — Minikube with automatic port-forward

Deploys the full stack to Minikube, builds images into the Minikube Docker daemon, and starts background port-forwards to `localhost`:

```powershell
./scripts/minikube-deploy.ps1
```

Port-forwards start automatically (no manual `kubectl port-forward`):

| Service | Local URL |
|---------|-----------|
| Gateway | http://localhost:5000 |
| Keycloak | http://localhost:8080 |

PIDs are saved to `.minikube/port-forwards.json`. Teardown stops port-forwards and removes the namespace:

```powershell
./scripts/minikube-teardown.ps1
```

Options:

```powershell
# Skip image rebuild on redeploy
./scripts/minikube-deploy.ps1 -SkipBuild

# Custom Minikube resources
./scripts/minikube-deploy.ps1 -MinikubeCpus 4 -MinikubeMemoryMb 8192
```

Linux/macOS equivalents: `./scripts/minikube-deploy.sh` and `./scripts/minikube-teardown.sh`.

### Option C — Infrastructure only + dotnet run

Start infrastructure:

```powershell
docker compose -f docker/docker-compose.yml up -d
```

Run services locally:

```powershell
dotnet build SmartLogistics.sln

dotnet run --project src/Gateway/SmartLogistics.Gateway          # :5000
dotnet run --project src/Services/Shipment/SmartLogistics.Shipment.Api   # :5101
dotnet run --project src/Services/Tariff/SmartLogistics.Tariff.Api       # :5102
dotnet run --project src/Services/Billing/SmartLogistics.Billing.Api    # :5103
dotnet run --project src/Services/Notification/SmartLogistics.Notification.Api # :5104
```

### Health checks

```powershell
curl http://localhost:5000/health
curl http://localhost:5101/health
```

### Frontend (Angular 22)

Included in the Docker full stack at http://localhost:4200.

For local development without Docker:

```powershell
cd web/smartlogistics-web
npm install
npm start
```

Open http://localhost:4200 — Keycloak login, dashboard charts, AG Grid, Leaflet map, SignalR notifications, EN/TH i18n, dark mode.

Full feature matrix: [docs/PORTFOLIO_FEATURES.md](docs/PORTFOLIO_FEATURES.md)

## Security

- **Authentication:** OIDC via Keycloak
- **Authorization:** RBAC — roles, groups, client roles, custom claims
- **Secrets:** Use Azure Key Vault or AWS Secrets Manager — never commit connection strings

## Event-driven design

```
Shipment Created → Kafka (shipment.created) → Billing, Notification, Reporting
```

Avoid direct service-to-service HTTP where events suffice. Use Client Credentials (no shared API keys) when services must call each other.

## Database per service

| Database | Service |
|----------|---------|
| ShipmentDB | Shipment API |
| TariffDB | Tariff API |
| BillingDB | Billing API |

Each service includes an `AuditLog` model placeholder for login, CRUD, and change tracking.

## Kubernetes

### Minikube (local)

Manifests: `infrastructure/k8s/minikube/` — namespace `smartlogistics-local`, local images with `imagePullPolicy: Never`.

```powershell
kubectl apply -k infrastructure/k8s/minikube
```

Use `./scripts/minikube-deploy.ps1` for build + deploy + automatic port-forward.

### AKS / EKS (dev / uat / prod)

Namespaces: `dev`, `uat`, `prod`

Manifests under `infrastructure/k8s/` — replace `ghcr.io/OWNER/*` image tags and wire secrets from Key Vault / Secrets Manager before deploying.

## CI/CD

GitHub Actions (`.github/workflows/ci-cd.yml`):

```
GitHub → GitHub Actions → Docker Build → Container Registry → AKS/EKS
```

## This project demonstrates

- OpenID Connect (OIDC) and OAuth2
- Keycloak — realms, clients, roles, groups, claims
- Authorization Code Flow + PKCE (never Implicit Flow)
- Service-to-service authentication (Client Credentials)
- API Gateway pattern (YARP)
- Microservices with database-per-service
- Event-driven architecture (Kafka)
- Audit trail design
- Observability (OpenTelemetry, Prometheus, Grafana)
- Kubernetes and CI/CD
- Cloud-native security (Key Vault, no secrets in source)

## License

See [LICENSE](LICENSE).
