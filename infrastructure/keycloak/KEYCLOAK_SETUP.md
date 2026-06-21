# Keycloak — MFA, SSO, Social Login

## MFA (TOTP) — enforced

The realm import (`smartlogistics-realm.json`) configures:

- **Browser flow:** `SmartLogistics Browser MFA` (username/password + OTP)
- **OTP policy:** TOTP 6 digits, 30s period (Google / Microsoft Authenticator)
- **Demo users** `admin` / `finance` have `CONFIGURE_TOTP` required on first login

| User | Password | Roles | MFA |
|------|----------|-------|-----|
| admin@smartlogistics.local | admin | Admin | Required |
| finance@smartlogistics.local | finance | Finance | Required |
| ops@smartlogistics.local | ops | Operations | Optional |

After first login, scan the QR code with an authenticator app.

## SSO (multi-portal)

Three OIDC clients share the same realm session:

| Client | Portal |
|--------|--------|
| `smartlogistics-web` | Customer |
| `admin-web` | Admin |
| `partner-web` | Partner |

Login once at Keycloak — session applies across clients on `localhost:4200`.

## Social login — GitHub (enabled)

GitHub IdP is **enabled** in the realm with placeholder credentials.

1. Create a GitHub OAuth App: https://github.com/settings/developers  
   - Homepage: `http://localhost:4200`  
   - Callback: `http://localhost:8080/realms/SmartLogistics/broker/github/endpoint`
2. In Keycloak Admin → Identity Providers → GitHub → set Client ID and Secret  
   (or re-import realm after editing `smartlogistics-realm.json`)

Google and Microsoft remain disabled placeholders.

## Manual MFA override (Admin console)

Authentication → Flows → **SmartLogistics Browser MFA** → ensure `OTP Form` is **Required**.
