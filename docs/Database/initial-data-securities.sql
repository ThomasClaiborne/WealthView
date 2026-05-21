USE wealthview;

-- ─────────────────────────────────────────────────────────────────────────────
-- WealthView Securities Seed Data
-- Run once after schema.sql — before any user data scripts
-- ─────────────────────────────────────────────────────────────────────────────

-- ── Securities (prices from live fetch May 20 2026) ───────────────────────────
INSERT INTO security (ticker, company_name, asset_class, last_price, price_fetched_at) VALUES
('AAPL',  'Apple Inc.',                     'Equity',      298.97, '2026-05-20 01:48:32'),
('BND',   'Vanguard Total Bond Market ETF', 'FixedIncome',  72.45, '2026-05-20 01:48:33'),
('GOOGL', 'Alphabet Inc.',                  'Equity',      387.66, '2026-05-20 01:48:35'),
('JPM',   'JPMorgan Chase',                 'Equity',      295.70, '2026-05-20 01:48:36'),
('MSFT',  'Microsoft Corp.',                'Equity',      417.42, '2026-05-20 01:48:37'),
('QQQ',   'Invesco QQQ Trust',              'ETF',         701.53, '2026-05-20 01:48:38'),
('SPY',   'SPDR S&P 500 ETF',               'ETF',         733.73, '2026-05-20 01:48:40'),
('TLT',   'iShares 20+ Yr Bond ETF',        'FixedIncome',  83.02, '2026-05-20 01:48:41'),
('TSLA',  'Tesla Inc.',                     'Equity',      404.11, '2026-05-20 01:48:42');

