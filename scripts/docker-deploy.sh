#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Smart Logistics — Docker full stack deploy"
docker compose up --build -d

echo ""
echo "==> Stack is up. Endpoints:"
echo "    Web UI:    http://localhost:4200"
echo "    Gateway:   http://localhost:5000/health"
echo "    Keycloak:  http://localhost:8080  (admin / admin)"
echo ""
echo "One command: docker compose up --build -d"
echo "Stop:        docker compose down"
