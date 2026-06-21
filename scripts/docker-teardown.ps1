$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

Write-Host "==> Stopping Smart Logistics Docker stack..." -ForegroundColor Cyan
Set-Location $Root
docker compose down
Write-Host "==> Done." -ForegroundColor Green
