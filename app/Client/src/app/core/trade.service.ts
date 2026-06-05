import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Trade, TradeResponse } from './models';

@Injectable({ providedIn: 'root' })
export class TradeService {
    private http = inject(HttpClient);
    private baseUrl = 'http://localhost:5266/api/trades';

    getAll(): Observable<Trade[]> {
        return this.http.get<Trade[]>(this.baseUrl);
    }

    buy(ticker: string, quantity: number): Observable<TradeResponse> {
        return this.http.post<TradeResponse>(`${this.baseUrl}/buy`, { ticker, quantity });
    }

    sell(ticker: string, quantity: number): Observable<TradeResponse> {
        return this.http.post<TradeResponse>(`${this.baseUrl}/sell`, { ticker, quantity });
    }
}