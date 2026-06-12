#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "==> Stopping Smart Logistics Docker stack..."
docker compose -f docker/docker-compose.yml -f docker/docker-compose.local.yml down
echo "==> Done."
