USE wealthview;

-- ─────────────────────────────────────────────────────────────────────────────
-- WealthView Securities Seed Data
-- Run once after schema.sql — before any user data scripts
-- Prices from live Alpha Vantage fetch May 21 2026
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO security (ticker, company_name, asset_class, last_price, price_fetched_at) VALUES
('AAPL',  'Apple Inc.',                     'Equity',      304.99, '2026-05-22 13:52:06'),
('BND',   'Vanguard Total Bond Market ETF', 'FixedIncome',  72.93, '2026-05-22 13:52:07'),
('GOOGL', 'Alphabet Inc.',                  'Equity',      387.66, '2026-05-22 13:52:08'),
('JPM',   'JPMorgan Chase',                 'Equity',      303.00, '2026-05-22 13:52:10'),
('MSFT',  'Microsoft Corp.',                'Equity',      419.09, '2026-05-22 13:52:11'),
('QQQ',   'Invesco QQQ Trust',              'ETF',         714.51, '2026-05-22 13:52:12'),
('SPY',   'SPDR S&P 500 ETF',               'ETF',         742.72, '2026-05-22 13:52:13'),
('TLT',   'iShares 20+ Yr Bond ETF',        'FixedIncome',  84.22, '2026-05-22 13:52:15'),
('TSLA',  'Tesla Inc.',                     'Equity',      417.85, '2026-05-22 13:52:16');