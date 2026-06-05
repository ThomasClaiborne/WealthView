USE wealthview;

-- ─────────────────────────────────────────────────────────────────────────────
-- WealthView Demo Data — Account 2
-- Active for 1 week (May 10 – May 17 2026), Chime only
--
-- SETUP ORDER:
--   1. Run schema.sql (if not already done)
--   2. Register account 2 on the frontend (app_user_id = 2)
--   3. Run this file
--
-- Can be run independently of initial-data-user1.sql.
-- Run initial-data-securities.sql first if securities are not yet seeded.
-- ─────────────────────────────────────────────────────────────────────────────

-- ── Bank Accounts ─────────────────────────────────────────────────────────────
INSERT INTO bank_account (app_user_id, bank_name, nickname, balance, is_active, last_activated_at, created_at) VALUES
(2, 'Chime', NULL, 6000.0000, TRUE, '2026-05-10 09:00:00', '2026-05-10 09:00:00');

-- ── Portfolio ─────────────────────────────────────────────────────────────────
UPDATE portfolio SET cash_balance = 2543.68, created_at = '2026-05-10 09:00:00' WHERE app_user_id = 2;

-- ── Holdings ──────────────────────────────────────────────────────────────────
INSERT INTO holding (portfolio_id, ticker, quantity, avg_cost, created_at, updated_at) VALUES
(2, 'AAPL', 3.0000, 296.00, '2026-05-11 10:00:00', '2026-05-14 10:00:00'),
(2, 'BND',  5.0000,  71.80, '2026-05-12 10:00:00', '2026-05-12 10:00:00');

-- ── Trades ────────────────────────────────────────────────────────────────────
INSERT INTO trade (portfolio_id, ticker, trade_type, quantity, price_per_share, total_value, executed_at) VALUES
(2, 'AAPL', 'Buy', 2.0000, 294.00, 588.00, '2026-05-11 10:00:00'),
(2, 'BND',  'Buy', 5.0000,  71.80, 359.00, '2026-05-12 10:00:00'),
(2, 'AAPL', 'Buy', 1.0000, 300.00, 300.00, '2026-05-14 10:00:00');

-- ── Fund Transfers ────────────────────────────────────────────────────────────
-- bank_account_id for account 2's Chime — use the ID assigned when inserted above
-- If user1 data was run first: bank_account_id = 4
-- If user1 data was NOT run:   bank_account_id = 1
-- Update the value below to match what's in your bank_account table for app_user_id = 2
SET @chime2 = (SELECT bank_account_id FROM bank_account WHERE app_user_id = 2 AND bank_name = 'Chime' LIMIT 1);

INSERT INTO fund_transfer (portfolio_id, bank_account_id, direction, amount, status, created_at, resolved_at) VALUES
(2, @chime2, 'Deposit',    3000.0000, 'Approved', '2026-05-10 09:00:00', '2026-05-10 09:05:00'),
(2, @chime2, 'Deposit',     500.0000, 'Approved', '2026-05-13 10:00:00', '2026-05-13 10:10:00'),
(2, @chime2, 'Withdrawal',  200.0000, 'Rejected', '2026-05-15 09:00:00', '2026-05-15 09:20:00'),
(2, @chime2, 'Deposit',    1000.0000, 'Pending',  '2026-05-17 07:00:00', NULL);

-- ── Portfolio Snapshots ───────────────────────────────────────────────────────
-- May 10 – May 16 (today's snapshot created by app on first dashboard load)
INSERT INTO portfolio_snapshot (portfolio_id, snapshot_date, total_value) VALUES
(2, '2026-05-10', 3000.00),
(2, '2026-05-11', 3580.00),
(2, '2026-05-12', 3920.00),
(2, '2026-05-13', 4390.00),
(2, '2026-05-14', 4670.00),
(2, '2026-05-15', 4580.00),
(2, '2026-05-16', 4710.00),
(2, '2026-05-17', 3900.00);

UPDATE portfolio_snapshot SET total_value = 3200.00 WHERE portfolio_id = 2 AND snapshot_date = '2026-05-17';
