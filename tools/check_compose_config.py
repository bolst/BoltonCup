#!/usr/bin/env python3
"""Fails a deploy whose resolved compose config would expose a port, break the trusted proxy, or miss Postgres.

Usage: check_compose_config.py WEBAPI_CONFIG_JSON POSTGRES_CONFIG_JSON
Each argument is the output of `docker compose ... config --format json`; tools/prepare_host.sh passes them.
"""
import ipaddress
import json
import re
import sys

# Inside boltoncup-db, Postgres listens on its container port whatever port the host publishes.
POSTGRES_HOST = "postgres"
POSTGRES_CONTAINER_PORT = "5432"


def bind_errors(config, service, variable):
    for port in config["services"][service].get("ports", []):
        host_ip = port.get("host_ip", "").strip("[]")
        published = port.get("published")
        if host_ip in ("", "0.0.0.0", "::"):
            yield f"{service} publishes port {published} on every interface; set {variable} to the tailnet IP"
        elif ipaddress.ip_address(host_ip).is_loopback:
            yield (f"{service} publishes port {published} on loopback only, so tailnet tools cannot reach it; "
                   f"set {variable} to the tailnet IP")


def tunnel_errors(webapi):
    if not webapi["services"]["cloudflared"].get("environment", {}).get("TUNNEL_TOKEN"):
        yield "TUNNEL_TOKEN is empty"


def trusted_proxy_errors(webapi):
    proxy_ip = webapi["services"]["cloudflared"]["networks"]["edge"]["ipv4_address"]
    trusted = webapi["services"]["webapi"].get("environment", {}).get("BoltonCup__ForwardedHeaders__KnownNetworks__0", "")
    if trusted != f"{proxy_ip}/32":
        yield (f"trusted proxy {trusted!r} does not match cloudflared ipv4_address {proxy_ip}; "
               "every caller would share one rate limit")


def parse_connection_string(connection_string):
    settings = {}
    for part in connection_string.split(";"):
        key, _, value = part.partition("=")
        settings[re.sub(r"\s+", "", key).lower()] = value.strip()
    return settings


def connection_string_errors(webapi):
    settings = parse_connection_string(webapi["services"]["webapi"].get("environment", {}).get("BoltonCup__ConnectionString", ""))
    host, _, host_port = (settings.get("server") or settings.get("host") or "").partition(":")
    port = settings.get("port") or host_port or POSTGRES_CONTAINER_PORT
    if host.lower() != POSTGRES_HOST or port != POSTGRES_CONTAINER_PORT:
        yield (f"CONNECTION_STRING must use Server={POSTGRES_HOST} on port {POSTGRES_CONTAINER_PORT} "
               "(the API reaches Postgres over boltoncup-db)")


def main(webapi_json, postgres_json):
    webapi = json.loads(webapi_json)
    postgres = json.loads(postgres_json)
    errors = [
        *bind_errors(webapi, "webapi", "WEBAPI_BIND"),
        *bind_errors(postgres, "postgres", "POSTGRES_BIND"),
        *tunnel_errors(webapi),
        *trusted_proxy_errors(webapi),
        *connection_string_errors(webapi),
    ]
    for error in errors:
        print(f"prepare_host: {error}", file=sys.stderr)
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main(*sys.argv[1:3]))