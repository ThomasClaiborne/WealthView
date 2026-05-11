# WealthView — Frontend Plan

---

## Tech Stack

| Concern      | Tool                          |
|--------------|-------------------------------|
| Framework    | Angular (TypeScript)          |
| Styling      | Bootstrap 5 (CDN)             |
| Charts       | Chart.js via ng2-charts       |
| HTTP         | Angular HttpClient            |
| Routing      | Angular Router                |
| Auth state   | AuthService + BehaviorSubject |
| Forms        | Angular Reactive Forms        |

---

## Routes

| Path             | Component             | Guard      |
|------------------|-----------------------|------------|
| `/`              | LandingComponent      | public     |
| `/login`         | LoginComponent        | publicOnly |
| `/register`      | RegisterComponent     | publicOnly |
| `/dashboard`     | DashboardComponent    | authGuard  |
| `/bank-accounts` | BankAccountsComponent | authGuard  |
| `/transfers`     | TransfersComponent    | authGuard  |
| `/trading`       | TradingComponent      | authGuard  |
| `/holdings`      | HoldingsComponent     | authGuard  |
| `/trade-log`     | TradeLogComponent     | authGuard  |

> `authGuard` — checks `AuthService.isLoggedIn()`. If false, redirects to `/login`.
> `publicOnly` — checks `AuthService.isLoggedIn()`. If true, redirects to `/dashboard`.

---

## Auth State

```
AuthService
  Singleton service shared across the entire app.
  Holds the logged-in user as shared global state — any component that needs
  to know who is logged in injects AuthService directly.

  currentUser$  : BehaviorSubject<AppUser | null>
                  null = logged out  /  AppUser object = logged in

  login(token, user)  → saves JWT to localStorage, marks user as logged in
  logout()            → clears localStorage, marks user as logged out
  getToken()          → reads JWT from localStorage (used by the interceptor)
  isLoggedIn()        → returns true if a user is currently logged in
```

```
AuthInterceptor
  Runs automatically on every outbound HTTP request.
  Reads the JWT via AuthService.getToken() and attaches it as an Authorization header.
  No component ever manually adds auth headers — it is handled here automatically.
```

> `authGuard` redirects unauthenticated users away from protected routes to `/login`.
> `publicOnlyGuard` redirects already-logged-in users away from `/login` and `/register` to `/dashboard`.

---

## Component Tree

> **[smart]** — injects services, owns state, makes HTTP calls, passes data down to children.
> **[dumb]**  — receives data via `@Input()` only, communicates via `@Output()` only, no service dependencies.

