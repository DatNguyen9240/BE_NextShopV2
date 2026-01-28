#!/usr/bin/env bash
set -euo pipefail

CERT_SUBJ="/CN=nextshop.local"
DAYS=${1:-3650}
PFX_PASS=${2:-ChangeMePlease}

echo "Generating self-signed certificate for DataProtection (will create dp_key.pfx)..."
openssl req -x509 -nodes -days "$DAYS" -newkey rsa:2048 -keyout key.pem -out cert.pem -subj "$CERT_SUBJ"
openssl pkcs12 -export -out dp_key.pfx -inkey key.pem -in cert.pem -passout pass:"$PFX_PASS"

echo "Created dp_key.pfx (password: $PFX_PASS). Move dp_key.pfx to ./secrets and add to docker-compose as a bind mount or use Docker secrets."