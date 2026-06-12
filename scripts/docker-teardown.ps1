$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

Write-Host "==> Stopping Smart Logistics Docker stack..." -ForegroundColor Cyan
Set-Location $Root
docker compose -f docker/docker-compose.yml -f docker/docker-compose.local.yml down
Write-Host "==> Done." -ForegroundColor Green