```
AppComponent
└── <router-outlet>
    │
    ├── AuthLayoutComponent
    │     Shell for all public pages — no sidebar
    │   │
    │   ├── NavbarComponent                                        [dumb]
    │   │     Logo + Login + Register links
    │   │
    │   └── <router-outlet>
    │       ├── LandingComponent                                   [smart]
    │       │     Hero page with Get Started and Login CTAs
    │       │
    │       ├── LoginComponent                                     [smart]
    │       │     Credential + password form — navigates to /dashboard on success
    │       │
    │       └── RegisterComponent                                  [smart]
    │               First name, last name, username, email, password — navigates to /dashboard on success
    │
    └── MainLayoutComponent                                        [authGuard]
          Shell for all authenticated pages — sidebar + router outlet
        │
        ├── SidebarComponent                                       [dumb]
        │     Left nav panel with all route links and user info at bottom
        │   ├── NavLinkComponent                                   [dumb]
        │   │     Single nav link — receives label and route
        │   └── UserProfileComponent                               [dumb]
        │         Avatar, name, and logout button
        │
        └── <router-outlet>
            │
            ├── DashboardComponent                                 [smart]
            │     Hub page — fetches all summary data on load and passes it to child components
            │   ├── PortfolioSummaryComponent                      [dumb]
            │   │     Four metric cards: total value, cash balance, unrealized G/L, holdings count
            │   │
            │   ├── PerformanceChartComponent                      [dumb]
            │   │     Line chart built from portfolio snapshot history
            │   │
            │   ├── AssetAllocationChartComponent                  [dumb]
            │   │     Donut chart breaking portfolio down by asset class and cash
            │   │
            │   ├── BankAccountsPreviewComponent                   [dumb]
            │   │     Short bank account list — links to /bank-accounts
            │   │
            │   ├── PendingTransfersPreviewComponent               [dumb]
            │   │     Short pending transfer list — links to /transfers
            │   │
            │   ├── HoldingsPreviewComponent                       [dumb]
            │   │     Short holdings list — links to /holdings
            │   │
            │   └── RecentTradesPreviewComponent                   [dumb]
            │         Three most recent trades — links to /trade-log
            │
            ├── BankAccountsComponent                              [smart]
            │     Fetches accounts — handles add, deposit, withdraw, and delete
            │   ├── BankAccountCardComponent                       [dumb]
            │   │     Single bank card — name, balance, Add Funds, Remove Funds, Delete buttons
            │   └── AddAccountFormComponent                        [dumb]
            │         Reactive form for adding or reactivating a bank account
            │
            ├── TransfersComponent                                 [smart]
            │     Fetches pending and history transfers — handles submit, approve, and reject
            │   ├── TransferFormComponent                          [dumb]
            │   │     New transfer request form — bank dropdown, direction toggle, amount input
            │   └── TransferListComponent                          [dumb]
            │         Tabbed list switching between pending and resolved transfers
            │       └── TransferRowComponent                       [dumb]
            │             Single transfer row — direction badge, status badge, Approve and Reject buttons
            │
            ├── TradingComponent                                   [smart]
            │     Fetches securities and cash balance — handles buy execution
            │   ├── SecuritiesTableComponent                       [dumb]
            │   │     Browsable table of all available securities with live prices
            │   └── BuyFormComponent                               [dumb]
            │         Quantity input with live total cost preview and Confirm Buy button
            │
            ├── HoldingsComponent                                  [smart]
            │     Fetches all holdings — handles sell execution
            │   ├── HoldingsTableComponent                         [dumb]
            │   │     Sortable table of all holdings — all columns, green/red G/L
            │   └── SellFormComponent                              [dumb]
            │         Shows owned quantity, quantity input, proceeds preview, Confirm Sell button
            │
            └── TradeLogComponent                                  [smart]
                  Fetches complete trade history — read only
                └── TradeTableComponent                            [dumb]
                      Permanent append-only table — BUY/SELL badges, newest first

Shared (used across features)
    ├── BadgeComponent                                             [dumb]
    │     Colored pill for BUY, SELL, PENDING, APPROVED, REJECTED, Equity, ETF, Fixed Income
    ├── LoadingSpinnerComponent                                    [dumb]
    │     Centered spinner shown while HTTP calls are in flight
    └── ConfirmDialogComponent                                     [dumb]
          Confirmation prompt shown before destructive actions like delete
```

---

## Folder Structure

```
src/app/
├── core/
│   ├── guards/
│   │   ├── auth.guard.ts
│   │   └── public-only.guard.ts
│   └── interceptors/
│       └── auth.interceptor.ts
│
├── shared/
│   └── components/
│       ├── badge/
│       ├── loading-spinner/
│       └── confirm-dialog/
│
├── layout/
│   ├── auth-layout/
│   ├── main-layout/
│   ├── sidebar/
│   ├── nav-link/
│   └── user-profile/
│
├── features/
│   ├── auth/
│   │   ├── navbar/
│   │   ├── landing/
│   │   ├── login/
│   │   └── register/
│   ├── dashboard/
│   │   ├── pages/dashboard/
│   │   └── components/
│   │       ├── portfolio-summary/
│   │       ├── performance-chart/
│   │       ├── asset-allocation-chart/
│   │       ├── bank-accounts-preview/
│   │       ├── pending-transfers-preview/
│   │       ├── holdings-preview/
│   │       └── recent-trades-preview/
│   ├── bank-accounts/
│   │   ├── pages/bank-accounts/
│   │   └── components/
│   │       ├── bank-account-card/
│   │       └── add-account-form/
│   ├── transfers/
│   │   ├── pages/transfers/
│   │   └── components/
│   │       ├── transfer-form/
│   │       ├── transfer-list/
│   │       └── transfer-row/
│   ├── trading/
│   │   ├── pages/trading/
│   │   └── components/
│   │       ├── securities-table/
│   │       └── buy-form/
│   ├── holdings/
│   │   ├── pages/holdings/
│   │   └── components/
│   │       ├── holdings-table/
│   │       └── sell-form/
│   └── trade-log/
│       ├── pages/trade-log/
│       └── components/
│           └── trade-table/
│
└── app.routes.ts
```
