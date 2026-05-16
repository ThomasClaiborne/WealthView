import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BankAccountResponse } from './models';

@Injectable({ providedIn: 'root' })
export class BankAccountService {
    private http    = inject(HttpClient);
    private baseUrl = 'http://localhost:5266/api/bank-accounts';

    getAll(): Observable<BankAccountResponse[]> {
        return this.http.get<BankAccountResponse[]>(this.baseUrl);
    }

    add(bankName: string, nickname: string | null, startingBalance: number): Observable<BankAccountResponse> {
        return this.http.post<BankAccountResponse>(this.baseUrl, { bankName, nickname, startingBalance });
    }

    deposit(id: number, amount: number): Observable<BankAccountResponse> {
        return this.http.post<BankAccountResponse>(`${this.baseUrl}/${id}/deposit`, { amount });
    }

    withdraw(id: number, amount: number): Observable<BankAccountResponse> {
        return this.http.post<BankAccountResponse>(`${this.baseUrl}/${id}/withdraw`, { amount });
    }

    deactivate(id: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}