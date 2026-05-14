
CREATE DATABASE IF NOT EXISTS wealthview_test;
USE wealthview_test;

CREATE TABLE app_user (
    app_user_id   INT          NOT NULL AUTO_INCREMENT,
    username      VARCHAR(30)  NOT NULL,
    email         VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name    VARCHAR(50)  NOT NULL,
    last_name     VARCHAR(50)  NOT NULL,
    created_at    DATETIME     NOT NULL,
    PRIMARY KEY (app_user_id),
    UNIQUE KEY uq_username (username),
    UNIQUE KEY uq_email (email)
);

CREATE TABLE security (
    ticker           VARCHAR(10)   NOT NULL,
    company_name     VARCHAR(100)  NOT NULL,
    asset_class ENUM('Equity', 'ETF', 'FixedIncome') NOT NULL,
    last_price       DECIMAL(18,4),
    price_fetched_at DATETIME,
    PRIMARY KEY (ticker)
);

CREATE TABLE bank_account (
    bank_account_id   INT           NOT NULL AUTO_INCREMENT,
    app_user_id       INT           NOT NULL,
    bank_name ENUM('Chase', 'BankOfAmerica', 'Chime') NOT NULL,
    nickname          VARCHAR(50),
    balance           DECIMAL(18,4) NOT NULL DEFAULT 0.0000,
    is_active         BOOLEAN       NOT NULL DEFAULT TRUE,
    last_activated_at DATETIME      NOT NULL,
    created_at        DATETIME      NOT NULL,
    PRIMARY KEY (bank_account_id),
    UNIQUE KEY uq_user_bank (app_user_id, bank_name),
    CONSTRAINT fk_bank_account_user
        FOREIGN KEY (app_user_id) REFERENCES app_user(app_user_id)
);

CREATE TABLE portfolio (
    portfolio_id INT           NOT NULL AUTO_INCREMENT,
    app_user_id  INT           NOT NULL,
    cash_balance DECIMAL(18,4) NOT NULL DEFAULT 0.0000,
    created_at   DATETIME      NOT NULL,
    PRIMARY KEY (portfolio_id),
    UNIQUE KEY uq_portfolio_user (app_user_id),
    CONSTRAINT fk_portfolio_user
        FOREIGN KEY (app_user_id) REFERENCES app_user(app_user_id)
);

CREATE TABLE holding (
    holding_id   INT           NOT NULL AUTO_INCREMENT,
    portfolio_id INT           NOT NULL,
    ticker       VARCHAR(10)   NOT NULL,
    quantity     DECIMAL(18,4) NOT NULL,
    avg_cost     DECIMAL(18,4) NOT NULL,
    created_at   DATETIME      NOT NULL,
    updated_at   DATETIME      NOT NULL,
    PRIMARY KEY (holding_id),
    UNIQUE KEY uq_holding_portfolio_ticker (portfolio_id, ticker),
    CONSTRAINT fk_holding_portfolio
        FOREIGN KEY (portfolio_id) REFERENCES portfolio(portfolio_id),
    CONSTRAINT fk_holding_security
        FOREIGN KEY (ticker) REFERENCES security(ticker)
);

CREATE TABLE trade (
    trade_id        INT           NOT NULL AUTO_INCREMENT,
    portfolio_id    INT           NOT NULL,
    ticker          VARCHAR(10)   NOT NULL,
    trade_type ENUM('Buy', 'Sell') NOT NULL,
    quantity        DECIMAL(18,4) NOT NULL,
    price_per_share DECIMAL(18,4) NOT NULL,
    total_value     DECIMAL(18,4) NOT NULL,
    executed_at     DATETIME      NOT NULL,
    PRIMARY KEY (trade_id),
    CONSTRAINT fk_trade_portfolio
        FOREIGN KEY (portfolio_id) REFERENCES portfolio(portfolio_id)
);

CREATE TABLE fund_transfer (
    fund_transfer_id INT           NOT NULL AUTO_INCREMENT,
    portfolio_id     INT           NOT NULL,
    bank_account_id  INT           NOT NULL,
    direction        ENUM('Deposit', 'Withdrawal') NOT NULL,
    amount           DECIMAL(18,4) NOT NULL,
    status           ENUM('Pending', 'Approved', 'Rejected') NOT NULL DEFAULT 'Pending',
    created_at       DATETIME      NOT NULL,
    resolved_at      DATETIME,
    PRIMARY KEY (fund_transfer_id),
    CONSTRAINT fk_fund_transfer_portfolio
        FOREIGN KEY (portfolio_id) REFERENCES portfolio(portfolio_id),
    CONSTRAINT fk_fund_transfer_bank
        FOREIGN KEY (bank_account_id) REFERENCES bank_account(bank_account_id)
);

