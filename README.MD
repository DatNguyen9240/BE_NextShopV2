src/
├── domain/                 # Lõi nghiệp vụ thuần
│   ├── entities/
│   ├── valueObjects/
│   ├── events/
│   ├── repositories/      # Interfaces/Ports
│   └── services/          # Domain services
│
├── application/           # Use cases + business logic
│   ├── usecases/
│   ├── dtos/
│   ├── interfaces/        # Abstraction để infra implement
│   └── validators/        # Validation logic, nếu cần
│
├── infrastructure/        # Chi tiết implement
│   ├── db/                # DB drivers, ORM, migrations
│   ├── services/          # Client 3rd-party, provider
│   └── repositories/      # Implementation của Interfaces
│
├── shared/                # Code dùng chung (utils/types/constants)
│   ├── constants/
│   ├── errors/
│   ├── utils/
│   └── types/
│
└── tests/                 # Test từng layer riêng như Domain, Application, Infra

---

## DataProtection & Docker setup 🔐

To avoid ephemeral DataProtection keys and secure them in production, you can either:

- Persist keys to filesystem and mount a volume (recommended for local/dev):

```yaml
services:
  web:
    volumes:
      - ./data/dataprotection-keys:/root/.aspnet/DataProtection-Keys
    environment:
      - DP_KEYS_PATH=/root/.aspnet/DataProtection-Keys
```

- Use a certificate (`dp_key.pfx`) to protect keys (recommended for production):

1. Generate a self-signed cert (example scripts provided):
   - Linux/macOS: `./scripts/generate-dp-cert.sh [days] [password]`
   - Windows PowerShell: `./scripts/generate-dp-cert.ps1 -Days 3650 -Password 'YourPassword'`

2. Mount it into container and set env vars (or use Docker secrets):

```yaml
services:
  web:
    volumes:
      - ./secrets/dp_key.pfx:/app/secrets/dp_key.pfx:ro
    environment:
      - DP_CERT_PATH=/app/secrets/dp_key.pfx
      - DP_CERT_PASSWORD=YourPassword
```

> Security tips: do NOT commit `dp_key.pfx` or any secrets into the repo. Use Docker secrets, a secret manager, or CI/CD secrets to inject `DP_CERT_PASSWORD`.

---

If you want, I can also add a Docker Secret example (create a secret file and reference it in `docker-compose.yml`) or add an entrypoint helper script that exports `DP_CERT_PASSWORD` from `/run/secrets/dp_cert_password` into `DP_CERT_PASSWORD` env var for the container.

### Docker Secret example

Instead of committing the password into env files or the repo, create a secret file locally and reference it in `docker-compose.yml`:

```bash
# create secret file (local, not committed)
echo "YourStrongPassword" > ./secrets/dp_cert_password.txt
```

`docker-compose.yml` already contains an example mapping for the secret `dp_cert_password` which will be available in the container at `/run/secrets/dp_cert_password`. The `scripts/entrypoint.sh` will load this into `DP_CERT_PASSWORD` at process start.
