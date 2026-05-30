# NutriVision

Student implementation repository for the Food and Drink coursework.

## Current Status

- Starter/demo code removed from branch `lz-21906394`.
- Architecture baseline created.
- Core MAUI solution and service layer implemented.
- Voice input (microphone) flow implemented and validated.
- Food recognition supports cloud-first + local fallback.

## Branch Strategy

- Long-lived branch: `lz-21906394`
- Feature branches: `feature/<small-scope>`
- One small feature per PR for better readability and marking evidence.

## Cloud API Setup (Optional but recommended)

No extra backend project download is required.

Set environment variables before running the app:

- `NUTRIVISION_FOOD_API_URL`:
  - default: `https://api-inference.huggingface.co/models/nateraw/food`
- `NUTRIVISION_FOOD_API_TOKEN`:
  - your Hugging Face access token (optional)

Behavior:

- If token is configured, app calls cloud API first.
- If cloud call fails or returns unsupported label, app automatically falls back to local recognition.
