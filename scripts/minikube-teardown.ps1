$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Namespace = "smartlogistics-local"
$StateDir = Join-Path $Root ".minikube"
$PfFile = Join-Path $StateDir "port-forwards.json"

Write-Host "==> Stopping port-forwards..." -ForegroundColor Cyan
if (Test-Path $PfFile) {
    Get-Content $PfFile -Raw | ConvertFrom-Json | ForEach-Object {
        if ($_.pid) {
            Stop-Process -Id $_.pid -Force -ErrorAction SilentlyContinue
            Write-Host "    stopped $($_.service) (pid $($_.pid))"
        }
    }
    Remove-Item $PfFile -Force -ErrorAction SilentlyContinue
}

Write-Host "==> Deleting namespace $Namespace..." -ForegroundColor Cyan
kubectl delete namespace $Namespace --ignore-not-found=true

Write-Host "==> Done. Minikube cluster is still running." -ForegroundColor Green
Write-Host "    Stop Minikube: minikube stop" -ForegroundColor Yellow
