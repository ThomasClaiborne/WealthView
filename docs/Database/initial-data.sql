USE wealthview;

-- Users (plain text passwords until BCrypt is wired up)
INSERT INTO app_user (username, email, password_hash, first_name, last_name, created_at) VALUES
('jdoe',   'john@wealthview.com', 'Test1234!', 'John', 'Doe',   '2026-01-01 09:00:00'),
('jsmith', 'jane@wealthview.com', 'Test1234!', 'Jane', 'Smith', '2026-01-01 09:00:00');

-- Securities (9 curated tickers — pre-seeded, prices filled by Alpha Vantage)
INSERT INTO security (ticker, company_name, asset_class, last_price, price_fetched_at) VALUES
('AAPL',  'Apple Inc.',                     'Equity',       NULL, NULL),
('MSFT',  'Microsoft Corp.',                'Equity',       NULL, NULL),
('GOOGL', 'Alphabet Inc.',                  'Equity',       NULL, NULL),
('TSLA',  'Tesla Inc.',                     'Equity',       NULL, NULL),
('JPM',   'JPMorgan Chase',                 'Equity',       NULL, NULL),
('SPY',   'SPDR S&P 500 ETF',               'ETF',          NULL, NULL),
('QQQ',   'Invesco QQQ Trust',              'ETF',          NULL, NULL),
('TLT',   'iShares 20+ Yr Bond ETF',        'FixedIncome',  NULL, NULL),
('BND',   'Vanguard Total Bond Market ETF', 'FixedIncome',  NULL, NULL);

-- Bank accounts
-- jdoe: 1 account (Chime)
INSERT INTO bank_account (app_user_id, bank_name, nickname, balance, is_active, last_activated_at, created_at) VALUES
(1, 'Chime', NULL, 5000.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00');

-- jsmith: all 3 accounts
INSERT INTO bank_account (app_user_id, bank_name, nickname, balance, is_active, last_activated_at, created_at) VALUES
(2, 'Chase',         'Personal Checking', 10000.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00'),
(2, 'BankOfAmerica', 'Savings',            5200.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00'),
(2, 'Chime',         NULL,                 1800.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00');

-- Portfolios
INSERT INTO portfolio (app_user_id, cash_balance, created_at) VALUES
(1,  850.0000, '2026-01-01 09:00:00'),
(2, 2050.0000, '2026-01-01 09:00:00');

-- Holdings
INSERT INTO holding (portfolio_id, ticker, quantity, avg_cost, created_at, updated_at) VALUES
(1, 'AAPL', 10.0000, 165.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00'),
(2, 'SPY',   5.0000, 430.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00'),
(2, 'TLT',   8.0000, 100.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00');

-- Trades
INSERT INTO trade (portfolio_id, ticker, trade_type, quantity, price_per_share, total_value, executed_at) VALUES
(1, 'AAPL', 'Buy', 10.0000, 165.0000, 1650.0000, '2026-01-02 10:00:00'),
(2, 'SPY',  'Buy',  5.0000, 430.0000, 2150.0000, '2026-01-02 10:00:00'),
(2, 'TLT',  'Buy',  8.0000, 100.0000,  800.0000, '2026-01-02 10:00:00');

-- Fund transfers
INSERT INTO fund_transfer (portfolio_id, bank_account_id, direction, amount, status, created_at, resolved_at) VALUES
(1, 1, 'Deposit',    2500.0000, 'Approved', '2026-01-01 10:00:00', '2026-01-01 10:01:00'),
(2, 2, 'Deposit',    5000.0000, 'Approved', '2026-01-01 10:00:00', '2026-01-01 10:01:00'),
(2, 3, 'Withdrawal', 1000.0000, 'Pending',  '2026-01-05 09:00:00', NULL);

-- Portfolio snapshots
INSERT INTO portfolio_snapshot (portfolio_id, snapshot_date, total_value) VALUES
(1, '2026-01-02', 2500.0000),
(1, '2026-01-03', 2650.0000),
(2, '2026-01-02', 5000.0000),
(2, '2026-01-03', 5300.0000);