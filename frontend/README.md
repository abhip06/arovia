# Healthcare Store Frontend

The frontend is a Next.js App Router application built with React and TypeScript. It provides the customer storefront and account flows, including product discovery, cart and checkout, addresses, profile, and order history, along with informational pages and administrative screens.

## Implementation

- Next.js standalone output is enabled for container builds.
- Tailwind CSS is used for styling; React Hook Form and Zod handle forms and validation.
- Axios is the HTTP client, TanStack Query manages server data, and Zustand holds client-side state.
- The API base URL is read from `NEXT_PUBLIC_API_BASE_URL`; include the `/api` prefix, for example `http://localhost:8080/api`.

## Local development

Use Node.js and pnpm (the package manager version is declared in `package.json`):

```bash
pnpm install --frozen-lockfile
pnpm dev
```

Open `http://localhost:3000`. Useful project checks are:

```bash
pnpm lint
pnpm build
```

The API must be running and reachable for data-backed flows. Set `NEXT_PUBLIC_API_BASE_URL` in the frontend environment when it is not the local default.

## Vercel deployment

Set the Vercel project root directory to `frontend`. Configure `NEXT_PUBLIC_API_BASE_URL` with the deployed API URL ending in `/api` (for example, `https://api.example.com/api`) and configure the API's CORS allowed origin to the deployed frontend origin. This value is embedded into the client build, so redeploy after changing it.
