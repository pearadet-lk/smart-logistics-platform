param(
    [int]$MinikubeCpus = 4,
    [int]$MinikubeMemoryMb = 8192,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$Namespace = "smartlogistics-local"
$StateDir = Join-Path $Root ".minikube"
$PfFile = Join-Path $StateDir "port-forwards.json"

function Write-Step($Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Stop-ExistingPortForwards {
    if (Test-Path $PfFile) {
        Write-Step "Stopping existing port-forwards..."
        Get-Content $PfFile -Raw | ConvertFrom-Json | ForEach-Object {
            if ($_.pid) {
                Stop-Process -Id $_.pid -Force -ErrorAction SilentlyContinue
            }
        }
        Remove-Item $PfFile -Force -ErrorAction SilentlyContinue
    }
}

function Start-PortForward {
    param(
        [string]$Service,
        [int]$LocalPort,
        [int]$RemotePort
    )

    $proc = Start-Process -FilePath "kubectl" -ArgumentList @(
        "port-forward",
        "-n", $Namespace,
        "svc/$Service",
        "${LocalPort}:${RemotePort}"
    ) -PassThru -WindowStyle Hidden

    return [PSCustomObject]@{
        service   = $Service
        localPort = $LocalPort
        pid       = $proc.Id
    }
}

function Build-LocalImages {
    Write-Step "Building images inside Minikube Docker daemon..."
    Invoke-Expression (minikube docker-env --shell powershell | Out-String)

    $images = @(
        @{ Name = "smartlogistics/gateway:local"; Dockerfile = "docker/Dockerfile.gateway" },
        @{ Name = "smartlogistics/shipment-api:local"; Dockerfile = "docker/Dockerfile.shipment-api" },
        @{ Name = "smartlogistics/tariff-api:local"; Dockerfile = "docker/Dockerfile.tariff-api" },
        @{ Name = "smartlogistics/billing-api:local"; Dockerfile = "docker/Dockerfile.billing-api" },
        @{ Name = "smartlogistics/notification-api:local"; Dockerfile = "docker/Dockerfile.notification-api" }
    )

    foreach ($img in $images) {
        Write-Host "    docker build -t $($img.Name) -f $($img.Dockerfile) ."
        docker build -t $img.Name -f $img.Dockerfile .
        if ($LASTEXITCODE -ne 0) { throw "Docker build failed for $($img.Name)" }
    }
}

Set-Location $Root
New-Item -ItemType Directory -Force -Path $StateDir | Out-Null

Write-Step "Smart Logistics — Minikube deploy"

if (-not (Get-Command minikube -ErrorAction SilentlyContinue)) {
    throw "minikube not found. Install: https://minikube.sigs.k8s.io/docs/start/"
}
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    throw "kubectl not found. Install kubectl and ensure it is on PATH."
}

$status = minikube status --format='{{.Host}}' 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($status)) {
    Write-Step "Starting Minikube (cpus=$MinikubeCpus, memory=${MinikubeMemoryMb}mb)..."
    minikube start --cpus $MinikubeCpus --memory $MinikubeMemoryMb
} else {
    Write-Step "Minikube is already running."
}

Stop-ExistingPortForwards

if (-not $SkipBuild) {
    Build-LocalImages
}

Write-Step "Applying Kubernetes manifests..."
kubectl apply -k infrastructure/k8s/minikube

Write-Step "Waiting for deployments..."
$deployments = @("postgres", "keycloak", "shipment-api", "tariff-api", "billing-api", "notification-api", "gateway")
foreach ($dep in $deployments) {
    kubectl rollout status "deployment/$dep" -n $Namespace --timeout=300s
    if ($LASTEXITCODE -ne 0) { throw "Rollout failed for deployment/$dep" }
}

Write-Step "Starting automatic port-forwards..."
Start-Sleep -Seconds 2

$forwards = @(
    (Start-PortForward -Service "gateway" -LocalPort 5000 -RemotePort 80),
    (Start-PortForward -Service "keycloak" -LocalPort 8080 -RemotePort 8080)
)

$forwards | ConvertTo-Json | Set-Content $PfFile

Write-Host ""
Write-Host "==> Minikube stack is ready (port-forwards running in background)." -ForegroundColor Green
Write-Host "    Gateway:   http://localhost:5000/health"
Write-Host "    Keycloak:  http://localhost:8080  (admin / admin)"
Write-Host ""
Write-Host "    Port-forward PIDs saved to: $PfFile"
Write-Host ""
Write-Host "Teardown:  ./scripts/minikube-teardown.ps1" -ForegroundColor Yellow
