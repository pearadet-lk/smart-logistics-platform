# Module Federation Remotes

Shell host: `web/smartlogistics-web` (:4200)  
Manifest: `web/smartlogistics-web/public/federation.manifest.json`

## Layout

```
web/
├── smartlogistics-web/          # Shell (host)
├── federation.manifest.json     # Dev reference copy
└── remotes/
    ├── shipment-remote/         # Federated shipment module (:4201)
    └── tariff-remote/           # Federated tariff module (:4202)
```

## Run shell standalone

```powershell
cd web/smartlogistics-web
npm start
```

The shell embeds shipment/tariff screens locally. When remotes are offline, `FederationLoaderService` keeps the shell fully functional.

## Add native federation remotes (optional)

From each remote folder:

```powershell
ng new shipment-remote --standalone --routing
cd shipment-remote
ng add @angular-architects/native-federation --project shipment-remote --port 4201 --type remote
```

Expose `./Component` entries and update `federation.manifest.json` URLs. Start remotes:

```powershell
ng serve shipment-remote --port 4201
ng serve tariff-remote --port 4202
```

The shell probes `remoteEntry.json` on startup and can delegate routes when remotes are available.
