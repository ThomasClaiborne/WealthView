# WealthView — Client

Angular 21 frontend for the WealthView portfolio management application. Provides a reactive, single-page interface for trading, holdings tracking, and portfolio analytics.

---

## Tech Stack

- **Framework:** Angular 21
- **Language:** TypeScript 5.9 (strict mode)
- **Reactive:** RxJS
- **Charts:** Chart.js
- **Testing:** Vitest
- **Formatting:** Prettier

---

## Pages

| Route | Description |
|-------|-------------|
| `/` | Landing page with product overview |
| `/register` | New account creation |
| `/login` | Login with username or email |
| `/dashboard` | Portfolio value, performance chart, asset allocation, holdings preview |
| `/holdings` | Sortable table of current positions with live P&L and portfolio weight |
| `/trading` | Browse securities with live prices, buy and sell |
| `/trade-log` | Full transaction history (append-only, never edited) |
| `/bank-accounts` | Link bank accounts, deposit and withdraw funds |
| `/transfers` | View and act on pending fund transfers |

Authenticated routes are protected by `AuthGuard`. Login/register redirect to dashboard if already signed in via `PublicOnlyGuard`.

---

## Project Structure

```
src/app/
├── auth/              # Landing, login, register pages
├── core/              # Services, guards, interceptor, shared models
│   ├── auth.service.ts
│   ├── auth.interceptor.ts
│   ├── auth.guard.ts
│   ├── public-only.guard.ts
│   ├── models.ts
│   ├── portfolio.service.ts
│   ├── holding.service.ts
│   ├── trade.service.ts
│   ├── security.service.ts
│   ├── bank-account.service.ts
│   ├── fund-transfer.service.ts
│   └── theme.service.ts
├── dashboard/
├── holdings/
├── trading/
├── trade-log/
├── bank-accounts/
├── transfers/
├── layout/            # MainLayout, AuthLayout, Sidebar
└── shared/            # Reusable components
```

---

## Setup

### Prerequisites

- Node.js 18+
- Angular CLI — `npm install -g @angular/cli`

### Install and run

```bash
npm install
ng serve
```

The app runs at `http://localhost:4200`. The API is expected at `https://localhost:5001` — make sure the backend is running first.

### Build for production

```bash
ng build
```

Output goes to `dist/`. The build is optimized and tree-shaken.

---

## Auth Flow

On login or register, the server returns a JWT. `AuthService` stores it in `localStorage` and attaches it to every outbound request via `AuthInterceptor`. On logout (or token expiry), the token is cleared and the user is redirected to `/login`.

---

## Running Tests

```bash
ng test
```

Uses Vitest with jsdom. Tests run in watch mode by default.
