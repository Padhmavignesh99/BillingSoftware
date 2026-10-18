# Billing Software UI

## Run locally

1. Configure the database connection in `API/appsettings.json` and apply the API migrations.
2. From the repository root, run `dotnet run --project API/BillingSoftware.API.csproj --launch-profile http`. The API listens on `http://localhost:5224`.
3. From `UI/Billing software`, run `npm ci` and `npm run dev`. Open the Vite URL shown in the terminal.

Vite proxies `/api` and `/health` to the local API. The UI requires a real account; register a business or sign in. API errors are shown in the form, and failed saves do not appear as successful local records.

For a deployed build, serve the UI and API under one origin with `/api` and `/health` routed to the API, or set `VITE_API_BASE_URL` to the API origin before `npm run build`. Configure the API CORS policy for that UI origin when using separate origins.

## Checks

- `npm run build` checks TypeScript and builds the UI.
- `dotnet build API/BillingSoftware.API.csproj` checks the API.