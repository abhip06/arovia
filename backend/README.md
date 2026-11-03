# Healthcare Store Backend

The backend is a .NET 10 REST API for the Healthcare Store web application. Its versioned endpoints cover authentication, users and addresses, catalog, cart and checkout, orders and payments, location, warehouses, and inventory.

## Implementation

The solution separates responsibilities into four projects:

- `HealthcareStore.Api` exposes HTTP endpoints and configures authentication, authorization, validation, and dependency injection.
- `HealthcareStore.Application` contains use cases and application abstractions.
- `HealthcareStore.Domain` contains core business entities and rules.
- `HealthcareStore.Infrastructure` implements persistence and external-service integrations.

PostgreSQL with EF Core stores transactional data, MongoDB stores product information, and Redis supports caching. The API also integrates with Cloudinary, Razorpay, Google OAuth, and SMTP email. Tests are grouped under `tests/` by layer.

## Configuration and local development

The API requires configuration for its databases, JWT signing, Google OAuth, email, payment, and media integrations. Keep secrets in environment variables or a local ignored `.env` file; do not commit them. Use the names and required values documented in `docker-compose.yml`.

With dependencies configured, run from this directory:

```bash
dotnet restore HealthcareStore.slnx
dotnet run --project src/HealthcareStore.Api
```

The API listens on port `8080` in the Docker configuration; versioned routes use the `/api/v1` prefix. Apply the EF Core migrations to PostgreSQL before first use because the API does not migrate the database at startup.

## VM deployment

For an Azure or AWS VM, deploy only this `backend/` directory and its deployment configuration. Provide the required environment values and reachable managed PostgreSQL, MongoDB, and Redis services, then run:

```bash
docker compose up --build -d
```

The Compose file binds the API to `127.0.0.1:8080`, so configure a host reverse proxy for public HTTPS access. Set `FRONTEND_ORIGIN` to the exact deployed frontend origin and configure secure refresh-token cookies for the frontend/API domain arrangement.

## Current implementation note

Prescription verification is currently implemented by a placeholder reader that always returns unverified.