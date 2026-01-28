#!/usr/bin/env bash
set -euo pipefail

# Load DP_CERT_PASSWORD from Docker secret if available
if [ -f "/run/secrets/dp_cert_password" ]; then
  export DP_CERT_PASSWORD="$(cat /run/secrets/dp_cert_password)"
fi

# If ASPNETCORE_URLS is set, unset HTTP_PORTS/HTTPS_PORTS to avoid Kestrel override warnings
if [ -n "${ASPNETCORE_URLS:-}" ]; then
  prev_http="${HTTP_PORTS:-}"
  prev_https="${HTTPS_PORTS:-}"
  if [ -n "$prev_http" ] || [ -n "$prev_https" ]; then
    echo "ℹ️ Clearing HTTP_PORTS/HTTPS_PORTS (was: HTTP_PORTS='$prev_http', HTTPS_PORTS='$prev_https') because ASPNETCORE_URLS is set to '${ASPNETCORE_URLS}'."
    unset HTTP_PORTS
    unset HTTPS_PORTS
  fi
fi

# Start the application
exec dotnet NextShopV2.Api.dll
