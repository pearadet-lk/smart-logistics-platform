$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

Write-Host "==> Smart Logistics — Docker local deploy" -ForegroundColor Cyan
Write-Host "    Root: $Root"

Set-Location $Root

Write-Host "==> Building and starting full stack..." -ForegroundColor Cyan
docker compose -f docker/docker-compose.yml -f docker/docker-compose.local.yml up --build -d

Write-Host ""
Write-Host "==> Stack is up. Endpoints:" -ForegroundColor Green
Write-Host "    Gateway:   http://localhost:5000/health"
Write-Host "    Keycloak:  http://localhost:8080  (admin / admin)"
Write-Host "    Postgres:  localhost:5432"
Write-Host "    Kafka:     localhost:9092"
Write-Host ""
Write-Host "    curl http://localhost:5000/health"
Write-Host ""
Write-Host "Stop:  ./scripts/docker-teardown.ps1" -ForegroundColor Yellow
