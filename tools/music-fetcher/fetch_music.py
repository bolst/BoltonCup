#!/usr/bin/env python3
"""Drain a tournament's "requested but missing" music list.

For each player song request that has a Spotify track id but no uploaded file, this
downloads the track with spotDL, uploads it to R2 via the API's presigned-upload
flow, and registers it tagged with the Spotify track id so the playlist composer
matches it. Requests with no track id are skipped and reported (the composer can
only match on a track id).

Re-runs are safe: a registered track stops appearing in the missing list.
"""

import argparse
import os
import subprocess
import sys
import tempfile
import time

import requests
import spotipy
from spotipy.oauth2 import SpotifyClientCredentials


# Must match ApiKeyConstants.Header on the API.
API_KEY_HEADER = "BoltonCup-Api-Key"


class Config:
    def __init__(self) -> None:
        self.api_base = _require("BC_API_BASE").rstrip("/")
        self.api_key = _require("BC_API_KEY")
        self.tournament_id = int(_require("BC_TOURNAMENT_ID"))
        self.spotify_client_id = _require("SPOTIFY_CLIENT_ID")
        self.spotify_client_secret = _require("SPOTIFY_CLIENT_SECRET")
        # Seconds to pause between tracks so we are gentle on YouTube / the API.
        self.track_delay = float(os.environ.get("BC_TRACK_DELAY", "2"))


