# WealthView

A full-stack portfolio management application for tracking investment holdings, executing trades, and monitoring portfolio performance over time.

---

## Overview

WealthView lets users manage a simulated investment portfolio end-to-end — from linking bank accounts and depositing funds, to buying and selling securities, to tracking performance with historical snapshots. Live stock prices are pulled from the Alpha Vantage API, and all trade history is preserved as an immutable audit log.

The backend enforces all business logic (cash validation, weighted average cost basis, transfer approval), while the frontend delivers a clean, reactive interface across seven feature pages.

---

## Features

- **Authentication** — Register and log in with JWT-based sessions (60-minute expiry)
- **Dashboard** — Portfolio summary with a performance chart and asset allocation breakdown
- **Holdings** — Sortable table of current positions with live unrealized gain/loss and portfolio weight
- **Trading** — Browse available securities with live prices, execute buy/sell orders
- **Trade Log** — Permanent, append-only record of every transaction
- **Bank Accounts** — Link up to 3 accounts (Chase, Bank of America, Chime) with deposit/withdrawal support
- **Fund Transfers** — Initiate, approve, or reject pending transfers between bank and portfolio cash
- **Dark Mode** — Full theme toggle across all pages

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9, C# |
| ORM | Entity Framework Core 9 (Pomelo MySQL) |
| Auth | JWT Bearer tokens, BCrypt password hashing |
| Database | MySQL |
| Frontend | Angular 21, TypeScript |
| Reactive | RxJS |
| Charts | Chart.js |
| Testing | xUnit (server), Vitest (client) |
| API Docs | OpenAPI / Swagger |
| Market Data | Alpha Vantage API |

---

## Project Structure

```
WealthView/
├── app/
│   ├── Client/       # Angular 21 frontend
│   ├── Server/       # ASP.NET Core 9 backend
│   └── Server.Tests/ # xUnit test project
└── docs/
    ├── controller-endpoints.md
    ├── user-stories.md
    └── schema-diagram.png
```

Each application has its own README with setup instructions:

- [Backend — app/Server](app/Server/README.md)
- [Frontend — app/Client](app/Client/README.md)

---

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org)
- [MySQL 8+](https://dev.mysql.com/downloads/)
- [Angular CLI](https://angular.dev/tools/cli) — `npm install -g @angular/cli`

### 1. Clone the repository

```bash
git clone https://github.com/ThomasClaiborne/WealthView.git
cd WealthView
```

### 2. Configure and run the backend

```bash
cd app/Server
cp appsettings.example.json appsettings.json
# Edit appsettings.json with your MySQL credentials and JWT secret
dotnet ef database update
dotnet run
```

Server runs at `https://localhost:5001`

### 3. Run the frontend

```bash
cd app/Client
npm install
ng serve
```

Client runs at `http://localhost:4200`

---

## How It Works

**Buying a security:** The server validates the user has sufficient cash, deducts the cost, creates or updates the holding using weighted average cost, and appends a trade record — all within a single database transaction.

**Selling a security:** Cash is returned, the holding quantity decreases (and is deleted if it reaches zero), and a sell trade is appended. The cost basis doesn't change on sells.

**Fund transfers:** Created in a `Pending` state and only execute when explicitly approved. Approval re-validates balances at execution time, so stale approvals can't overdraw accounts.

**Portfolio value:** Computed on each request by summing `cash + Σ(quantity × current price)` across all holdings. Daily snapshots are saved to power the performance chart.

---

## License

MIT — see [LICENSE](LICENSE)
