import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FundTransferResponse } from './models';

@Injectable({ providedIn: 'root' })
export class FundTransferService {
    private http    = inject(HttpClient);
    private baseUrl = 'http://localhost:5266/api/transfers';

    getPending(): Observable<FundTransferResponse[]> {
        return this.http.get<FundTransferResponse[]>(`${this.baseUrl}/pending`);
    }

    getHistory(): Observable<FundTransferResponse[]> {
        return this.http.get<FundTransferResponse[]>(`${this.baseUrl}/history`);
    }

    requestDeposit(bankAccountId: number, amount: number): Observable<FundTransferResponse> {
        return this.http.post<FundTransferResponse>(`${this.baseUrl}/deposit`, { bankAccountId, amount });
    }

    requestWithdrawal(bankAccountId: number, amount: number): Observable<FundTransferResponse> {
        return this.http.post<FundTransferResponse>(`${this.baseUrl}/withdrawal`, { bankAccountId, amount });
    }

    approve(id: number): Observable<FundTransferResponse> {
        return this.http.post<FundTransferResponse>(`${this.baseUrl}/${id}/approve`, {});
    }

    reject(id: number): Observable<FundTransferResponse> {
        return this.http.post<FundTransferResponse>(`${this.baseUrl}/${id}/reject`, {});
    }
}