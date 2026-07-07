# Deployment

## Status: Azure removed, migration pending

This project previously deployed to Azure Web App (resource name `remedin`) via a GitHub Actions
workflow (`.github/workflows/main_doto.yml`, now removed). That Azure infrastructure is no longer
used and the workflow has been deleted.

## Next steps: pick a free hosting alternative

Candidates for a free/low-cost .NET 10 web API host, to be evaluated and set up:

- **Render** (recommended starting point) — free web service tier, Docker or native runtime
  support for .NET, easy GitHub-integrated auto-deploy on push to `main`. Note: free tier spins
  down on inactivity (cold starts).
- **Fly.io** — free allowance, requires a Dockerfile, better for always-on small apps than Render's
  free tier.
- **Railway** — usage-based free credits rather than a permanent free tier.

## What deployment needs, regardless of host

- Environment variables / secrets (currently only in `appsettings.Development.json`, which has
  live secrets checked into git — **must be rotated and moved to the host's secret manager before
  any new deploy**, not committed):
  - Postgres connection string (Supabase-hosted Postgres — hosting change does not affect this)
  - `SUPABASE_PROJECT_REF`, `SUPABASE_JWT_SECRET`, `SUPABASE_SERVICE_ROLE_KEY`
- A `Dockerfile` if the chosen host requires containerized deploys (none exists yet in this repo).
- A new GitHub Actions workflow (or the host's native GitHub integration) once a target is chosen.

No CI/CD workflow exists in this repo right now — add one once the hosting target is decided.
