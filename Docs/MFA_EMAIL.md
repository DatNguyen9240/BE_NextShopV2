# Email OTP MFA Implementation

This project now supports Email OTP MFA for login and account enrollment.

## Features
- Start OTP during login: `POST /api/auth/login/start` — if user has Email MFA enabled, a requestId is returned and an OTP is sent to the user's email.
- Verify OTP during login: `POST /api/auth/login/verify` — supply `{ requestId, code }` to complete login and receive tokens.
- Enable Email MFA (authenticated): `POST /api/auth/mfa/enable/start` and `POST /api/auth/mfa/enable/verify`.
- Disable Email MFA (authenticated): `POST /api/auth/mfa/disable`.

## Configuration
1. SMTP settings in `appsettings.json` (or environment variables):
   - `Smtp:Host`, `Smtp:Port`, `Smtp:Username`, `Smtp:Password`, `Smtp:FromEmail`, `Smtp:FromName`.
2. Optional `Mfa:Key` in config for generating code hashes (defaults to `Jwt:Key`).

## Database
Add `MfaEnabled` and `MfaType` columns to `Users` table. You can either run EF Core migration or apply the SQL script `DB_MIGRATIONS/20260113_add_mfa_columns.sql`.

## Frontend
- Login flow now supports MFA. If the server responds with `mfaRequired`, the login UI will show an OTP input and call `/api/auth/login/verify`.
- Account Settings includes an Email MFA section to enable/disable the feature.

## Notes & Security
- OTPs are 6 digits, stored as SHA256(code + userId + serverKey) in Redis with a short TTL (5 minutes). Attempts limited to 5.
- It's recommended to use a production-grade SMTP provider (SendGrid, Mailgun, SES) and configure TLS.
- Consider rate-limiting start endpoints and adding CAPTCHA/rate-limits to prevent abuse.

## Next steps
- Add unit and integration tests for OTP flows
- Add backup codes support
- Add resend and rate-limit UI/UX improvements
- Consider adding WebAuthn/passkeys as a stronger second factor (already supported in the project)
