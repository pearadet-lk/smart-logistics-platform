$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

Write-Host "==> Smart Logistics - Docker full stack deploy" -ForegroundColor Cyan
Write-Host "    Root: $Root"

Set-Location $Root

Write-Host "==> Building and starting all services (infra + APIs + web)..." -ForegroundColor Cyan
docker compose up --build -d

Write-Host ""
Write-Host "==> Stack is up. Endpoints:" -ForegroundColor Green
Write-Host "    Web UI:    http://localhost:4200"
Write-Host "    Gateway:   http://localhost:5000/health"
Write-Host "    Keycloak:  http://localhost:8080  (admin / admin)"
Write-Host "    Postgres:  localhost:5432"
Write-Host "    Jaeger:    http://localhost:16686"
Write-Host "    Seq:       http://localhost:8081"
Write-Host "    Grafana:   http://localhost:3000  (admin / admin)"
Write-Host "    Kibana:    http://localhost:5601"
Write-Host "    Redis:     localhost:6379"
Write-Host ""
Write-Host "One command (same as this script):" -ForegroundColor Cyan
Write-Host "    docker compose up --build -d"
Write-Host ""
Write-Host "Stop:  docker compose down" -ForegroundColor Yellow
Write-Host "       ./scripts/docker-teardown.ps1"
