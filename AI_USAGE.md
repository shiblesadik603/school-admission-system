# AI Usage

## Tool

Claude (Anthropic), used as a coding assistant in Claude Code.

## Where I used it

- Reading the brief and planning the structure (backend / frontend / Docker, step by step commits).
- Writing first versions of the backend code (EF Core model, seat hold service, controllers),
  the tests, the React pages, Dockerfiles and docker-compose.
- Drafting README and ASSUMPTIONS.

I reviewed the code, decided the rules in ASSUMPTIONS.md, and checked everything by running it:
backend tests, frontend tests, curl requests against the running API, and clicking through the UI.

## Where the AI was wrong and how I caught it

1. **Seat count query crashed.** `GET /classes` returned 500. EF Core could not translate an
   `OrderBy` placed after projecting into a C# record. I found it with a manual curl test and the
   server log. Fix: query plain values first, then build the response objects in memory.
2. **Over-engineered first version.** The first draft had generic result wrappers and long XML
   comments, which made it hard to read. I asked for a simpler, conventional layout
   (Controllers / Services / Models / Dtos) and plain exceptions + one error middleware.
3. **Package version conflict.** It added `Microsoft.EntityFrameworkCore.Design` 10.0.14 while
   the Npgsql provider uses EF Core 10.0.4, which gave build warnings. Aligned the versions.
4. **Unverified instructions.** The README draft had a command for running the tests inside an
   SDK container that was never actually tried. I removed it and kept only commands I ran.
5. **Test setup stuck.** The first test run seemed to hang while Docker downloaded the
   Testcontainers images. I added the `TEST_DB_CONNECTION` option to run tests against an existing
   database, and later confirmed the Testcontainers path also passes.

## How the important parts were checked

- Concurrency: automated test with 60 parallel requests for 10 seats (exactly 10 succeed), the same
  with two app instances on one database, and a manual burst of 41 requests for 5 seats against the
  Docker stack (5 × 201, 36 × 409).
- Expiry: test with a 2 second hold, and in the browser the status changed to EXPIRED and the pay
  button was disabled without reloading.
- Persistence: created data, ran `docker compose down` and `up`, data was still there.
