# WealthView — Controller Endpoints

All protected endpoints require a valid JWT sent in the `Authorization` header as `Bearer <token>`.  
The JWT contains the authenticated user's ID — the backend uses this to scope all queries to the correct user.  
No endpoint ever accepts a `userId` in the request body or URL — identity always comes from the token.

---

## AuthController — `/api/auth`

### POST `/api/auth/register`
**Auth:** Public

**Request Body:**
```json
{
  "firstName": "Thomas",
  "lastName": "Smith",
  "username": "tsmith",
  "email": "thomas@email.com",
  "password": "SecurePass1!"
}
```

**Response:** `201 Created`
```json
{
  "token": "<jwt>",
  "user": {
    "appUserId": 1,
    "username": "tsmith",
    "email": "thomas@email.com",
    "firstName": "Thomas",
    "lastName": "Smith"
  }
}
```

**Business Rules:**
- Username must be 3–30 characters, letters/numbers/underscores only
- Username must be unique across all users
- Email must be unique across all users
- Password minimum 8 characters
- Password is hashed with BCrypt before storage — plain text is never saved
- Portfolio is automatically created for the new user in the same transaction
- Returns a JWT on success so the frontend can log the user in immediately

---

### POST `/api/auth/login`
**Auth:** Public

**Request Body:**
```json
{
  "credential": "tsmith",
  "password": "SecurePass1!"
}
```

**Response:** `200 OK`
```json
{
  "token": "<jwt>",
  "user": {
    "appUserId": 1,
    "username": "tsmith",
    "email": "thomas@email.com",
    "firstName": "Thomas",
    "lastName": "Smith"
  }
}
```

**Business Rules:**
- `credential` accepts either a username or an email
- If `credential` contains `@` → look up by email, otherwise look up by username
- BCrypt verifies the password against the stored hash
- Returns `401 Unauthorized` if credential not found or password does not match

---

## BankAccountController — `/api/bank-accounts`

### GET `/api/bank-accounts`
**Auth:** Required

**Response:** `200 OK`
```json
[
  {
    "bankAccountId": 1,
    "bankName": "Chase",
    "nickname": "Personal Checking",
    "balance": 10000.0000,
    "lastActivatedAt": "2025-05-09T10:00:00"
  }
]
```

**Business Rules:**
- Returns only `is_active = true` accounts belonging to the authenticated user

---

### POST `/api/bank-accounts`
**Auth:** Required

**Request Body:**
```json
{
  "bankName": "Chase",
  "nickname": "Personal Checking",
  "startingBalance": 10000.00
}
```

**Response:** `201 Created` — the created or reactivated bank account

**Business Rules:**
- `bankName` must be one of: `Chase`, `Bank of America`, `Chime`
- `startingBalance` must be >= 0
- If an **active** account for this bank already exists → `409 Conflict`
- If an **inactive** account for this bank exists → reactivate it, reset balance to `startingBalance`, update `last_activated_at`
- If no account exists → create a new row

---

### DELETE `/api/bank-accounts/{id}`
**Auth:** Required

**Response:** `204 No Content`

**Business Rules:**
- Verify the account belongs to the authenticated user → `403 Forbidden` if not
- If any `fund_transfer` with `status = PENDING` references this account → `409 Conflict`, cannot delete
- Sets `is_active = false` — the row is never deleted
- All transfer history tied to this account is preserved

---

### PATCH `/api/bank-accounts/{id}/deposit`
**Auth:** Required

**Request Body:**
```json
{
  "amount": 2500.00
}
```

**Response:** `200 OK`
```json
{
  "bankAccountId": 1,
  "balance": 12500.0000
}
```

**Business Rules:**
- Simulates external money coming in (paycheck, etc.) — not related to portfolio transfers
- `amount` must be > 0
- Verify account belongs to authenticated user and is active

---

### PATCH `/api/bank-accounts/{id}/withdraw`
**Auth:** Required

**Request Body:**
```json
{
  "amount": 500.00
}
```

**Response:** `200 OK`
```json
{
  "bankAccountId": 1,
  "balance": 10000.0000
}
```

**Business Rules:**
- Simulates external spending — not related to portfolio transfers
- `amount` must be > 0
- `amount` must be <= current bank account balance → `400 Bad Request` if not
- Verify account belongs to authenticated user and is active

---

## PortfolioController — `/api/portfolio`

### GET `/api/portfolio`
**Auth:** Required

