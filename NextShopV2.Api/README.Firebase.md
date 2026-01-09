Firebase / Push Notifications setup

Quick steps to enable push notifications (Firebase Cloud Messaging):

1) Backend (NextShopV2.Api)
- Provide a Firebase Service Account to the API. You can either:
  - Set the JSON content in configuration (not recommended for production):
    - Use `dotnet user-secrets set "Firebase:ServiceAccountJson" "<JSON_CONTENT>"`
    - Or set environment variable: `setx Firebase__ServiceAccountJson "<JSON_CONTENT>"`
  - Or point to a file path containing the service account JSON:
    - Set config value `Firebase:ServiceAccountFile` or env var `FIREBASE_SERVICE_ACCOUNT_FILE` to the JSON file path.
- The API reads `Firebase:ServiceAccountJson` or `Firebase:ServiceAccountFile` and initializes FirebaseAdmin.
- Ensure `Firebase:WebApiKey`, `Firebase:ProjectId`, and `Firebase:VapidKey` are present in `appsettings.json` or envs for reference.

2) Frontend (NextShopV2)
- Add a `.env.local` with the Firebase web config (example in `.env.local.example`).
- Add `NEXT_PUBLIC_FIREBASE_VAPID_KEY` (generate in Firebase Console → Cloud Messaging → Web Push certificates).
- The client registers service worker `public/firebase-messaging-sw.js` and uses `app/lib/firebaseClient.ts` + `app/hooks/usePushNotifications.ts`.
- The client sends the FCM token to backend at `POST /api/push/register-guest` (uses `NEXT_PUBLIC_API_URL` to determine backend base URL).

3) Additional notes
- The ServiceAccount JSON is a secret: do not commit it to source control. Use user-secrets or environment variables or a secret manager.
- For production, consider Key Vault or equivalent.

If you want, I can:
- Add a small admin UI to send test notifications from the API, or
- Add automatic registration on app load (currently we expose a button component `app/components/PushSubscribeButton.tsx`).
