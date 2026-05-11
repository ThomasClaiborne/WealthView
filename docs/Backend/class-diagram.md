# WealthView — Class Diagram

---

## Notes on the NB Stack vs Java Stack

| Java (Dev10)              | C# / NB Stack                                                          |
|---------------------------|------------------------------------------------------------------------|
| RowMapper<T>              | Not needed — EF Core maps automatically via navigation properties      |
| JdbcClient                | AppDbContext (EF Core)                                                 |
| @Service / @Repository    | Registered in Program.cs via builder.Services                          |
| ResponseEntity<T>         | IActionResult / ActionResult<T>                                        |
| @RestController           | [ApiController]                                                        |
| @GetMapping etc.          | [HttpGet], [HttpPost], [HttpPatch], [HttpDelete]                       |
| Result<T> (synchronous)   | Task<Result<T>> — async wrapper. await unwraps it.                     |
| ResultType enum           | ResultType enum — same concept, adds Forbidden                         |
| @NotNull / @NotBlank      | [Required] on DTOs (not domain models)                                 |
| List<String> errors       | List<string> errors (C# lowercase)                                     |
| BigDecimal                | decimal — never double or float for financial values                   |
| LocalDate                 | DateOnly — for snapshot dates where time is irrelevant                 |

---

## Models  (/Models)

> Plain C# classes (POCOs). No annotations needed on these.
> Validation annotations go on DTOs (request objects) instead.
> EF Core maps these to DB tables automatically by convention.
> Navigation properties are NOT database columns.
> They are C# objects EF Core populates in memory via .Include() —
> the equivalent of writing a JOIN + RowMapper in Java.
> Computed fields (MarketValue, UnrealizedGl, PortfolioWeight) are
> never stored in the DB — they are calculated in the service layer.

```
┌─────────────────────────────────────────────────────────────────┐
│ AppUser                                                         │
│ Represents a registered user. Maps to app_user table.           │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - AppUserId    : int                                           │
│  - Username     : string                                        │
│  - Email        : string                                        │
│  - PasswordHash : string                                        │
│  - FirstName    : string                                        │
│  - LastName     : string                                        │
│  - CreatedAt    : DateTime                                      │
│                                                                 │
│  [Navigation — populated by EF Core .Include()]                 │
│  - BankAccounts : List<BankAccount>                             │
│  - Portfolio    : Portfolio                                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ BankAccount                                                     │
│ A mock bank account owned by a user. Maps to bank_account.      │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - BankAccountId   : int                                        │
│  - AppUserId       : int          (FK → app_user)               │
│  - BankName        : BankName     (enum)                        │
│  - Nickname        : string?                                    │
│  - Balance         : decimal                                    │
│  - IsActive        : bool                                       │
│  - LastActivatedAt : DateTime                                   │
│  - CreatedAt       : DateTime                                   │
│                                                                 │
│  [Navigation]                                                   │
│  - AppUser       : AppUser                                      │
│  - FundTransfers : List<FundTransfer>                           │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Portfolio                                                       │
│ A user's investment account. One per user. Maps to portfolio.   │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - PortfolioId  : int                                           │
│  - AppUserId    : int      (FK → app_user, unique)              │
│  - CashBalance  : decimal                                       │
│  - CreatedAt    : DateTime                                      │
│                                                                 │
│  [Navigation]                                                   │
│  - AppUser       : AppUser                                      │
│  - Holdings      : List<Holding>                                │
│  - Trades        : List<Trade>                                  │
│  - FundTransfers : List<FundTransfer>                           │
│  - Snapshots     : List<PortfolioSnapshot>                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Security                                                        │
│ Ticker metadata and cached live price. Maps to security.        │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - Ticker         : string      (PK)                            │
│  - CompanyName    : string                                      │
│  - AssetClass     : AssetClass  (enum)                          │
│  - LastPrice      : decimal?                                    │
│  - PriceFetchedAt : DateTime?                                   │
│                                                                 │
│  [Navigation]                                                   │
│  - Holdings : List<Holding>                                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Holding                                                         │
│ A user's current ownership of one ticker. Maps to holding.      │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - HoldingId   : int                                            │
│  - PortfolioId : int      (FK → portfolio)                      │
│  - Ticker      : string   (FK → security)                       │
│  - Quantity    : decimal                                        │
│  - AvgCost     : decimal  (weighted average — recalculated on buy) │
│  - CreatedAt   : DateTime                                       │
│  - UpdatedAt   : DateTime                                       │
│                                                                 │
│  [Navigation]                                                   │
│  - Portfolio : Portfolio                                        │
│  - Security  : Security                                         │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ Trade                                                           │
│ Permanent record of every buy and sell. Maps to trade.          │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - TradeId       : int                                          │
│  - PortfolioId   : int        (FK → portfolio)                  │
│  - Ticker        : string     (plain text — intentionally no FK to Holding) │
│  - TradeType     : TradeType  (enum)                            │
│  - Quantity      : decimal                                      │
│  - PricePerShare : decimal                                      │
│  - TotalValue    : decimal                                      │
│  - ExecutedAt    : DateTime                                     │
│                                                                 │
│  [Navigation]                                                   │
│  - Portfolio : Portfolio                                        │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ FundTransfer                                                    │
│ A request to move money between a bank account and portfolio.   │
│ Maps to fund_transfer.                                          │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - FundTransferId : int                                         │
│  - PortfolioId    : int                (FK → portfolio)         │
│  - BankAccountId  : int                (FK → bank_account)      │
│  - Direction      : TransferDirection  (enum)                   │
│  - Amount         : decimal                                     │
│  - Status         : TransferStatus     (enum — default Pending) │
│  - CreatedAt      : DateTime                                    │
│  - ResolvedAt     : DateTime?                                   │
│                                                                 │
│  [Navigation]                                                   │
│  - Portfolio   : Portfolio                                      │
│  - BankAccount : BankAccount                                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ PortfolioSnapshot                                               │
│ Total portfolio value recorded once per day. Maps to            │
│ portfolio_snapshot. Powers the performance line chart.          │
├─────────────────────────────────────────────────────────────────┤
│ Fields                                                          │
│  - SnapshotId   : int                                           │
│  - PortfolioId  : int      (FK → portfolio)                     │
│  - SnapshotDate : DateOnly                                      │
│  - TotalValue   : decimal                                       │
│                                                                 │
│  [Navigation]                                                   │
│  - Portfolio : Portfolio                                        │
└─────────────────────────────────────────────────────────────────┘
```

### Enums  (/Models/Enums.cs)

```
public enum BankName          { Chase, BankOfAmerica, Chime }
public enum AssetClass        { Equity, ETF, FixedIncome }
public enum TradeType         { Buy, Sell }
public enum TransferDirection { Deposit, Withdrawal }
public enum TransferStatus    { Pending, Approved, Rejected }
```

---

## Request DTOs  (/DTOs/Requests)

> Incoming HTTP request bodies are deserialized into these objects.
> Validation annotations ([Required], [StringLength], etc.) live here —
> NOT on domain models. [ApiController] auto-validates and returns
> 400 Bad Request before the controller method runs if invalid.
> Java equivalent: @RequestBody model classes annotated with @NotBlank etc.

```
┌───────────────────────────────────────────────────────────────┐
│ RegisterRequest                                               │
├───────────────────────────────────────────────────────────────┤
│  - FirstName : string   [Required]                            │
│  - LastName  : string   [Required]                            │
│  - Username  : string   [Required] [StringLength(30, Min=3)]  │
│              RegEx: letters, numbers, underscores only        │
│  - Email     : string   [Required] [EmailAddress]             │
│  - Password  : string   [Required] [MinLength(8)]             │
└───────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│ LoginRequest                                               │
├────────────────────────────────────────────────────────────┤
│  - Credential : string   [Required]  (username or email)   │
│  - Password   : string   [Required]                        │
└────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ NewBankAccountRequest                                            │
├──────────────────────────────────────────────────────────────────┤
│  - BankName        : BankName   [Required]                       │
│  - Nickname        : string?    [StringLength(50)]               │
│  - StartingBalance : decimal    [Range(0, double.MaxValue)]      │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ AdjustBankBalanceRequest                                         │
│ Used for both deposit-to-bank and withdraw-from-bank endpoints.  │
├──────────────────────────────────────────────────────────────────┤
│  - Amount : decimal   [Range(0.01, double.MaxValue)]             │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ NewTransferRequest                                               │
├──────────────────────────────────────────────────────────────────┤
│  - BankAccountId : int                [Required]                 │
│  - Direction     : TransferDirection  [Required]                 │
│  - Amount        : decimal            [Range(0.01, double.MaxValue)] │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ BuyRequest                                                       │
├──────────────────────────────────────────────────────────────────┤
│  - Ticker   : string    [Required]                               │
│  - Quantity : decimal   [Range(0.0001, double.MaxValue)]         │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ SellRequest                                                      │
├──────────────────────────────────────────────────────────────────┤
│  - Ticker   : string    [Required]                               │
│  - Quantity : decimal   [Range(0.0001, double.MaxValue)]         │
└──────────────────────────────────────────────────────────────────┘
```

---

## Response DTOs  (/DTOs/Responses)

> Used when the response shape differs from the domain model —
> primarily when computed fields must be attached before sending.
> Computed fields are calculated in the service layer and
> set onto the response DTO — they are never stored in the DB.

```
┌──────────────────────────────────────────────────────────────────┐
│ AuthResponse                                                     │
│ Returned on successful register or login.                        │
├──────────────────────────────────────────────────────────────────┤
│  - Token     : string                                            │
│  - AppUserId : int                                               │
│  - Username  : string                                            │
│  - Email     : string                                            │
│  - FirstName : string                                            │
│  - LastName  : string                                            │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ PortfolioResponse                                                │
│ Portfolio with computed summary fields attached.                 │
├──────────────────────────────────────────────────────────────────┤
│  - PortfolioId        : int                                      │
│  - CashBalance        : decimal                                  │
│  - TotalValue         : decimal   (CashBalance + all MarketValues) │
│  - TotalUnrealizedGl  : decimal   (sum of all holding G/L)       │
│  - HoldingCount       : int                                      │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ HoldingResponse                                                  │
│ Holding with live price and computed fields attached.            │
├──────────────────────────────────────────────────────────────────┤
│  - HoldingId        : int                                        │
│  - PortfolioId      : int                                        │
│  - Ticker           : string                                     │
│  - CompanyName      : string     (from Security)                 │
│  - AssetClass       : AssetClass (from Security)                 │
│  - Quantity         : decimal                                    │
│  - AvgCost          : decimal                                    │
│  - CurrentPrice     : decimal    (from Security.LastPrice)       │
│  - MarketValue      : decimal    (Quantity × CurrentPrice)       │
│  - UnrealizedGl     : decimal    (MarketValue − Quantity × AvgCost) │
│  - UnrealizedGlPct  : decimal    (UnrealizedGl / CostBasis × 100) │
│  - PortfolioWeight  : decimal    (MarketValue / TotalPortfolioValue × 100) │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ TradeResponse                                                    │
│ Trade record with the portfolio's updated cash balance attached. │
├──────────────────────────────────────────────────────────────────┤
│  - TradeId        : int                                          │
│  - Ticker         : string                                       │
│  - TradeType      : TradeType                                    │
│  - Quantity       : decimal                                      │
│  - PricePerShare  : decimal                                      │
│  - TotalValue     : decimal                                      │
│  - ExecutedAt     : DateTime                                     │
│  - NewCashBalance : decimal   (portfolio cash after the trade)   │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ FundTransferResponse                                             │
│ Transfer with bank name attached for display.                    │
├──────────────────────────────────────────────────────────────────┤
│  - FundTransferId : int                                          │
│  - BankAccountId  : int                                          │
│  - BankName       : string              (from BankAccount)       │
│  - Direction      : TransferDirection                            │
│  - Amount         : decimal                                      │
│  - Status         : TransferStatus                               │
│  - CreatedAt      : DateTime                                     │
│  - ResolvedAt     : DateTime?                                    │
└──────────────────────────────────────────────────────────────────┘
```

---

## Data Layer  (/Data)

> Repositories handle all DB reads and writes.
> Each interface (I*) has one EF Core implementation (Ef*Repository).
> AppDbContext is the EF Core equivalent of JdbcClient / DataSource.
> Java: interface + JdbcClientRepository injecting JdbcClient.
> C#:   interface + EfRepository injecting AppDbContext.

```
┌──────────────────────────────────────────────────────────────────────┐
│ AppDbContext  extends DbContext                                       │
│ The EF Core session. One instance per HTTP request (scoped).         │
│ Registered in Program.cs via builder.Services.AddDbContext<>().      │
├──────────────────────────────────────────────────────────────────────┤
│ DbSets (one per table — EF Core uses these to query and write)        │
│  - Users              : DbSet<AppUser>                               │
│  - BankAccounts       : DbSet<BankAccount>                           │
│  - Portfolios         : DbSet<Portfolio>                             │
│  - Securities         : DbSet<Security>                              │
│  - Holdings           : DbSet<Holding>                               │
│  - Trades             : DbSet<Trade>                                 │
│  - FundTransfers      : DbSet<FundTransfer>                          │
│  - PortfolioSnapshots : DbSet<PortfolioSnapshot>                     │
├──────────────────────────────────────────────────────────────────────┤
│  # OnModelCreating(builder: ModelBuilder) : void                     │
│    - Configures decimal(18,4) precision on all decimal columns       │
│    - Configures unique index on Portfolio.AppUserId (one-to-one)     │
│    - Configures unique index on (Holding.PortfolioId, Holding.Ticker)│
│    - Configures unique index on (Snapshot.PortfolioId, Snapshot.SnapshotDate)│
│    - Stores enums as strings for DB readability                      │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IUserRepository  (interface)                                         │
│ Auth layer only — no update or delete.                               │
├──────────────────────────────────────────────────────────────────────┤
│  + GetByCredential(credential: string)  : Task<AppUser?>             │
│    Checks if credential contains '@' — looks up by email or username │
│  + GetById(id: int)                     : Task<AppUser?>             │
│  + Create(user: AppUser)                : Task<AppUser>              │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfUserRepository  implements IUserRepository                         │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IUserRepository methods)                            │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IBankAccountRepository  (interface)                                  │
│ CRUD on bank accounts + soft delete + reactivation support.          │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAllActiveByUserId(userId: int)                                 │
│        : Task<List<BankAccount>>                                     │
│  + GetById(id: int)                  : Task<BankAccount?>            │
│  + GetInactiveByUserAndBank(userId: int, bankName: BankName)         │
│        : Task<BankAccount?>                                          │
│  + HasPendingTransfers(id: int)      : Task<bool>                    │
│  + Create(account: BankAccount)      : Task<BankAccount>             │
│  + Update(account: BankAccount)      : Task<BankAccount?>            │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfBankAccountRepository  implements IBankAccountRepository           │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IBankAccountRepository methods)                     │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IPortfolioRepository  (interface)                                    │
│ Read and update the portfolio. Created once at registration.         │
├──────────────────────────────────────────────────────────────────────┤
│  + GetByUserId(userId: int)           : Task<Portfolio?>             │
│  + GetById(id: int)                   : Task<Portfolio?>             │
│  + Create(portfolio: Portfolio)       : Task<Portfolio>              │
│  + UpdateCashBalance(portfolioId: int,                               │
│        newBalance: decimal)           : Task<bool>                   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfPortfolioRepository  implements IPortfolioRepository               │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IPortfolioRepository methods)                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ ISecurityRepository  (interface)                                     │
│ The ticker catalog. Read and price-cache update.                     │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll()                             : Task<List<Security>>       │
│  + GetByTicker(ticker: string)          : Task<Security?>            │
│  + Create(security: Security)           : Task<Security>             │
│  + UpdatePrice(ticker: string,                                       │
│        price: decimal)                  : Task<bool>                 │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfSecurityRepository  implements ISecurityRepository                 │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all ISecurityRepository methods)                        │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IHoldingRepository  (interface)                                      │
│ Current positions per portfolio. Insert, update, or delete.          │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAllByPortfolioId(portfolioId: int) : Task<List<Holding>>       │
│  + GetByPortfolioAndTicker(portfolioId: int,                         │
│        ticker: string)                  : Task<Holding?>             │
│  + Create(holding: Holding)             : Task<Holding>              │
│  + Update(holding: Holding)             : Task<Holding?>             │
│  + Delete(holdingId: int)               : Task<bool>                 │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfHoldingRepository  implements IHoldingRepository                   │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IHoldingRepository methods)                         │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ ITradeRepository  (interface)                                        │
│ Append-only. No update or delete — ever.                             │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAllByPortfolioId(portfolioId: int) : Task<List<Trade>>         │
│  + Create(trade: Trade)                 : Task<Trade>                │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfTradeRepository  implements ITradeRepository                       │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all ITradeRepository methods)                           │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IFundTransferRepository  (interface)                                 │
│ Transfers are never deleted. Status is the only mutable field.       │
├──────────────────────────────────────────────────────────────────────┤
│  + GetPendingByPortfolioId(portfolioId: int)                         │
│        : Task<List<FundTransfer>>                                    │
│  + GetHistoryByPortfolioId(portfolioId: int)                         │
│        : Task<List<FundTransfer>>                                    │
│  + GetById(id: int)              : Task<FundTransfer?>               │
│  + Create(transfer: FundTransfer): Task<FundTransfer>                │
│  + UpdateStatus(id: int, status: TransferStatus,                     │
│        resolvedAt: DateTime?)    : Task<bool>                        │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfFundTransferRepository  implements IFundTransferRepository         │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IFundTransferRepository methods)                    │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IPortfolioSnapshotRepository  (interface)                            │
│ One row per portfolio per day. Never edited or deleted.              │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAllByPortfolioId(portfolioId: int)                             │
│        : Task<List<PortfolioSnapshot>>                               │
│  + GetByPortfolioAndDate(portfolioId: int, date: DateOnly)           │
│        : Task<PortfolioSnapshot?>                                    │
│  + Create(snapshot: PortfolioSnapshot) : Task<PortfolioSnapshot>     │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ EfPortfolioSnapshotRepository  implements IPortfolioSnapshotRepository│
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _db : AppDbContext                                                │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IPortfolioSnapshotRepository methods)               │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Domain Layer  (/Domain)

> Services contain all business logic and financial rules.
> Each interface (I*) has one implementation.
> Controllers call services — never repositories directly.
> userId is always extracted from the JWT — never accepted in the request body.
> Java equivalent: @Service classes implementing interfaces,
> using Result<T> pattern to communicate outcome to the controller.

### Result Pattern

```
┌───────────────────────────────────────────────────────────────┐
│ ResultType  (enum)                                            │
├───────────────────────────────────────────────────────────────┤
│  Success, NotFound, Invalid, Forbidden                        │
└───────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────┐
│ Result<T>                                                     │
├───────────────────────────────────────────────────────────────┤
│  - Type    : ResultType                                       │
│  - Data    : T?                                               │
│  - Errors  : List<string>                                     │
└───────────────────────────────────────────────────────────────┘
```

### Services

```
┌──────────────────────────────────────────────────────────────────────┐
│ IAuthService  (interface)                                            │
├──────────────────────────────────────────────────────────────────────┤
│  + Register(request: RegisterRequest)  : Task<Result<AuthResponse>>  │
│    Validates username and email uniqueness.                          │
│    Hashes password with BCrypt.                                      │
│    Creates user and auto-creates Portfolio in one transaction.       │
│    Returns JWT + user on success.                                    │
│                                                                      │
│  + Login(request: LoginRequest)        : Task<Result<AuthResponse>>  │
│    Looks up user by credential (username or email).                  │
│    Verifies password against BCrypt hash.                            │
│    Result.Type = NotFound if credential not found.                   │
│    Result.Type = Invalid if password does not match.                 │
│    Returns JWT + user on success.                                    │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ AuthService  implements IAuthService                                 │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _userRepo      : IUserRepository                                  │
│  - _portfolioRepo : IPortfolioRepository                             │
│  - _config        : IConfiguration      (reads JWT secret/expiry)   │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IAuthService methods)                               │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IBankAccountService  (interface)                                     │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll(userId: int)                                               │
│        : Task<Result<List<BankAccount>>>                             │
│                                                                      │
│  + AddAccount(userId: int, request: NewBankAccountRequest)           │
│        : Task<Result<BankAccount>>                                   │
│    Checks for active account with same bank → Result.Type = Invalid. │
│    Checks for inactive account → reactivates with new balance.       │
│    Otherwise creates a new row.                                      │
│                                                                      │
│  + Deposit(accountId: int, userId: int, request: AdjustBankBalanceRequest)│
│        : Task<Result<BankAccount>>                                   │
│    Simulates external deposit. Increases bank balance.               │
│    Result.Type = NotFound if account missing or not owned by userId. │
│                                                                      │
│  + Withdraw(accountId: int, userId: int, request: AdjustBankBalanceRequest)│
│        : Task<Result<BankAccount>>                                   │
│    Simulates external withdrawal. Decreases bank balance.            │
│    Result.Type = Invalid if amount > current balance.                │
│                                                                      │
│  + Delete(accountId: int, userId: int)                               │
│        : Task<Result<bool>>                                          │
│    Result.Type = Forbidden if account not owned by userId.           │
│    Result.Type = Invalid if account has pending transfers.           │
│    Sets IsActive = false — row is never deleted.                     │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ BankAccountService  implements IBankAccountService                   │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _bankAccountRepo : IBankAccountRepository                         │
│  - _transferRepo    : IFundTransferRepository                        │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IBankAccountService methods)                        │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IPortfolioService  (interface)                                       │
├──────────────────────────────────────────────────────────────────────┤
│  + GetByUserId(userId: int)                                          │
│        : Task<Result<PortfolioResponse>>                             │
│    Computes TotalValue, TotalUnrealizedGl, HoldingCount.             │
│    Creates a snapshot for today if one does not exist.               │
│                                                                      │
│  + GetSnapshots(userId: int)                                         │
│        : Task<Result<List<PortfolioSnapshot>>>                       │
│    Returns all snapshots ordered by date ascending for line chart.   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ PortfolioService  implements IPortfolioService                       │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _portfolioRepo  : IPortfolioRepository                            │
│  - _holdingRepo    : IHoldingRepository                              │
│  - _securityRepo   : ISecurityRepository                             │
│  - _snapshotRepo   : IPortfolioSnapshotRepository                    │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IPortfolioService methods)                          │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ ISecurityService  (interface)                                        │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll()                             : Task<Result<List<Security>>>│
│    Refreshes prices via Alpha Vantage for any ticker where           │
│    PriceFetchedAt is not today. Returns updated list.                │
│                                                                      │
│  + GetByTicker(ticker: string)          : Task<Result<Security>>     │
│    Result.Type = NotFound if ticker not in security table.           │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ SecurityService  implements ISecurityService                         │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _securityRepo      : ISecurityRepository                          │
│  - _marketDataService : IMarketDataService                           │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all ISecurityService methods)                           │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IHoldingService  (interface)                                         │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAllByPortfolioId(portfolioId: int)                             │
│        : Task<Result<List<HoldingResponse>>>                         │
│    Joins Security for CompanyName and AssetClass.                    │
│    Computes MarketValue, UnrealizedGl, UnrealizedGlPct,              │
│    and PortfolioWeight for each holding before returning.            │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ HoldingService  implements IHoldingService                           │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _holdingRepo   : IHoldingRepository                               │
│  - _securityRepo  : ISecurityRepository                              │
│  - _portfolioRepo : IPortfolioRepository                             │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IHoldingService methods)                            │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ ITradeService  (interface)                                           │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll(portfolioId: int)             : Task<Result<List<Trade>>>  │
│                                                                      │
│  + Buy(portfolioId: int, request: BuyRequest)                        │
│        : Task<Result<TradeResponse>>                                 │
│    Result.Type = NotFound if ticker not in security table.           │
│    Fetches live price via IMarketDataService.                        │
│    Result.Type = Invalid if portfolio cash < totalCost.              │
│    In one transaction:                                               │
│      - portfolio.CashBalance decreases by totalCost                  │
│      - if holding exists: increase Quantity, recalculate AvgCost     │
│      - if no holding: insert new Holding row                         │
│      - insert Trade record (TradeType = Buy)                         │
│    Returns TradeResponse including NewCashBalance.                   │
│                                                                      │
│  + Sell(portfolioId: int, request: SellRequest)                      │
│        : Task<Result<TradeResponse>>                                 │
│    Result.Type = NotFound if user does not hold this ticker.         │
│    Result.Type = Invalid if request.Quantity > holding.Quantity.     │
│    Fetches live price via IMarketDataService.                        │
│    In one transaction:                                               │
│      - portfolio.CashBalance increases by proceeds                   │
│      - holding.Quantity decreases — holding deleted if Quantity = 0  │
│      - AvgCost is NOT changed on sell                                │
│      - insert Trade record (TradeType = Sell)                        │
│    Returns TradeResponse including NewCashBalance.                   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ TradeService  implements ITradeService                               │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _tradeRepo         : ITradeRepository                             │
│  - _holdingRepo       : IHoldingRepository                           │
│  - _portfolioRepo     : IPortfolioRepository                         │
│  - _securityRepo      : ISecurityRepository                          │
│  - _marketDataService : IMarketDataService                           │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all ITradeService methods)                              │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IFundTransferService  (interface)                                    │
├──────────────────────────────────────────────────────────────────────┤
│  + GetPending(portfolioId: int)                                      │
│        : Task<Result<List<FundTransferResponse>>>                    │
│                                                                      │
│  + GetHistory(portfolioId: int)                                      │
│        : Task<Result<List<FundTransferResponse>>>                    │
│                                                                      │
│  + Create(portfolioId: int, userId: int, request: NewTransferRequest)│
│        : Task<Result<FundTransferResponse>>                          │
│    Verifies bank account belongs to userId → Result.Type = Forbidden.│
│    If Deposit: Result.Type = Invalid if amount > bank balance.       │
│    If Withdrawal: Result.Type = Invalid if amount > portfolio cash.  │
│    No balances change — status starts as Pending.                    │
│                                                                      │
│  + Approve(transferId: int, portfolioId: int)                        │
│        : Task<Result<FundTransferResponse>>                          │
│    Result.Type = Invalid if transfer is not Pending.                 │
│    Re-validates balances at approval time.                           │
│    In one transaction:                                               │
│      - If Deposit: bank.Balance−=amount, portfolio.CashBalance+=amount│
│      - If Withdrawal: portfolio.CashBalance−=amount, bank.Balance+=amount│
│      - transfer.Status = Approved, transfer.ResolvedAt = now         │
│                                                                      │
│  + Reject(transferId: int, portfolioId: int)                         │
│        : Task<Result<FundTransferResponse>>                          │
│    Result.Type = Invalid if transfer is not Pending.                 │
│    No balance changes — only Status and ResolvedAt are updated.      │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ FundTransferService  implements IFundTransferService                 │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _transferRepo    : IFundTransferRepository                        │
│  - _portfolioRepo   : IPortfolioRepository                           │
│  - _bankAccountRepo : IBankAccountRepository                         │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IFundTransferService methods)                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ IMarketDataService  (interface)                                      │
│ Wrapper around the Alpha Vantage API. Used by SecurityService        │
│ and TradeService to fetch live prices.                               │
├──────────────────────────────────────────────────────────────────────┤
│  + GetLivePrice(ticker: string)  : Task<decimal?>                    │
│    Calls Alpha Vantage GLOBAL_QUOTE endpoint.                        │
│    Returns null if the API call fails.                               │
│    Updates security.LastPrice and PriceFetchedAt on success.         │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ MarketDataService  implements IMarketDataService                     │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _securityRepo : ISecurityRepository                               │
│  - _httpClient   : HttpClient                                        │
│  - _apiKey       : string            (read from appsettings.json)    │
├──────────────────────────────────────────────────────────────────────┤
│  (implements all IMarketDataService methods)                         │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Controllers  (/Controllers)

