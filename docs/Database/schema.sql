DROP DATABASE IF EXISTS wealthview;
CREATE DATABASE wealthview;
USE wealthview;

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
    direction ENUM('Deposit', 'Withdrawal') NOT NULL,
    amount           DECIMAL(18,4) NOT NULL,
    status ENUM('Pending', 'Approved', 'Rejected') NOT NULL DEFAULT 'Pending',
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