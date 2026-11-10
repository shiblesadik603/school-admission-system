# Assumptions

Things the brief did not say exactly, and what I decided.

## Seats and concurrency

- **How overselling is prevented:** applying for a seat runs in a transaction that first locks the
  class row (`SELECT ... FOR UPDATE`). Requests for the same class wait for each other, count the used
  seats and only then insert the hold. The lock lives in PostgreSQL, so it also works when two or more
  backend processes on different ports use the same database (bonus requirement, covered by a test).
  Requests for different classes don't block each other.
- **Expiry without an API call:** a `HELD` hold counts as used only while `expires_at > now`. So the
  seat is free the moment the time is up. A background job also changes old rows to `EXPIRED` every
  5 seconds, but that is just cleanup; correctness doesn't depend on it.
- `GET /holds/{id}` and `GET /me/holds` show `EXPIRED` as soon as the time is over, even if the
  cleanup job has not updated the row yet.
- A partial unique index (`class_id, student_id` where status is `HELD` or `CONFIRMED`) is an extra
  safety net for "one active application per student per class".

## Rules

- "One active application" is **per class** (a student may apply to Class 6 Morning and Class 7 at the
  same time, but not twice to Class 6 Morning). A student who already has a confirmed admission in a
  class can't apply to it again. After an expired or cancelled application the student can apply again.
- **Confirm twice** returns `200` with `CONFIRMED` again and does not count the seat twice.
- **Confirm an expired hold** → `409 HOLD_EXPIRED`. Confirm a cancelled hold → `409 HOLD_CANCELLED`.
- **Cancel** works only for a `HELD` hold. Cancelling an already cancelled hold returns `204` again.
  A confirmed (paid) admission can't be cancelled here (`409`), since refunds are out of scope.
  Cancelling an expired hold → `409`.
- Confirming or cancelling someone else's hold → `403`. `GET /holds/{id}` does not need the header
  (the brief doesn't list it there).

## Input

- `X-Student-Id` must be exactly 17 digits. Missing → `400 MISSING_STUDENT_ID`, wrong format →
  `400 INVALID_STUDENT_ID`.
- Class name: required, max 100 characters, unique (case-insensitive) → `409 CLASS_NAME_EXISTS`.
  Total seats: whole number from 1 to 100000.
- Seat count of a class can't be changed after creation (no endpoint for it in the brief).
- IDs look like `c_3f9a1b2c4d5e6f70` and `h_0a1b2c3d4e5f6a7b` (random, so they can't be guessed one by one).

## Responses

- All errors use `{ "error": "<CODE>", "message": "<text for the user>" }`. The UI shows `message`.
- Hold responses have a few extra fields (`className`, `studentId`, `createdAt`, `confirmedAt`,
  `serverTime`). `serverTime` lets the UI correct the countdown if the user's clock is wrong.
- `GET /classes` is sorted by class name.

## UI

- The student ID is stored in `localStorage`, so after refresh or reopening the browser the student
  sees their applications again.
- Seat numbers refresh every 3 seconds (polling; simple and enough here). The application page
  refreshes every 5 seconds and the countdown ticks every second.
- The class creation screen is open to everyone because the brief says there is no login.
- UI text is in English.
- The backend URL for the frontend is set with the `API_URL` env variable at container start
  (written into `config.js`), so the image doesn't need to be rebuilt to change it.
  In development `VITE_API_URL` is used.

## Other

- Database migrations run automatically when the backend starts (with retries while the DB starts up).
- CORS allows any origin, because there is no login or cookie.
