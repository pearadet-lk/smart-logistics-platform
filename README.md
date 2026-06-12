# Smart Logistics Platform

Cloud-native logistics platform demonstrating OIDC/OAuth2, microservices, event-driven architecture, and Kubernetes deployment patterns.

> **Status:** Placeholder scaffold — business logic, persistence, and integrations are TODO.

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

## Repository structure

```
smart-logistics-platform/
├── src/
│   ├── Gateway/SmartLogistics.Gateway/          # YARP — JWT, routing, correlation ID
│   ├── Services/
│   │   ├── Shipment/SmartLogistics.Shipment.Api/  # ShipmentDB
│   │   ├── Tariff/SmartLogistics.Tariff.Api/      # TariffDB
│   │   ├── Billing/SmartLogistics.Billing.Api/    # BillingDB
│   │   └── Notification/SmartLogistics.Notification.Api/
│   └── Shared/SmartLogistics.Shared/              # Audit, events, role constants
├── web/smartlogistics-web/                        # Angular (placeholder)
├── docker/                                        # docker-compose, Dockerfiles
├── infrastructure/
│   ├── keycloak/                                  # Realm config (SmartLogistics)
│   ├── k8s/                                       # AKS/EKS manifests (dev/uat/prod)
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

### Start infrastructure

```powershell
docker compose -f docker/docker-compose.yml up -d
```

Starts: PostgreSQL, Keycloak (`http://localhost:8080`, admin/admin), Kafka.

### Run services

```powershell
dotnet build SmartLogistics.sln

# Terminal 1 — Gateway (port 5000)
dotnet run --project src/Gateway/SmartLogistics.Gateway

# Terminal 2 — Shipment API (5101)
dotnet run --project src/Services/Shipment/SmartLogistics.Shipment.Api

# Terminal 3 — Tariff API (5102)
dotnet run --project src/Services/Tariff/SmartLogistics.Tariff.Api

# Terminal 4 — Billing API (5103)
dotnet run --project src/Services/Billing/SmartLogistics.Billing.Api

# Terminal 5 — Notification API (5104)
dotnet run --project src/Services/Notification/SmartLogistics.Notification.Api
```

### Health checks

```powershell
curl http://localhost:5000/health
curl http://localhost:5101/health
```

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

Namespaces: `dev`, `uat`, `prod`

Manifests under `infrastructure/k8s/` — replace `ghcr.io/OWNER/*` image tags and wire secrets from Key Vault / Secrets Manager before deploying to AKS or EKS.

## CI/CD

GitHub Actions (`.github/workflows/ci-cd.yml`):

```
GitHub → GitHub Actions → Docker Build → Container Registry → AKS/EKS
```

## Interview talking points

This project demonstrates:

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