**Response:** `200 OK`
```json
{
  "portfolioId": 1,
  "cashBalance": 2500.0000,
  "totalValue": 8320.0000,
  "totalUnrealizedGl": 820.0000,
  "holdingCount": 3
}
```

**Business Rules:**
- `totalValue` = `cashBalance` + sum of all holding market values (computed, not stored)
- `totalUnrealizedGl` = sum of (quantity × current price − quantity × avg_cost) across all holdings (computed)
- `holdingCount` = count of active holdings
- Snapshot logic runs here: if no `portfolio_snapshot` exists for today → calculate `totalValue` and insert one

---

### GET `/api/portfolio/snapshots`
**Auth:** Required

**Response:** `200 OK`
```json
[
  { "snapshotDate": "2025-05-01", "totalValue": 7500.0000 },
  { "snapshotDate": "2025-05-02", "totalValue": 7800.0000 },
  { "snapshotDate": "2025-05-09", "totalValue": 8320.0000 }
]
```

**Business Rules:**
- Returns all snapshots for the authenticated user's portfolio ordered by date ascending
- Used directly by the frontend to render the performance line chart

---

## SecurityController — `/api/securities`

### GET `/api/securities`
**Auth:** Required

**Response:** `200 OK`
```json
[
  {
    "ticker": "AAPL",
    "companyName": "Apple Inc.",
    "assetClass": "Equity",
    "lastPrice": 182.5000,
    "priceFetchedAt": "2025-05-09T09:35:00"
  }
]
```

**Business Rules:**
- Returns all securities in the security table (the curated list)
- If `price_fetched_at` is not today → call Alpha Vantage `GLOBAL_QUOTE` for each ticker, update `last_price` and `price_fetched_at`
- If `price_fetched_at` is already today → return cached price, no API call

---

### GET `/api/securities/{ticker}`
**Auth:** Required

**Response:** `200 OK` — single security object  
**Response:** `404 Not Found` if ticker does not exist in the table

---

## HoldingController — `/api/holdings`

### GET `/api/holdings`
**Auth:** Required

**Response:** `200 OK`
```json
[
  {
    "holdingId": 1,
    "ticker": "AAPL",
    "companyName": "Apple Inc.",
    "assetClass": "Equity",
    "quantity": 10.0000,
    "avgCost": 165.0000,
    "currentPrice": 182.5000,
    "marketValue": 1825.0000,
    "unrealizedGl": 175.0000,
    "unrealizedGlPercent": 10.61,
    "portfolioWeight": 21.93
  }
]
```

**Business Rules:**
- Returns all holdings for the authenticated user's portfolio
- `marketValue` = `quantity × currentPrice` (computed)
- `unrealizedGl` = `marketValue − (quantity × avgCost)` (computed)
- `unrealizedGlPercent` = `unrealizedGl / (quantity × avgCost) × 100` (computed)
- `portfolioWeight` = `marketValue / totalPortfolioValue × 100` (computed)
- None of these computed fields are stored in the database

---

## TradeController — `/api/trades`

### GET `/api/trades`
**Auth:** Required

**Response:** `200 OK`
```json
[
  {
    "tradeId": 1,
    "ticker": "AAPL",
    "tradeType": "BUY",
    "quantity": 10.0000,
    "pricePerShare": 165.0000,
    "totalValue": 1650.0000,
    "executedAt": "2025-05-01T10:22:00"
  }
]
```

**Business Rules:**
- Returns all trades for the authenticated user's portfolio ordered by `executedAt` descending
- Records are never deleted — this is the permanent trade log

---

### POST `/api/trades/buy`
**Auth:** Required

**Request Body:**
```json
{
  "ticker": "AAPL",
  "quantity": 5
}
```

**Response:** `201 Created`
```json
{
  "tradeId": 3,
  "ticker": "AAPL",
  "tradeType": "BUY",
  "quantity": 5.0000,
  "pricePerShare": 182.5000,
  "totalValue": 912.5000,
  "executedAt": "2025-05-09T11:00:00",
  "newCashBalance": 1587.5000
}
```

