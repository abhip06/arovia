# Healthcare Store

Healthcare Store is a full-stack online pharmacy storefront. Customers can browse products, manage an account and cart, check out, and view orders. The backend also provides catalog, warehouse, and inventory operations for staff.

## Project layout

| Directory | Contents |
| --- | --- |
| `frontend/` | Next.js and React web application; deploy this directory to Vercel. |
| `backend/` | .NET API, application/domain/infrastructure projects, tests, and API-only Docker deployment files. |
| `docker-compose.yml` | Root-level local orchestration for the API and optional frontend. |

The backend is organized using Clean Architecture. PostgreSQL stores transactional data, MongoDB stores product information, and Redis is used for caching. Integrations include Cloudinary for media, Razorpay for payments, Google OAuth, and SMTP email.

## Run locally with Docker Compose

Provide the required API settings and connection strings in `backend/.env` (or another Compose environment file). Do not commit credentials. PostgreSQL, MongoDB, and Redis must be available at the configured addresses; Compose does not start database containers.

From the project root, start the API and frontend:

```bash
docker compose --env-file backend/.env --profile local up --build
```

The web app is available at `http://localhost:3000` and the API at `http://localhost:8080`. Without `--profile local`, the root Compose file starts only the API. Stop the stack with `docker compose --env-file backend/.env --profile local down`.

Apply the backend's EF Core migrations to PostgreSQL before using the API against a new database. The application does not apply migrations automatically.

## Deployment

- **Frontend:** Deploy `frontend/` on Vercel and set `NEXT_PUBLIC_API_BASE_URL` to the API base URL ending in `/api`, for example `https://api.example.com/api`.
- **Backend VM:** Deploy the `backend/` directory on the Azure or AWS VM and use its `docker-compose.yml`. Configure the required secrets and managed service connection strings in the VM's environment. The API container binds to `127.0.0.1:8080`; put an HTTPS reverse proxy in front of it and set `FRONTEND_ORIGIN` to the exact Vercel/custom frontend origin.

See [frontend/README.md](frontend/README.md) and [backend/README.md](backend/README.md) for component-specific setup and deployment notes.

## Current implementation notes

- Prescription verification for order processing is currently backed by a placeholder reader that always reports unverified.
