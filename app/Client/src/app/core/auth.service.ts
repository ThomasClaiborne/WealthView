import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { AppUser, AuthResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private http   = inject(HttpClient);
    private router = inject(Router);

    private readonly API = 'http://localhost:5266/api/auth';
    private userSubject  = new BehaviorSubject<AppUser | null>(null);

    currentUser$ = this.userSubject.asObservable();

    constructor() {
        // Re-hydrate session on page refresh
        const stored = localStorage.getItem('currentUser');
        if (stored) {
            try   { this.userSubject.next(JSON.parse(stored)); }
            catch { localStorage.clear(); }
        }
    }

    register(body: object): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.API}/register`, body)
            .pipe(tap(res => this.setSession(res)));
    }

    login(body: object): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.API}/login`, body)
            .pipe(tap(res => this.setSession(res)));
    }

    logout(): void {
        localStorage.removeItem('token');
        localStorage.removeItem('currentUser');
        this.userSubject.next(null);
        this.router.navigate(['/']);
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }

    isLoggedIn(): boolean {
        return this.userSubject.value !== null;
    }

    get currentUser(): AppUser | null {
        return this.userSubject.value;
    }

    private setSession(res: AuthResponse): void {
        const user: AppUser = {
            appUserId: res.appUserId,
            username:  res.username,
            email:     res.email,
            firstName: res.firstName,
            lastName:  res.lastName
        };
        localStorage.setItem('token',       res.token);
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.userSubject.next(user);
    }
}