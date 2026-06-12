# Smart Logistics Web (Angular)

Placeholder frontend for the Smart Logistics Platform.

## Planned stack

- Angular (Authorization Code Flow + PKCE via Keycloak)
- Keycloak client: `smartlogistics-web` (public client)
- API calls routed through YARP gateway

## Setup (when implemented)

```bash
npm install -g @angular/cli
ng new smartlogistics-web --routing --style=scss
cd smartlogistics-web
npm install keycloak-angular keycloak-js
ng serve
```

## Authentication flow

```
User → Angular → Keycloak Login → Authorization Code + PKCE → Access Token → API Gateway → Microservices
```

## User types

| Type | Roles |
|------|-------|
| Internal Staff | Admin, Operations, Finance, Sales |
| External | Customer, Agent, Carrier |
