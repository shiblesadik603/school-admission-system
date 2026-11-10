# School Admission System

Online admission for a school where seats are given **first come, first served**.
A parent applies for a class, the seat is **held** for a limited time (10 minutes by default),
and the admission is **confirmed** when the fee is paid within that time. Otherwise the hold
expires and the seat is free again.

| Part     | Tech                                   | Folder      |
|----------|----------------------------------------|-------------|
| Backend  | ASP.NET Core (.NET 10), EF Core, PostgreSQL | `backend/`  |
| Frontend | React 19 + Vite + TypeScript           | `frontend/` |

## Run with Docker

Only Docker is needed.

```bash
docker compose up --build
```

| What     | URL                    |
|----------|------------------------|
| Frontend | http://localhost:3000  |
| Backend  | http://localhost:8080  |

Data is stored in the `pgdata` volume, so `docker compose down` + `docker compose up` keeps it.
To delete all data: `docker compose down -v`.

### Settings (environment variables)

| Variable | Used by | Default | Meaning |
|----------|---------|---------|---------|
| `HOLD_DURATION_SECONDS` | backend | `600` | How long a seat stays on hold |
| `ConnectionStrings__Default` | backend | set in compose | PostgreSQL connection string |
| `API_URL` | frontend | `http://localhost:8080` | Backend URL the browser calls (read when the container starts) |

Example: `HOLD_DURATION_SECONDS=60 docker compose up` for 1 minute holds.

## How to use

1. Open http://localhost:3000.
2. **Create class (admin)** – add a class with its seat count, one per shift
   (e.g. "Class 6 - Morning Shift", "Class 6 - Day Shift").
3. Enter the student's 17 digit birth registration number in the box at the top. This is the student's identity (no login).
4. **Classes** – press **Apply** on a class. You go to the application page with a countdown.
5. Press **Pay admission fee** before the time is over (payment is simulated), or **Cancel**.
6. **My applications** – all applications of the current student.

## API

Base URL `http://localhost:8080`. Student identity comes from the `X-Student-Id` header (17 digits).

| Method | Path | |
|--------|------|-|
| POST   | `/classes` | create class |
| GET    | `/classes` | all classes with seat counts |
| GET    | `/classes/{classId}` | one class with seat counts |
| POST   | `/classes/{classId}/holds` | apply (hold a seat) – `409` if no seat |
| GET    | `/holds/{holdId}` | application status |
| POST   | `/holds/{holdId}/confirm` | pay fee / confirm |
| DELETE | `/holds/{holdId}` | cancel – `204` |
| GET    | `/me/holds` | all applications of the student |

Errors always look like `{ "error": "NO_SEATS_AVAILABLE", "message": "..." }`.

## Running the tests

### Backend

Needs the .NET 10 SDK and Docker running (tests start a PostgreSQL container with Testcontainers):

```bash
cd backend
dotnet test
```

To use an existing PostgreSQL instead of a container:

```bash
TEST_DB_CONNECTION="Host=localhost;Port=5432;Database=admission_test;Username=postgres;Password=postgres" dotnet test
```

Tests include concurrency tests: 60 students apply at the same time for 10 seats and exactly
10 get a seat, and the same with two backend instances sharing one database.

### Frontend

```bash
cd frontend
npm install
npm test
```

## Running without Docker (development)

```bash
# backend (needs a PostgreSQL; change the connection string as needed)
cd backend/Admission.Api
ConnectionStrings__Default="Host=localhost;Port=5432;Database=admission;Username=admission;Password=admission" dotnet run

# frontend
cd frontend
npm install
VITE_API_URL=http://localhost:8080 npm run dev
```

## Project structure

```
backend/
  Admission.Api/
    Controllers/   HTTP endpoints
    Services/      business logic (ClassService, HoldService, expiry cleanup job)
    Models/        database entities
    Dtos/          request / response shapes
    Data/          DbContext and migrations
    Middleware/    error handling
    Helpers/       X-Student-Id reading, id generation
  Admission.Tests/ API + concurrency tests
frontend/
  src/pages/       Classes, Create class, Application, My applications
  src/components/  small shared components
  src/api.ts       calls to the backend
```

See [ASSUMPTIONS.md](ASSUMPTIONS.md) for decisions that were not specified in the brief.
