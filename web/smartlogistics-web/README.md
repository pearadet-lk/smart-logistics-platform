# Smart Logistics Web

Angular **22** SPA for the Smart Logistics Platform.

## Stack

- Angular 22 (standalone components)
- Keycloak client: `smartlogistics-web` (Authorization Code + PKCE — TODO)
- API Gateway: `http://localhost:5000`

## Development

```bash
cd web/smartlogistics-web
npm install
npm start
```

Open http://localhost:4200

## Build

```bash
npm run build
```

Output: `dist/smartlogistics-web/`

## Next steps (placeholder)

- [ ] Integrate `keycloak-angular` with PKCE
- [ ] Route guards for realm roles (Customer, Carrier, Admin, …)
- [ ] HTTP interceptor for Bearer token to API Gateway
- [ ] Feature modules: Shipments, Tariffs, Invoices
