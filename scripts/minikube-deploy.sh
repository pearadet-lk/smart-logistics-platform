#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAMESPACE="smartlogistics-local"
STATE_DIR="$ROOT/.minikube"
PF_FILE="$STATE_DIR/port-forwards.json"
SKIP_BUILD=false

while [[ $# -gt 0 ]]; do
  case $1 in
    --skip-build) SKIP_BUILD=true; shift ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

step() { echo "==> $*"; }

stop_existing_port_forwards() {
  if [[ -f "$PF_FILE" ]]; then
    step "Stopping existing port-forwards..."
    python3 - <<'PY' 2>/dev/null || true
import json, os, signal, sys
pf = os.path.join(os.environ["STATE_DIR"], "port-forwards.json")
with open(pf) as f:
    data = json.load(f)
items = data if isinstance(data, list) else [data]
for item in items:
    pid = item.get("pid")
    if pid:
        try:
            os.kill(int(pid), signal.SIGTERM)
        except ProcessLookupError:
            pass
PY
    rm -f "$PF_FILE"
  fi
}

start_port_forward() {
  local service=$1
  local local_port=$2
  local remote_port=$3
  kubectl port-forward -n "$NAMESPACE" "svc/$service" "${local_port}:${remote_port}" >/dev/null 2>&1 &
  local pid=$!
  echo "{\"service\":\"$service\",\"localPort\":$local_port,\"pid\":$pid}"
}

cd "$ROOT"
mkdir -p "$STATE_DIR"
export STATE_DIR

step "Smart Logistics — Minikube deploy"

command -v minikube >/dev/null || { echo "minikube not found"; exit 1; }
command -v kubectl >/dev/null || { echo "kubectl not found"; exit 1; }

if ! minikube status >/dev/null 2>&1; then
  step "Starting Minikube..."
  minikube start --cpus 4 --memory 8192
else
  step "Minikube is already running."
fi

stop_existing_port_forwards

if [[ "$SKIP_BUILD" != "true" ]]; then
  step "Building images inside Minikube Docker daemon..."
  eval "$(minikube docker-env)"
  docker build -t smartlogistics/gateway:local -f docker/Dockerfile.gateway .
  docker build -t smartlogistics/shipment-api:local -f docker/Dockerfile.shipment-api .
  docker build -t smartlogistics/tariff-api:local -f docker/Dockerfile.tariff-api .
  docker build -t smartlogistics/billing-api:local -f docker/Dockerfile.billing-api .
  docker build -t smartlogistics/notification-api:local -f docker/Dockerfile.notification-api .
fi

step "Applying Kubernetes manifests..."
kubectl apply -k infrastructure/k8s/minikube

step "Waiting for deployments..."
for dep in postgres keycloak shipment-api tariff-api billing-api notification-api gateway; do
  kubectl rollout status "deployment/$dep" -n "$NAMESPACE" --timeout=300s
done

step "Starting automatic port-forwards..."
sleep 2

FORWARDS="["
FORWARDS+=$(start_port_forward gateway 5000 80)
FORWARDS+=","
FORWARDS+=$(start_port_forward keycloak 8080 8080)
FORWARDS+="]"
echo "$FORWARDS" > "$PF_FILE"

echo ""
echo "==> Minikube stack is ready (port-forwards running in background)."
echo "    Gateway:   http://localhost:5000/health"
echo "    Keycloak:  http://localhost:8080  (admin / admin)"
echo ""
echo "Teardown:  ./scripts/minikube-teardown.sh"