> Thin HTTP layer. Receives request → calls service → maps ResultType → returns status code.
> portfolioId is resolved from userId via PortfolioService — never accepted in the request body.
> userId is extracted from the JWT via User.FindFirstValue(ClaimTypes.NameIdentifier).
> [Authorize] marks endpoints that require a valid JWT.
> Java equivalent: @RestController using ResponseEntity + result.getType() switch.

```
┌──────────────────────────────────────────────────────────────────────┐
│ AuthController   [ApiController]  [Route("api/auth")]                │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _authService : IAuthService                                       │
├──────────────────────────────────────────────────────────────────────┤
│  + Register  [HttpPost("register")]                                  │
│      (request: RegisterRequest) : Task<IActionResult>               │
│    201 Created + AuthResponse on success. 400 if Invalid.            │
│                                                                      │
│  + Login     [HttpPost("login")]                                     │
│      (request: LoginRequest) : Task<IActionResult>                   │
│    200 OK + AuthResponse on success. 404 or 400 mapped from Result.  │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ BankAccountController   [ApiController]  [Route("api/bank-accounts")]│
│                         [Authorize]                                  │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _bankAccountService : IBankAccountService                         │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll    [HttpGet]                                               │
│      () : Task<IActionResult>                                        │
│    200 OK + account list.                                            │
│                                                                      │
│  + Add       [HttpPost]                                              │
│      (request: NewBankAccountRequest) : Task<IActionResult>          │
│    201 Created + account. 400 if Invalid (duplicate or bad input).   │
│                                                                      │
│  + Deposit   [HttpPatch("{id}/deposit")]                             │
│      (id: int, request: AdjustBankBalanceRequest) : Task<IActionResult>│
│    200 OK + updated account. 404 if NotFound. 403 if Forbidden.      │
│                                                                      │
│  + Withdraw  [HttpPatch("{id}/withdraw")]                            │
│      (id: int, request: AdjustBankBalanceRequest) : Task<IActionResult>│
│    200 OK + updated account. 400 if insufficient balance.            │
│                                                                      │
│  + Delete    [HttpDelete("{id}")]                                    │
│      (id: int) : Task<IActionResult>                                 │
│    204 No Content. 403 if Forbidden. 400 if pending transfers exist. │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ PortfolioController   [ApiController]  [Route("api/portfolio")]      │
│                       [Authorize]                                    │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _portfolioService : IPortfolioService                             │
├──────────────────────────────────────────────────────────────────────┤
│  + Get          [HttpGet]                                            │
│      () : Task<IActionResult>                                        │
│    200 OK + PortfolioResponse with computed totals.                  │
│    Also triggers snapshot creation for today if not yet recorded.    │
│                                                                      │
│  + GetSnapshots [HttpGet("snapshots")]                               │
│      () : Task<IActionResult>                                        │
│    200 OK + snapshot list ordered by date ascending.                 │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ SecurityController   [ApiController]  [Route("api/securities")]      │
│                      [Authorize]                                     │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _securityService : ISecurityService                               │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll       [HttpGet]                                            │
│      () : Task<IActionResult>                                        │
│    200 OK + security list with refreshed prices.                     │
│                                                                      │
│  + GetByTicker  [HttpGet("{ticker}")]                                │
│      (ticker: string) : Task<IActionResult>                          │
│    200 OK + security. 404 if NotFound.                               │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ HoldingController   [ApiController]  [Route("api/holdings")]         │
│                     [Authorize]                                      │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _holdingService  : IHoldingService                                │
│  - _portfolioService: IPortfolioService                              │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll  [HttpGet]                                                 │
│      () : Task<IActionResult>                                        │
│    200 OK + HoldingResponse list with all computed fields.           │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ TradeController   [ApiController]  [Route("api/trades")]             │
│                   [Authorize]                                        │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _tradeService    : ITradeService                                  │
│  - _portfolioService: IPortfolioService                              │
├──────────────────────────────────────────────────────────────────────┤
│  + GetAll  [HttpGet]                                                 │
│      () : Task<IActionResult>                                        │
│    200 OK + trade list ordered newest first.                         │
│                                                                      │
│  + Buy     [HttpPost("buy")]                                         │
│      (request: BuyRequest) : Task<IActionResult>                     │
│    201 Created + TradeResponse. 400 if Invalid. 404 if NotFound.     │
│                                                                      │
│  + Sell    [HttpPost("sell")]                                        │
│      (request: SellRequest) : Task<IActionResult>                    │
│    201 Created + TradeResponse. 400 if Invalid. 404 if NotFound.     │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│ FundTransferController   [ApiController]  [Route("api/transfers")]   │
│                          [Authorize]                                 │
├──────────────────────────────────────────────────────────────────────┤
│ Fields                                                               │
│  - _transferService : IFundTransferService                           │
│  - _portfolioService: IPortfolioService                              │
├──────────────────────────────────────────────────────────────────────┤
│  + GetPending  [HttpGet("pending")]                                  │
│      () : Task<IActionResult>                                        │
│    200 OK + pending transfer list.                                   │
│                                                                      │
│  + GetHistory  [HttpGet("history")]                                  │
│      () : Task<IActionResult>                                        │
│    200 OK + resolved transfer list (Approved and Rejected only).     │
│                                                                      │
│  + Create   [HttpPost]                                               │
│      (request: NewTransferRequest) : Task<IActionResult>             │
│    201 Created + FundTransferResponse (status = Pending).            │
│    400 if Invalid. 403 if Forbidden.                                 │
│                                                                      │
│  + Approve  [HttpPatch("{id}/approve")]                              │
│      (id: int) : Task<IActionResult>                                 │
│    200 OK + updated FundTransferResponse. 400 if not Pending.        │
│                                                                      │
│  + Reject   [HttpPatch("{id}/reject")]                               │
│      (id: int) : Task<IActionResult>                                 │
│    200 OK + updated FundTransferResponse. 400 if not Pending.        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Dependency Registration  (Program.cs)

> Java equivalent: your App.java composition root.
> AddScoped = one instance per HTTP request, then discarded.
> Use AddScoped for everything that touches EF Core — DbContext is
> scoped by default and anything depending on it must also be scoped.
> AddSingleton would cause a "cannot consume scoped from singleton" error.
> HttpClient for Alpha Vantage is registered via AddHttpClient.

```
builder.Services.AddScoped<IUserRepository,                 EfUserRepository>();
builder.Services.AddScoped<IBankAccountRepository,          EfBankAccountRepository>();
builder.Services.AddScoped<IPortfolioRepository,            EfPortfolioRepository>();
builder.Services.AddScoped<ISecurityRepository,             EfSecurityRepository>();
builder.Services.AddScoped<IHoldingRepository,              EfHoldingRepository>();
builder.Services.AddScoped<ITradeRepository,                EfTradeRepository>();
builder.Services.AddScoped<IFundTransferRepository,         EfFundTransferRepository>();
builder.Services.AddScoped<IPortfolioSnapshotRepository,    EfPortfolioSnapshotRepository>();

