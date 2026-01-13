# Passkeys (WebAuthn) Setup & Manual Test

## 1. Install dependencies
Run in backend project:

```bash
dotnet add package Fido2
```

## 2. Database
Add migration and update database to include Passkeys table:

```bash
dotnet ef migrations add AddPasskeys
dotnet ef database update
```

The `Passkeys` table should have columns matching `NextShopV2.Domain.Entities.Security.Passkey`.

## 3. Configuration
In `appsettings.Development.json` add:

```json
"Fido2": {
  "Origin": "https://localhost:3000",
  "RPID": "localhost",
  "ServerName": "NextShop"
}
```

Ensure `Jwt:Key` exists for token generation.

## 4. Run & Test (local)
- Start backend (`dotnet run`) and frontend (Next.js).
- Use ngrok for HTTPS if needed: `ngrok http 3000` and update `Fido2:Origin`/`RPID` accordingly.

### Manual flow
1. Go to `/auth/passkey-register` (ensure `window.CURRENT_USER_ID` is set to your user GUID for manual testing).
2. Click **Register** and follow browser prompts.
3. Go to `/auth/passkey-login` and click **Login**.
4. After successful login, a JWT token will be returned.

## 5. Notes
- HTTPS is required (ngrok ok for local testing).
- Allow multiple passkeys per user for device backup.
- Keep fallback auth methods.
