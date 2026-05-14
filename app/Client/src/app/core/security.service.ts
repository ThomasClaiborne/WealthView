import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Security } from './models';

@Injectable({ providedIn: 'root' })
export class SecurityService {
    private http = inject(HttpClient);
    private url = 'http://localhost:5266/api/securities';

    getAll(): Observable<Security[]> {
        return this.http.get<Security[]>(this.url);
    }
}