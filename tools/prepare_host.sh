#!/usr/bin/env bash
# Prepares a Docker host for docker/webapi.yml. Idempotent: CI runs it before every deploy, and a fresh
# host at any provider needs only Docker, Tailscale and the .env file.
# It checks the values compose will actually use (after .env, quoting and shell overrides), so a deploy
# stops before `compose up` instead of publishing a port on every interface.
# Usage: tools/prepare_host.sh [path/to/.env]
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
env_file="${1:-.env}"
db_network=boltoncup-db
postgres_container=boltoncup-postgres-1
sysctl_file=/etc/sysctl.d/99-boltoncup.conf
sysctl_setting="net.ipv4.ip_nonlocal_bind = 1"

if [ ! -f "$env_file" ]; then
  echo "prepare_host: $env_file not found" >&2
  exit 1
fi

resolved_config() {
  docker compose -f "$repo_root/docker/$1" --env-file "$env_file" "${@:2}" config --format json
}

python3 "$repo_root/tools/check_compose_config.py" \
  "$(resolved_config webapi.yml --profile edge)" \
  "$(resolved_config postgres.yml)"

# Lets Docker bind the tailnet IP even when it starts before tailscaled has brought the address up.
if [ "$(cat "$sysctl_file" 2>/dev/null)" != "$sysctl_setting" ]; then
  echo "$sysctl_setting" > "$sysctl_file"
  sysctl -q -p "$sysctl_file"
fi

# Starts Postgres (creating boltoncup-db) if it isn't running. --no-recreate means a deploy never restarts a
# running database just because postgres.yml changed; apply such changes by hand with `up -d`.
docker compose -f "$repo_root/docker/postgres.yml" --env-file "$env_file" up -d --no-recreate --wait

postgres_state="$(docker inspect -f '{{.State.Running}} {{json .NetworkSettings.Networks}}' "$postgres_container" 2>/dev/null || true)"
if [[ "$postgres_state" != true* || "$postgres_state" != *"\"$db_network\""* ]]; then
  echo "prepare_host: $postgres_container is running but not on $db_network (created before this layout), so the API could not reach it." >&2
  echo "prepare_host: recreate it once: docker compose -f docker/postgres.yml --env-file $env_file up -d" >&2
  exit 1
fi

echo "prepare_host: ok"