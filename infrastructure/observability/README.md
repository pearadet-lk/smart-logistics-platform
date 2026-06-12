# OpenTelemetry → Prometheus → Grafana (placeholder)

# TODO: Add OTel Collector, Prometheus, and Grafana to docker-compose when implementing observability.

# Metrics to monitor:
# - Requests/sec
# - Response time (p50, p95, p99)
# - Error rate
# - Authentication failures

# Each .NET service should register:
# - OpenTelemetry.Instrumentation.AspNetCore
# - OpenTelemetry.Exporter.Prometheus.AspNetCore (or OTLP exporter → Collector)