def _require(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        sys.exit(f"Missing required environment variable: {name}")
    return value


def log(message: str) -> None:
    print(message, flush=True)


def make_session(cfg: Config) -> requests.Session:
    """A session that sends the admin API key on every request (no login round-trip)."""
    session = requests.Session()
    session.headers[API_KEY_HEADER] = cfg.api_key
    return session


def fetch_missing(session: requests.Session, cfg: Config) -> list[dict]:
    resp = session.get(
        f"{cfg.api_base}/api/tournaments/{cfg.tournament_id}/music/missing",
        timeout=30,
    )
    resp.raise_for_status()
    return resp.json()


def spotify_metadata(spotify: spotipy.Spotify, track_id: str) -> dict:
    track = spotify.track(track_id)
    images = track.get("album", {}).get("images") or []
    return {
        "title": track["name"],
        "artist": ", ".join(a["name"] for a in track.get("artists", [])) or None,
        "albumArtUrl": images[0]["url"] if images else None,
        "durationMs": track.get("duration_ms"),
    }


def download_track(track_id: str, dest_dir: str) -> str | None:
    """Download one Spotify track into dest_dir via spotDL. Returns the mp3 path or None."""
    url = f"https://open.spotify.com/track/{track_id}"
    # --output is a name *template*, not a literal path: a literal "<id>.mp3" makes spotDL create a
    # directory of that name. Use placeholders so it writes <dest_dir>/<track_id>.mp3.
    output_template = os.path.join(dest_dir, "{track-id}.{output-ext}")
    try:
        subprocess.run(
            ["spotdl", "download", url, "--format", "mp3", "--output", output_template],
            check=True,
            capture_output=True,
            text=True,
            timeout=600,
        )
    except (subprocess.CalledProcessError, subprocess.TimeoutExpired, FileNotFoundError) as exc:
        detail = getattr(exc, "stderr", "") or str(exc)
        log(f"    spotdl failed: {detail.strip().splitlines()[-1] if detail.strip() else exc}")
        return None
    # Return the largest .mp3 file spotDL actually wrote (recursive, so it survives layout quirks).
    mp3s = [
        os.path.join(root, name)
        for root, _dirs, files in os.walk(dest_dir)
        for name in files
        if name.lower().endswith(".mp3") and os.path.getsize(os.path.join(root, name)) > 0
    ]
    if not mp3s:
        log("    spotdl produced no mp3 file")
        return None
    return max(mp3s, key=os.path.getsize)


def upload_to_r2(session: requests.Session, cfg: Config, mp3_path: str) -> str:
    """Get presigned credentials, PUT the bytes, return the temp key."""
    cred = session.get(
        f"{cfg.api_base}/api/assets",
        params={"fileExtension": ".mp3", "contentType": "audio/mpeg"},
        timeout=30,
    )
    cred.raise_for_status()
    cred = cred.json()

    with open(mp3_path, "rb") as fh:
        # Content-Type must match what the presigned URL was signed with, or R2 rejects it.
        put = requests.put(
            cred["uploadUrl"],
            data=fh,
            headers={"Content-Type": "audio/mpeg"},
            timeout=120,
        )
    put.raise_for_status()
    return cred["tempKey"]


def register_track(
    session: requests.Session, cfg: Config, temp_key: str, track_id: str, meta: dict
) -> None:
    resp = session.post(
        f"{cfg.api_base}/api/tournaments/{cfg.tournament_id}/music",
        json={
            "tempKey": temp_key,
            "title": meta["title"],
            "artist": meta["artist"],
            "trackId": track_id,  # the join key the composer matches on
            "providerType": "Spotify",
            "albumArtUrl": meta["albumArtUrl"],
            "durationMs": meta["durationMs"],
            "isInBasePool": False,  # a player request, not an every-game track
        },
        timeout=30,
    )
    resp.raise_for_status()


def dedupe_by_track_id(missing: list[dict]) -> list[dict]:
    # Same song can be requested by several players; we only need to fetch it once.
    seen: set[str] = set()
    todo: list[dict] = []
    for item in missing:
        track_id = item.get("songTrackId")
        if track_id and track_id not in seen:
            seen.add(track_id)
            todo.append(item)
    return todo


def main() -> int:
    parser = argparse.ArgumentParser(description="Auto-fetch missing tournament music.")
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="List the missing tracks and exit; download/upload nothing.",
    )
    args = parser.parse_args()

    cfg = Config()
    session = make_session(cfg)
    log(f"Tournament {cfg.tournament_id}; API {cfg.api_base}.")

    missing = fetch_missing(session, cfg)
    todo = dedupe_by_track_id(missing)
    name_only = [m for m in missing if not m.get("songTrackId")]

    log(f"Missing requests: {len(missing)} ({len(todo)} resolvable, {len(name_only)} name-only).")

    if args.dry_run:
        for item in todo:
            log(f"  [resolvable] {item.get('songName') or item['songTrackId']} ({item['songTrackId']})")
        _report_name_only(name_only)
        return 0

    spotify = spotipy.Spotify(
        auth_manager=SpotifyClientCredentials(
            client_id=cfg.spotify_client_id,
            client_secret=cfg.spotify_client_secret,
        )
    )

    succeeded = 0
    failed: list[dict] = []

    for index, item in enumerate(todo, start=1):
        track_id = item["songTrackId"]
        label = item.get("songName") or track_id
        log(f"[{index}/{len(todo)}] {label} ({track_id})")

        try:
            meta = spotify_metadata(spotify, track_id)
        except Exception as exc:  # noqa: BLE001 - report and continue
            log(f"    metadata lookup failed: {exc}")
            failed.append(item)
            continue

        # Download then upload per-track: the presigned temp key expires in 15 min.
        with tempfile.TemporaryDirectory() as tmp:
            mp3_path = download_track(track_id, tmp)
            if mp3_path is None:
                failed.append(item)
                continue
            try:
                temp_key = upload_to_r2(session, cfg, mp3_path)
                register_track(session, cfg, temp_key, track_id, meta)
            except requests.HTTPError as exc:
                log(f"    upload/register failed: {exc}")
                failed.append(item)
                continue

        succeeded += 1
        log(f"    registered: {meta['title']} - {meta['artist']}")
        if cfg.track_delay and index < len(todo):
            time.sleep(cfg.track_delay)

    log("")
    log(f"Done. Registered {succeeded}, failed {len(failed)}.")
    if failed:
        log("Failed downloads (will retry next run):")
        for item in failed:
            log(f"  - {item.get('songName') or item['songTrackId']} ({item['songTrackId']}) [{item.get('playerName')}]")
    _report_name_only(name_only)

    return 0


def _report_name_only(name_only: list[dict]) -> None:
    if not name_only:
        return
    log("Name-only requests (no Spotify id, need manual handling):")
    for item in name_only:
        log(f"  - {item.get('songName') or '(no name)'} [{item.get('playerName')}]")


if __name__ == "__main__":
    sys.exit(main())
