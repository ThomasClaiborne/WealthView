USE wealthview;

-- ─────────────────────────────────────────────────────────────────────────────
-- WealthView Securities Seed Data
-- Run once after schema.sql — before any user data scripts
-- Prices captured from live Alpha Vantage fetch on May 17 2026
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO security (ticker, company_name, asset_class, last_price, price_fetched_at) VALUES
('AAPL',  'Apple Inc.',                     'Equity',      300.23, '2026-05-17 10:48:50'),
('MSFT',  'Microsoft Corp.',                'Equity',      421.92, '2026-05-17 10:48:55'),
('GOOGL', 'Alphabet Inc.',                  'Equity',      396.78, '2026-05-17 10:48:53'),
('TSLA',  'Tesla Inc.',                     'Equity',      422.24, '2026-05-17 10:49:00'),
('JPM',   'JPMorgan Chase',                 'Equity',      297.81, '2026-05-17 10:48:54'),
('SPY',   'SPDR S&P 500 ETF',               'ETF',         739.17, '2026-05-17 10:48:58'),
('QQQ',   'Invesco QQQ Trust',              'ETF',         708.93, '2026-05-17 10:48:56'),
('TLT',   'iShares 20+ Yr Bond ETF',        'FixedIncome',  83.66, '2026-05-17 10:48:59'),
('BND',   'Vanguard Total Bond Market ETF', 'FixedIncome',  72.74, '2026-05-17 10:48:51');
