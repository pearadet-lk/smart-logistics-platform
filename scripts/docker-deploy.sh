#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Smart Logistics — Docker local deploy"
docker compose -f docker/docker-compose.yml -f docker/docker-compose.local.yml up --build -d

echo ""
echo "==> Stack is up. Endpoints:"
echo "    Gateway:   http://localhost:5000/health"
echo "    Keycloak:  http://localhost:8080  (admin / admin)"
echo ""
echo "Stop:  ./scripts/docker-teardown.sh"