CREATE TABLE portfolio_snapshot (
    snapshot_id   INT           NOT NULL AUTO_INCREMENT,
    portfolio_id  INT           NOT NULL,
    snapshot_date DATE          NOT NULL,
    total_value   DECIMAL(18,4) NOT NULL,
    PRIMARY KEY (snapshot_id),
    UNIQUE KEY uq_snapshot_portfolio_date (portfolio_id, snapshot_date),
    CONSTRAINT fk_snapshot_portfolio
        FOREIGN KEY (portfolio_id) REFERENCES portfolio(portfolio_id)
);

-- ─────────────────────────────────────────────────────────────
-- KnownGoodState procedure
-- Call before every integration test: CALL set_known_good_state();
-- Wipes all tables and resets to a predictable baseline.
-- ─────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS set_known_good_state;

DELIMITER $$

CREATE PROCEDURE set_known_good_state()
BEGIN
    SET FOREIGN_KEY_CHECKS = 0;

    DELETE FROM portfolio_snapshot;
    DELETE FROM fund_transfer;
    DELETE FROM trade;
    DELETE FROM holding;
    DELETE FROM portfolio;
    DELETE FROM bank_account;
    DELETE FROM app_user;
    DELETE FROM security;

    ALTER TABLE portfolio_snapshot AUTO_INCREMENT = 1;
    ALTER TABLE fund_transfer AUTO_INCREMENT = 1;
    ALTER TABLE trade AUTO_INCREMENT = 1;
    ALTER TABLE holding AUTO_INCREMENT = 1;
    ALTER TABLE portfolio AUTO_INCREMENT = 1;
    ALTER TABLE bank_account AUTO_INCREMENT = 1;
    ALTER TABLE app_user AUTO_INCREMENT = 1;

    SET FOREIGN_KEY_CHECKS = 1;

    INSERT INTO app_user (app_user_id, username, email, password_hash, first_name, last_name, created_at) VALUES
    (1, 'jdoe',   'john@wealthview.com', 'Test1234!', 'John', 'Doe',   '2026-01-01 09:00:00'),
    (2, 'jsmith', 'jane@wealthview.com', 'Test1234!', 'Jane', 'Smith', '2026-01-01 09:00:00');

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

    INSERT INTO bank_account (bank_account_id, app_user_id, bank_name, nickname, balance, is_active, last_activated_at, created_at) VALUES
    (1, 1, 'Chime',         NULL,                5000.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00'),
    (2, 2, 'Chase',         'Personal Checking', 10000.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00'),
    (3, 2, 'BankOfAmerica', 'Savings',            5200.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00'),
    (4, 2, 'Chime',         NULL,                 1800.0000, TRUE, '2026-01-01 09:00:00', '2026-01-01 09:00:00');

    INSERT INTO portfolio (portfolio_id, app_user_id, cash_balance, created_at) VALUES
    (1, 1,  850.0000, '2026-01-01 09:00:00'),
    (2, 2, 2050.0000, '2026-01-01 09:00:00');

    INSERT INTO holding (holding_id, portfolio_id, ticker, quantity, avg_cost, created_at, updated_at) VALUES
    (1, 1, 'AAPL', 10.0000, 165.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00'),
    (2, 2, 'SPY',   5.0000, 430.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00'),
    (3, 2, 'TLT',   8.0000, 100.0000, '2026-01-02 10:00:00', '2026-01-02 10:00:00');

    INSERT INTO trade (trade_id, portfolio_id, ticker, trade_type, quantity, price_per_share, total_value, executed_at) VALUES
    (1, 1, 'AAPL', 'Buy', 10.0000, 165.0000, 1650.0000, '2026-01-02 10:00:00'),
    (2, 2, 'SPY',  'Buy',  5.0000, 430.0000, 2150.0000, '2026-01-02 10:00:00'),
    (3, 2, 'TLT',  'Buy',  8.0000, 100.0000,  800.0000, '2026-01-02 10:00:00');

    INSERT INTO fund_transfer (fund_transfer_id, portfolio_id, bank_account_id, direction, amount, status, created_at, resolved_at) VALUES
    (1, 1, 1, 'Deposit',    2500.0000, 'Approved', '2026-01-01 10:00:00', '2026-01-01 10:01:00'),
    (2, 2, 2, 'Deposit',    5000.0000, 'Approved', '2026-01-01 10:00:00', '2026-01-01 10:01:00'),
    (3, 2, 3, 'Withdrawal', 1000.0000, 'Pending',  '2026-01-05 09:00:00', NULL);

    INSERT INTO portfolio_snapshot (snapshot_id, portfolio_id, snapshot_date, total_value) VALUES
    (1, 1, '2026-01-02', 2500.0000),
    (2, 1, '2026-01-03', 2650.0000),
    (3, 2, '2026-01-02', 5000.0000),
    (4, 2, '2026-01-03', 5300.0000);
END$$

DELIMITER ;