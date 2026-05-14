import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PortfolioResponse } from './models';

@Injectable({ providedIn: 'root' })
export class PortfolioService {
    private http = inject(HttpClient);
    private url = 'http://localhost:5266/api/portfolio';

    getPortfolio(): Observable<PortfolioResponse> {
        return this.http.get<PortfolioResponse>(this.url);
    }
}