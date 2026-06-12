#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAMESPACE="smartlogistics-local"
PF_FILE="$ROOT/.minikube/port-forwards.json"

echo "==> Stopping port-forwards..."
if [[ -f "$PF_FILE" ]]; then
  python3 - <<'PY' 2>/dev/null || true
import json, os, signal
pf = os.path.join(os.environ["ROOT"], ".minikube", "port-forwards.json")
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
export ROOT

echo "==> Deleting namespace $NAMESPACE..."
kubectl delete namespace "$NAMESPACE" --ignore-not-found=true

echo "==> Done. Minikube cluster is still running."
echo "    Stop Minikube: minikube stop"
