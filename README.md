# OrderFlow

OrderFlow is a production-oriented .NET 8 backend service for reliable order processing under concurrent load.

## Assessment Goal
Design and implement a backend that:
- places orders with multiple products and quantities,
- prevents overselling under concurrency,
- triggers event-driven downstream processing,
- remains maintainable and testable with clean architecture boundaries.

## Technology Stack
- .NET 8 / ASP.NET Core Web API
- EF Core 8 + PostgreSQL
- MediatR for command/query + notification flow
- FluentValidation for request and command validation
- Serilog for structured logs
- Swagger/OpenAPI for interactive API documentation
- xUnit + Moq + FluentAssertions for automated tests
- Docker + Docker Compose for portable runtime

## Architecture
The solution is organized into four layers:
- API layer (src/OrderFlow.Api): transport, controllers, middleware, startup pipeline
- Application layer (src/OrderFlow.Application): use-case orchestration, validators, handlers, behaviors
- Domain layer (src/OrderFlow.Domain): entities, invariants, exceptions, contracts
- Infrastructure layer (src/OrderFlow.Infrastructure): EF Core persistence, repositories, migrations, background services

## Core Reliability Design

### Concurrency and Oversell Prevention
- Product stock updates use optimistic concurrency with PostgreSQL xmin token.
- Order placement retries on DbUpdateConcurrencyException up to 3 attempts.
- Retry path clears stale tracked entities between attempts to guarantee fresh state reads.

### Idempotency
- Orders use a unique IdempotencyKey index.
- First successful request with a key returns 201 Created.
- Replayed request with same key returns 200 OK with the original order response.

### Event-Driven Processing
- Successful order writes an outbox message in the same transaction.
- Outbox background service dispatches pending events.
- Notification handlers simulate payment, inventory confirmation, and customer notification.

## API Endpoints
- POST /api/orders
- GET /api/orders/{id}
- GET /api/orders
- GET /api/products
- GET /api/products/{id}
- POST /api/products

## Local Setup

### Prerequisites
- .NET 8 SDK
- Docker and Docker Compose

### Run with Local API + Docker PostgreSQL
1. Start PostgreSQL:
```bash
docker compose up -d postgres
```

2. Restore tools and packages:
```bash
dotnet tool restore
dotnet restore OrderFlow.sln
```

3. Apply migrations:
```bash
dotnet tool run dotnet-ef database update -p src/OrderFlow.Infrastructure -s src/OrderFlow.Api
```

4. Run API:
```bash
dotnet run --project src/OrderFlow.Api
```

5. Open Swagger:
- https://localhost:5001/swagger

### Run Full Stack in Docker
```bash
docker compose up --build
```

Swagger in container mode:
- http://localhost:8080/swagger

## Testing
Run all tests:
```bash
dotnet test OrderFlow.sln
```

## CI Quality Gates
CI workflow is defined at:
- .github/workflows/ci.yml

It enforces:
- restore
- build
- test
- migration/model consistency check

## Trade-offs and Decisions
- PostgreSQL chosen over SQLite for production realism and stronger relational behavior.
- Outbox-based asynchronous dispatch chosen over purely in-request notifications for resilience and lower request coupling.
- Authentication/authorization intentionally excluded to match assessment scope, while keeping middleware architecture ready for insertion.

## Assumptions
- Open API access is acceptable for this assessment.
- Event dispatch is eventually consistent after order creation.
- Single-service deployment boundary is sufficient for assessment scope.