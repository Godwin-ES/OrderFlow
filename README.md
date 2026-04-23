# OrderFlow

OrderFlow is a production-oriented .NET 8 backend for resilient order processing under concurrent load.

## Highlights
- Clean Architecture with API, Application, Domain, and Infrastructure layers
- EF Core 8 + PostgreSQL with optimistic concurrency to prevent overselling
- Idempotent order placement via unique idempotency key
- Transactional outbox with hosted background dispatcher
- MediatR-based command/query/event handling
- FluentValidation pipeline behavior
- Structured logging with Serilog and RFC 7807-style error responses
- Unit and integration tests including concurrency behavior

## Architecture
- API: Controllers, middleware, startup wiring
- Application: Commands, queries, handlers, validators, pipeline behaviors
- Domain: Entities, domain rules, events, exceptions, interfaces
- Infrastructure: DbContext, repositories, background services, DI

## Concurrency Strategy
Product stock uses an EF Core concurrency token (`Version`), and order placement retries on `DbUpdateConcurrencyException` up to 3 times. This prevents overselling while preserving throughput.

## Run Locally
1. Start PostgreSQL (or use Docker compose):
   ```bash
   docker compose up -d postgres
   ```
2. Apply migrations:
   ```bash
   dotnet tool restore
   dotnet tool run dotnet-ef database update -p src/OrderFlow.Infrastructure -s src/OrderFlow.Api
   ```
3. Run the API:
   ```bash
   dotnet run --project src/OrderFlow.Api
   ```
4. Open Swagger:
   - http://localhost:8080/swagger (container)
   - https://localhost:5001/swagger (local default profile)

## Run Full Stack With Docker
```bash
docker compose up --build
```

## Test
```bash
dotnet test
```

## Key Trade-Offs
- PostgreSQL is chosen for production realism (vs. SQLite portability).
- Event dispatch uses transactional outbox + background service to favor reliability and decouple request latency.
- Authentication is intentionally omitted for assessment scope, with middleware-oriented design so auth can be added without touching business handlers.

## Idempotency Semantics
- First successful `POST /api/orders` with a new idempotency key returns `201 Created`.
- Replayed `POST /api/orders` with the same idempotency key returns `200 OK` and the original order payload.

## Assumptions
- Open API access for assessment (no auth)
- Event-driven operations are eventually consistent after order placement
- Single service boundary (in-process handlers) with outbox durability
