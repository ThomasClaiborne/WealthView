# WealthView — Server

ASP.NET Core 9 REST API for the WealthView portfolio management application. Handles authentication, trade execution, holdings tracking, fund transfers, and live market data.

---

## Tech Stack

- **Framework:** ASP.NET Core 9
- **Language:** C# with nullable reference types
- **ORM:** Entity Framework Core 9
- **Database:** MySQL (via Pomelo connector)
- **Auth:** JWT Bearer tokens + BCrypt password hashing
- **Market Data:** Alpha Vantage API (daily-cached prices)
- **API Docs:** OpenAPI / Swagger (`/swagger` in development)
- **Testing:** xUnit (`Server.Tests/`)

---

## Architecture

The server follows a layered architecture with clear separation between request handling, business logic, and data access.

```
Controllers  →  Services  →  Repositories  →  EF Core  →  MySQL
     ↑               ↑
  DTOs (in)      Domain Models
  DTOs (out)
```

**Controllers** handle routing and HTTP concerns only — they validate input, call a service, and return a response. No business logic lives here.

**Services** contain all business logic: cash validation, weighted average cost calculations, transfer state machines, price fetching, snapshot creation.

**Repositories** are EF Core wrappers that provide typed data access. Each entity has its own repository interface and `Ef*` implementation.

**JWT identity:** All protected endpoints extract the user ID from the JWT claims. It is never accepted from the request body.

---

## Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MySQL 8+
- An [Alpha Vantage API key](https://www.alphavantage.co/support/#api-key) (free tier works)

### Configuration

```bash
cp appsettings.example.json appsettings.json
```

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=wealthview;user=YOUR_USER;password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "Issuer": "WealthView",
    "Audience": "WealthViewUsers",
    "ExpiryMinutes": 60
  },
  "AlphaVantage": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

### Database

```bash
dotnet ef database update
```

This runs all migrations and creates the schema. To seed initial securities data, see the migration files.

### Run

```bash
dotnet run
```

The API is available at `https://localhost:5001`. Swagger UI is at `https://localhost:5001/swagger`.

---

## API Endpoints

Full request/response documentation is in [`/docs/controller-endpoints.md`](../../docs/controller-endpoints.md).

| Controller | Endpoints | Auth |
|-----------|-----------|------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` | Public |
| Portfolio | `GET /api/portfolio`, `GET /api/portfolio/snapshots` | Required |
| Holdings | `GET /api/holdings` | Required |
| Trades | `GET /api/trades`, `POST /api/trades/buy`, `POST /api/trades/sell` | Required |
| Securities | `GET /api/securities` | Required |
| Bank Accounts | `GET`, `POST`, `PATCH`, `DELETE /api/bank-accounts` | Required |
| Fund Transfers | `GET`, `POST`, `PATCH /api/fund-transfers` | Required |

---

## Key Design Decisions

**Computed metrics, not stored** — Portfolio value, unrealized gain/loss, and portfolio weight are calculated on each request using live prices. This ensures accuracy without background sync jobs.

**Append-only trade log** — Trades are never updated or deleted. Every buy and sell is a permanent record. Holdings are derived from this log at query time.

**Weighted average cost on buys** — When buying additional shares of an existing holding, the average cost is recalculated: `(existing_value + new_value) / total_quantity`. Sells don't change the cost basis.

**Price caching** — Alpha Vantage prices are cached per ticker per day. Repeated requests within the same day don't hit the external API.

**Fund transfer safety** — Transfers execute in a two-step flow: create (Pending) → approve. At approval time, balances are re-validated inside a transaction, so the operation is safe even if the user's balance changed between creation and approval.

**Transactions everywhere that matters** — Buy, sell, and transfer approval all use `BeginTransactionAsync` to ensure either all side effects commit or none do.

---

## Running Tests

```bash
cd ../Server.Tests
dotnet test
```
