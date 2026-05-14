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