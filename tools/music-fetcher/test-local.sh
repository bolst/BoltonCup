#!/usr/bin/env bash
# Run the music-fetcher locally against a `dotnet run` API (dev-stack HTTPS, self-signed cert).
# Usage:  ./test-local.sh            # dry-run
#         ./test-local.sh --live     # actually download/upload/register (needs ffmpeg on PATH)
set -euo pipefail
cd "$(dirname "$0")"

CA_DIR="$HOME/.cache/mf-ca"
CA_BUNDLE="$CA_DIR/combined.pem"
PFX="$HOME/.aspnet/https/boltoncup.pfx"

# 1. venv with deps (install every run so requirements.txt changes are picked up)
[ -d .venv ] || { echo ">> creating venv"; python3 -m venv .venv; }
echo ">> ensuring deps"
./.venv/bin/pip install -q -r requirements.txt
# Put the venv bin on PATH so fetch_music.py's bare `spotdl` subprocess resolves.
export PATH="$(pwd)/.venv/bin:$PATH"

# 2. CA bundle (system roots + the self-signed dev cert) so Python's requests trusts localhost
if [ ! -f "$CA_BUNDLE" ]; then
  echo ">> building CA bundle from $PFX"
  mkdir -p "$CA_DIR"
  openssl pkcs12 -in "$PFX" -clcerts -nokeys -passin pass:devpassword -out "$CA_DIR/devcert.pem"
  ./.venv/bin/python -c "import certifi,sys;sys.stdout.write(open(certifi.where()).read())" > "$CA_BUNDLE"
  cat "$CA_DIR/devcert.pem" >> "$CA_BUNDLE"
fi

# 3. load config from .env
set -a; source .env; set +a

# 4. run (default dry-run; pass --live to do a real fetch)
ARGS="--dry-run"
if [ "${1:-}" = "--live" ]; then
  ARGS=""
  command -v ffmpeg >/dev/null || { echo "ERROR: ffmpeg not found (spotdl needs it). Run: brew install ffmpeg"; exit 1; }
fi

echo ">> hitting $BC_API_BASE (tournament $BC_TOURNAMENT_ID)"
REQUESTS_CA_BUNDLE="$CA_BUNDLE" ./.venv/bin/python fetch_music.py $ARGS
