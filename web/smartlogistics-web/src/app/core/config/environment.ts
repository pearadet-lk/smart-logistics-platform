/** Local development defaults — Keycloak PKCE integration TODO */
export const environment = {
  production: false,
  appName: 'Smart Logistics Platform',
  apiGatewayUrl: 'http://localhost:5000',
  keycloak: {
    url: 'http://localhost:8080',
    realm: 'SmartLogistics',
    clientId: 'smartlogistics-web',
  },
};
