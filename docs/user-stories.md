# WealthView — User Stories

---

## While Logged Out

### Authentication
- Users can view the WealthView landing page with an overview of the app
- Users can register for a new account with a first name, last name, unique username, unique email, and password
- Users are automatically logged in and redirected to their dashboard after successful registration
- Users can log in with either their username and password or their email and password
- Users are redirected to their dashboard after a successful login

---

## While Logged In

> The left sidebar navigation is accessible on every page and contains links to all major sections of the app. The user's name and avatar anchor the bottom of the sidebar. Logout is accessible from the sidebar on every page.

### Authentication
- Users can log out from the sidebar navigation on any page

---

### Dashboard
- Users are taken to the dashboard immediately after logging in or registering
- Users can view a summary of their portfolio including total portfolio value, portfolio cash balance, total unrealized gain/loss, and number of active holdings
- Users can see an asset allocation donut chart breaking portfolio value down by asset class (Equity, ETF, Fixed Income, Cash)
- Users can see a portfolio performance line chart showing total portfolio value over time with one data point per day
- Users can see a preview of their bank accounts showing each account name and current balance
- Users can see a preview of their most recent pending fund transfers
- Users can see a preview of their current holdings with live prices
- Users can see a preview of their most recent trades
- Users can click any preview section to navigate to that section's full page

---

### Bank Accounts
- Users can view a list of all their active bank accounts, each displaying the bank name, nickname, and current balance
- Users can add a bank account by selecting a bank (Chase, Bank of America, or Chime), providing an optional nickname, and setting a starting balance
- Users cannot add a second active bank account from the same bank (one active account per bank, per user)
- Users can add funds directly to a bank account to simulate external deposits (such as a paycheck), which increases the bank account balance
- Users can remove funds directly from a bank account to simulate external spending, which decreases the bank account balance
- Users cannot remove more funds from a bank account than its current balance
- Users can soft-delete a bank account, which deactivates it and removes it from view but preserves all its transfer history
- Users can re-add a previously deleted bank account from the same bank, which reactivates the account with a new starting balance

---

### Fund Transfers
- Users can submit a deposit request to move money from a bank account into their portfolio cash balance
- Users can submit a withdrawal request to move money from their portfolio cash balance into a bank account
- Users cannot submit a deposit for more than the current bank account balance
- Users cannot submit a withdrawal for more than the current portfolio cash balance
- Users can view all pending (unresolved) transfer requests
- Users can approve a pending transfer, which immediately executes the balance changes across both accounts in a single transaction
- Users can cancel a pending transfer, which marks it as rejected and makes no balance changes
- Users can view their full transfer history showing all resolved transfers (approved and rejected) with date, direction, amount, bank account, and final status

---

### Trading
- Users can browse the full list of available securities showing ticker, company name, asset class, and current live price
- Users can view the detail of any security including company name, asset class, and current live price
- Users can buy shares of any available security by selecting a ticker and entering a quantity to purchase
- Users cannot buy shares if their portfolio cash balance is insufficient to cover the total cost (quantity × live price)
- Users can sell shares of any holding they currently own by selecting a holding and entering a quantity to sell
- Users can sell a partial quantity of a holding — any amount from 1 share up to the full quantity owned
- Users cannot sell more shares of a holding than they currently own
- Buy and sell trades execute instantly with no approval step
- When a user sells all shares of a holding, that holding is automatically removed from their account
- Proceeds from a sale are immediately credited to the portfolio cash balance

---

### Holdings
- Users can view a table of all their current holdings
- Each holding row displays: ticker, company name, asset class, quantity owned, average cost per share, current live price, market value, unrealized gain/loss, and portfolio weight as a percentage
- Unrealized gain/loss is displayed in green when positive and red when negative
- Users can sort the holdings table by any column

---

### Trade Log
- Users can view a complete and permanent history of every buy and sell transaction they have ever made
- Each trade record displays: date and time, type (Buy or Sell), ticker, quantity, price per share at time of execution, and total value
- Trade records are never edited or deleted — the log is append-only

---

## Stretch Goals

- Users can filter the trade log by ticker, trade type, or date range
- Users can search for a security by ticker or company name on the trading page
- Users can view a dedicated detail page for any security showing a price history chart
- Users can export their trade log as a CSV file
- Users can edit their account information including username, email, first name, and last name
- Users can change their password from their account settings page
- Users can upload a profile image that appears in the sidebar and on their account page
- Users can toggle between light mode and dark mode, with their preference saved and persisted across sessions
