import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HoldingResponse } from './models';

@Injectable({ providedIn: 'root' })
export class HoldingService {
    private http = inject(HttpClient);
    private url = 'http://localhost:5266/api/holdings';

    getAll(): Observable<HoldingResponse[]> {
        return this.http.get<HoldingResponse[]>(this.url);
    }
}