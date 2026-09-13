#!/bin/sh
set -e

# Extract cert and key from pfx if they don't exist
if [ ! -f /etc/nginx/certs/cert.pem ] || [ ! -f /etc/nginx/certs/key.pem ]; then
    mkdir -p /etc/nginx/certs
    openssl pkcs12 -in /https/boltoncup.pfx -clcerts -nokeys -out /etc/nginx/certs/cert.pem -passin pass:${CERT_PASSWORD:-devpassword}
    openssl pkcs12 -in /https/boltoncup.pfx -nocerts -nodes -out /etc/nginx/certs/key.pem -passin pass:${CERT_PASSWORD:-devpassword}
fi

# Generate appsettings.json from environment variables
cat > /usr/share/nginx/html/appsettings.json << EOF
{
  "BoltonCup": {
    "ApiBaseUrl": "${BOLTONCUP_API_BASE_URL:-https://api.boltoncup.ca/}",
    "AuthBaseUrl": "${BOLTONCUP_AUTH_BASE_URL:-https://auth.boltoncup.ca/}",
    "WebBaseUrl": "${BOLTONCUP_WEB_BASE_URL:-https://boltoncup.ca/}"
  },
  "Sentry": {
    "Dsn": "${SENTRY_DSN:-}"
  }
}
EOF

exec nginx -g "daemon off;"
