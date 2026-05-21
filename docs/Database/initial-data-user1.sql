USE wealthview;

-- ─────────────────────────────────────────────────────────────────────────────
-- WealthView Demo Data — Securities + User 1
-- Run order:
--   1. schema.sql
--   2. This file (securities + user 1 data)
--   3. Register account 1 on the frontend (app_user_id = 1)
--   4. Run the UPDATE + INSERT blocks below for user 1
--
-- Today is May 20 2026. Snapshots go through May 19.
-- App creates May 20 snapshot on first dashboard load.
-- ─────────────────────────────────────────────────────────────────────────────
-- USER 1 DATA
-- Register account 1 on the frontend first, then run everything below.
-- ─────────────────────────────────────────────────────────────────────────────

-- ── Bank Account ──────────────────────────────────────────────────────────────
-- Chase only. Balance reflects starting deposit minus trades net.
INSERT INTO bank_account (app_user_id, bank_name, nickname, balance, is_active, last_activated_at, created_at) VALUES
(1, 'Chase', 'Main Checking', 4723.92, TRUE, '2026-04-20 09:00:00', '2026-04-20 09:00:00');

-- ── Portfolio ─────────────────────────────────────────────────────────────────
-- Cash balance ~matches the May 20 snapshot total minus holdings market value.
-- Holdings market value at today's prices:
--   AAPL  8 x 298.97 =  2391.76
--   MSFT  3 x 417.42 =  1252.26  (avg_cost 445 → in the red)
--   SPY   4 x 733.73 =  2934.92
--   QQQ   2 x 701.53 =  1403.06  (avg_cost 720 → in the red)
--   TLT  10 x  83.02 =   830.20
--   Total holdings = 8812.20
-- Cash + holdings ≈ 27600 total portfolio on May 20
UPDATE portfolio
SET cash_balance = 18812.43,
    created_at   = '2026-04-20 09:00:00'
WHERE app_user_id = 1;

-- ── Holdings ──────────────────────────────────────────────────────────────────
-- AAPL avg_cost 271 → currently 298.97 = IN THE GREEN (+$224 unrealized)
-- MSFT avg_cost 445 → currently 417.42 = IN THE RED   (-$83 unrealized)
-- SPY  avg_cost 698 → currently 733.73 = IN THE GREEN (+$143 unrealized)
-- QQQ  avg_cost 720 → currently 701.53 = IN THE RED   (-$37 unrealized)
-- TLT  avg_cost  80 → currently  83.02 = IN THE GREEN (+$30 unrealized)
INSERT INTO holding (portfolio_id, ticker, quantity, avg_cost, created_at, updated_at) VALUES
(1, 'AAPL',  8.0000, 271.00, '2026-04-22 10:00:00', '2026-05-06 10:00:00'),
(1, 'MSFT',  3.0000, 445.00, '2026-04-29 10:00:00', '2026-04-29 10:00:00'),
(1, 'SPY',   4.0000, 698.00, '2026-04-24 10:00:00', '2026-05-12 10:00:00'),
(1, 'QQQ',   2.0000, 720.00, '2026-05-07 10:00:00', '2026-05-07 10:00:00'),
(1, 'TLT',  10.0000,  80.00, '2026-05-01 10:00:00', '2026-05-01 10:00:00');

-- ── Trades ────────────────────────────────────────────────────────────────────
INSERT INTO trade (portfolio_id, ticker, trade_type, quantity, price_per_share, total_value, executed_at) VALUES
-- Week 1 (Apr 20-26)
(1, 'AAPL', 'Buy',  5.0000, 268.00, 1340.00, '2026-04-22 10:00:00'),
(1, 'SPY',  'Buy',  2.0000, 695.00, 1390.00, '2026-04-24 10:00:00'),
-- Week 2 (Apr 27 - May 3)
(1, 'MSFT', 'Buy',  3.0000, 445.00, 1335.00, '2026-04-29 10:00:00'),
(1, 'TLT',  'Buy', 10.0000,  80.00,  800.00, '2026-05-01 10:00:00'),
-- Week 3 (May 4-10)
(1, 'AAPL', 'Buy',  5.0000, 275.00, 1375.00, '2026-05-06 10:00:00'),
(1, 'AAPL', 'Sell', 2.0000, 281.00,  562.00, '2026-05-07 10:00:00'),
(1, 'QQQ',  'Buy',  2.0000, 720.00, 1440.00, '2026-05-07 10:00:00'),
-- Week 4 (May 11-17)
(1, 'SPY',  'Buy',  2.0000, 702.00, 1404.00, '2026-05-12 10:00:00'),
(1, 'GOOGL','Buy',  2.0000, 381.00,  762.00, '2026-05-15 10:00:00'),
(1, 'GOOGL','Sell', 2.0000, 390.00,  780.00, '2026-05-16 10:00:00');

-- ── Fund Transfers ────────────────────────────────────────────────────────────
-- bank_account_id = 1 (Chase — only account)
INSERT INTO fund_transfer (portfolio_id, bank_account_id, direction, amount, status, created_at, resolved_at) VALUES
-- Initial deposit to get started
(1, 1, 'Deposit',    15000.0000, 'Approved', '2026-04-20 09:00:00', '2026-04-20 09:05:00'),
-- Mid month top up
(1, 1, 'Deposit',     8000.0000, 'Approved', '2026-05-05 09:00:00', '2026-05-05 09:10:00'),
-- A withdrawal that was rejected
(1, 1, 'Withdrawal',  2000.0000, 'Rejected', '2026-05-10 10:00:00', '2026-05-10 10:30:00'),
-- Pending withdrawal for the demo
(1, 1, 'Withdrawal',   500.0000, 'Pending',  '2026-05-19 08:00:00', NULL);

-- ── Portfolio Snapshots ───────────────────────────────────────────────────────
-- Apr 20 through May 19 (30 days)
-- May 20 snapshot created by app on first dashboard load
-- Values show realistic fluctuation with general upward trend
-- Dip around May 7-9, recovery May 10+
INSERT INTO portfolio_snapshot (portfolio_id, snapshot_date, total_value) VALUES
(1, '2026-04-20', 15000.00),
(1, '2026-04-21', 15120.00),
(1, '2026-04-22', 16390.00),
(1, '2026-04-23', 16280.00),
(1, '2026-04-24', 17650.00),
(1, '2026-04-25', 17820.00),
(1, '2026-04-26', 17740.00),
(1, '2026-04-27', 17930.00),
(1, '2026-04-28', 18050.00),
(1, '2026-04-29', 19360.00),
(1, '2026-04-30', 20140.00),
(1, '2026-05-01', 20930.00),
(1, '2026-05-02', 21100.00),
(1, '2026-05-03', 21280.00),
(1, '2026-05-04', 21190.00),
(1, '2026-05-05', 29040.00),
(1, '2026-05-06', 30210.00),
(1, '2026-05-07', 31480.00),
(1, '2026-05-08', 30920.00),
(1, '2026-05-09', 30540.00),
(1, '2026-05-10', 30280.00),
(1, '2026-05-11', 30650.00),
(1, '2026-05-12', 31870.00),
(1, '2026-05-13', 32040.00),
(1, '2026-05-14', 31880.00),
(1, '2026-05-15', 32560.00),
(1, '2026-05-16', 32780.00),
(1, '2026-05-17', 27190.00),
(1, '2026-05-18', 27350.00),
(1, '2026-05-19', 27480.00);