builder.Services.AddScoped<IAuthService,           AuthService>();
builder.Services.AddScoped<IBankAccountService,    BankAccountService>();
builder.Services.AddScoped<IPortfolioService,      PortfolioService>();
builder.Services.AddScoped<ISecurityService,       SecurityService>();
builder.Services.AddScoped<IHoldingService,        HoldingService>();
builder.Services.AddScoped<ITradeService,          TradeService>();
builder.Services.AddScoped<IFundTransferService,   FundTransferService>();
builder.Services.AddScoped<IMarketDataService,     MarketDataService>();

builder.Services.AddHttpClient<MarketDataService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

---

## Project Folder Structure  (/server)

```
server/
├── Controllers/
│   ├── AuthController.cs
│   ├── BankAccountController.cs
│   ├── PortfolioController.cs
│   ├── SecurityController.cs
│   ├── HoldingController.cs
│   ├── TradeController.cs
│   └── FundTransferController.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── IUserRepository.cs                  EfUserRepository.cs
│   ├── IBankAccountRepository.cs           EfBankAccountRepository.cs
│   ├── IPortfolioRepository.cs             EfPortfolioRepository.cs
│   ├── ISecurityRepository.cs              EfSecurityRepository.cs
│   ├── IHoldingRepository.cs               EfHoldingRepository.cs
│   ├── ITradeRepository.cs                 EfTradeRepository.cs
│   ├── IFundTransferRepository.cs          EfFundTransferRepository.cs
│   └── IPortfolioSnapshotRepository.cs     EfPortfolioSnapshotRepository.cs
├── Domain/
│   ├── ResultType.cs
│   ├── Result.cs
│   ├── IAuthService.cs                     AuthService.cs
│   ├── IBankAccountService.cs              BankAccountService.cs
│   ├── IPortfolioService.cs                PortfolioService.cs
│   ├── ISecurityService.cs                 SecurityService.cs
│   ├── IHoldingService.cs                  HoldingService.cs
│   ├── ITradeService.cs                    TradeService.cs
│   ├── IFundTransferService.cs             FundTransferService.cs
│   └── IMarketDataService.cs               MarketDataService.cs
├── Models/
│   ├── AppUser.cs
│   ├── BankAccount.cs
│   ├── Portfolio.cs
│   ├── Security.cs
│   ├── Holding.cs
│   ├── Trade.cs
│   ├── FundTransfer.cs
│   ├── PortfolioSnapshot.cs
│   └── Enums.cs
├── DTOs/
│   ├── Requests/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   ├── NewBankAccountRequest.cs
│   │   ├── AdjustBankBalanceRequest.cs
│   │   ├── NewTransferRequest.cs
│   │   ├── BuyRequest.cs
│   │   └── SellRequest.cs
│   └── Responses/
│       ├── AuthResponse.cs
│       ├── PortfolioResponse.cs
│       ├── HoldingResponse.cs
│       ├── TradeResponse.cs
│       └── FundTransferResponse.cs
└── Program.cs
```