**Business Rules:**
- `ticker` must exist in the `security` table → `404 Not Found` if not
- `quantity` must be > 0
- Fetch live price from Alpha Vantage (or use today's cache)
- `totalCost` = `quantity × livePrice`
- Portfolio `cashBalance` must be >= `totalCost` → `400 Bad Request` if insufficient funds
- All of the following happen in one transaction:
  - `portfolio.cash_balance` decreases by `totalCost`
  - If holding exists for this ticker → increase `quantity`, recalculate `avg_cost` (weighted average)
  - If no holding exists → insert new holding row
  - Insert trade record with `trade_type = BUY`

---

### POST `/api/trades/sell`
**Auth:** Required

**Request Body:**
```json
{
  "ticker": "AAPL",
  "quantity": 3
}
```

**Response:** `201 Created`
```json
{
  "tradeId": 4,
  "ticker": "AAPL",
  "tradeType": "SELL",
  "quantity": 3.0000,
  "pricePerShare": 182.5000,
  "totalValue": 547.5000,
  "executedAt": "2025-05-09T11:05:00",
  "newCashBalance": 2135.0000
}
```

**Business Rules:**
- A holding for this `ticker` must exist in the user's portfolio → `404 Not Found` if not
- `quantity` must be > 0
- `quantity` must be <= `holding.quantity` → `400 Bad Request` if overselling
- Fetch live price from Alpha Vantage (or use today's cache)
- `proceeds` = `quantity × livePrice`
- All of the following happen in one transaction:
  - `portfolio.cash_balance` increases by `proceeds`
  - `holding.quantity` decreases by `quantity`
  - If `holding.quantity` reaches 0 → delete the holding row
  - `avg_cost` does NOT change on a sell — only recalculated on buys
  - Insert trade record with `trade_type = SELL`

---

## FundTransferController — `/api/transfers`

### GET `/api/transfers/pending`
**Auth:** Required

**Response:** `200 OK`
```json
[
  {
    "fundTransferId": 1,
    "bankAccountId": 2,
    "bankName": "Chase",
    "nickname": "Personal Checking",
    "direction": "DEPOSIT",
    "amount": 5000.0000,
    "status": "PENDING",
    "createdAt": "2025-05-09T09:00:00"
  }
]
```

**Business Rules:**
- Returns only `status = PENDING` transfers for the authenticated user's portfolio

---

### GET `/api/transfers/history`
**Auth:** Required

**Response:** `200 OK` — same shape as above but includes `resolvedAt`, filtered to `APPROVED` and `REJECTED` only

**Business Rules:**
- Returns only resolved transfers (`APPROVED` or `REJECTED`)
- Ordered by `created_at` descending
- Filters by `created_at >= bank_account.last_activated_at` so reactivated accounts only show history from their current activation

---

### POST `/api/transfers`
**Auth:** Required

**Request Body:**
```json
{
  "bankAccountId": 2,
  "direction": "DEPOSIT",
  "amount": 5000.00
}
```

**Response:** `201 Created`
```json
{
  "fundTransferId": 1,
  "bankAccountId": 2,
  "direction": "DEPOSIT",
  "amount": 5000.0000,
  "status": "PENDING",
  "createdAt": "2025-05-09T09:00:00"
}
```

**Business Rules:**
- `direction` must be `DEPOSIT` or `WITHDRAWAL`
- `amount` must be > 0
- Verify `bankAccountId` belongs to the authenticated user and is active → `403 Forbidden` if not
- If `DEPOSIT` → `amount` must be <= bank account balance → `400 Bad Request` if not
- If `WITHDRAWAL` → `amount` must be <= portfolio cash balance → `400 Bad Request` if not
- No balances change yet — status is `PENDING`, balances are only affected on approval

---

### PATCH `/api/transfers/{id}/approve`
**Auth:** Required

**Response:** `200 OK`
```json
{
  "fundTransferId": 1,
  "status": "APPROVED",
  "resolvedAt": "2025-05-09T09:15:00"
}
```

**Business Rules:**
- Verify the transfer belongs to the authenticated user → `403 Forbidden` if not
- Transfer must be `PENDING` → `409 Conflict` if already resolved
- Re-validate balances at time of approval (balance may have changed since submission)
- All of the following happen in one transaction:
  - If `DEPOSIT` → `bank_account.balance` decreases, `portfolio.cash_balance` increases
  - If `WITHDRAWAL` → `portfolio.cash_balance` decreases, `bank_account.balance` increases
  - `fund_transfer.status` = `APPROVED`
  - `fund_transfer.resolved_at` = now

---

### PATCH `/api/transfers/{id}/reject`
**Auth:** Required

**Response:** `200 OK`
```json
{
  "fundTransferId": 1,
  "status": "REJECTED",
  "resolvedAt": "2025-05-09T09:10:00"
}
```

**Business Rules:**
- Verify the transfer belongs to the authenticated user → `403 Forbidden` if not
- Transfer must be `PENDING` → `409 Conflict` if already resolved
- No balance changes — only `status` and `resolved_at` are updated
