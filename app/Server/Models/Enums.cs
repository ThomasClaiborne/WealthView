namespace Server.Models;

public enum AssetClass        { Equity, ETF, FixedIncome }
public enum BankName          { Chase, BankOfAmerica, Chime }
public enum TradeType         { Buy, Sell }
public enum TransferDirection { Deposit, Withdrawal }
public enum TransferStatus    { Pending, Approved, Rejected }