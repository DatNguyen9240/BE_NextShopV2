#!/usr/bin/env bash
set -euo pipefail

# Load DP_CERT_PASSWORD from Docker secret if available
if [ -f "/run/secrets/dp_cert_password" ]; then
  export DP_CERT_PASSWORD="$(cat /run/secrets/dp_cert_password)"
fi

# Optionally load DP_CERT_PATH and DP_KEYS_PATH if provided via env or mounts (no-op here)
# Start the application
exec dotnet NextShopV2.Api.dll
