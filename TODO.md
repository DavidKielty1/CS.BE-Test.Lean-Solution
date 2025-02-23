#### TODOs:

Additional Metrics Coverage with Prometheus

Rate Limiter

Utilize Automated Swagger Docs Package

## Docker Containerization (Not fully implemented yet!)

## Quick Start with Docker

1. Clone the repository
2. Update the API endpoints in docker-compose.yml
3. Run everything with:
   ```bash
   docker-compose up -d
   ```

This will start:

- Credit Card API (http://localhost:5000)
- Redis Cache
- Prometheus (http://localhost:9090)
- Grafana (http://localhost:3000)

Default Grafana login: admin/admin
