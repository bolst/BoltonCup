# music-fetcher

Drains a tournament's **requested-but-missing** music list. For each player song
request that has a Spotify track id but no uploaded file, it downloads the track
with [spotDL](https://spotdl.com), uploads it to R2 via the API's presigned-upload
flow, and registers it tagged with the Spotify track id so the playlist composer
matches it.

Requests with no Spotify track id (free-text name only) can't be matched by the
composer, so they are skipped and listed in the run report for manual handling.

Re-runs are safe: once a track is registered it stops appearing in the missing
list, so the job never duplicates and automatically retries anything that failed.

## Setup

1. Create a Spotify app at https://developer.spotify.com/dashboard for the client
   id/secret (used only for metadata lookup and by spotDL).
2. On the API, set `BoltonCup:Authentication:AdminApiKey` (the key this tool sends)
   and `BoltonCup:Authentication:AdminAccountId` (so the key can reach the
   upload-credentials endpoint, which is gated on a completed account).
3. `cp .env.example .env` and fill it in. `BC_API_KEY` must match the API's
   `AdminApiKey`, and `BC_API_BASE` must be the API's **Tailscale** address — the
   admin key is rejected from outside the tailnet (see
   `AdminApiKeyAllowedNetworks` on the API).
4. Build: `docker compose build`

## Run

```bash
# See what would be processed without downloading anything:
docker compose run --rm music-fetcher --dry-run

# Process the missing list once:
docker compose run --rm music-fetcher
```

## Schedule (host crontab)

Run every 30 minutes:

```cron
*/30 * * * * cd /path/to/tools/music-fetcher && docker compose run --rm music-fetcher >> /var/log/music-fetcher.log 2>&1
```

A run only touches the handful of requests added since the last run, since the
list self-drains.

## Configuration

See `.env.example`. Required: `BC_API_BASE`, `BC_API_KEY`, `BC_TOURNAMENT_ID`,
`SPOTIFY_CLIENT_ID`, `SPOTIFY_CLIENT_SECRET`. Optional: `BC_TRACK_DELAY` (seconds
between tracks, default 2).
