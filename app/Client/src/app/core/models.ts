export interface AppUser {
    appUserId: number;
    username:  string;
    email:     string;
    firstName: string;
    lastName:  string;
}

export interface Security {
    ticker:         string;
    companyName:    string;
    assetClass:     string;  
    lastPrice:      number | null;
    priceFetchedAt: string | null;
}

export interface Trade {
    tradeId:       number;
    ticker:        string;
    tradeType:     string;
    quantity:      number;
    pricePerShare: number;
    totalValue:    number;
    executedAt:    string;
} 3
export interface AuthResponse {
    token:     string;
    appUserId: number;
    username:  string;
    email:     string;
    firstName: string;
    lastName:  string;
}
export interface PortfolioResponse {
    portfolioId:       number;
    cashBalance:       number;
    totalValue:        number;
    totalUnrealizedGl: number;
    holdingCount:      number;
}

export interface HoldingResponse {
    holdingId:       number;
    portfolioId:     number;
    ticker:          string;
    companyName:     string;
    assetClass:      string;
    quantity:        number;
    avgCost:         number;
    currentPrice:    number;
    marketValue:     number;
    unrealizedGl:    number;
    unrealizedGlPct: number;
    portfolioWeight: number;
}

export interface TradeResponse {
    tradeId:        number;
    ticker:         string;
    tradeType:      string;  
    quantity:       number;
    pricePerShare:  number;
    totalValue:     number;
    executedAt:     string;
    newCashBalance: number;
}