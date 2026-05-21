import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
    private _isDark = new BehaviorSubject<boolean>(
        localStorage.getItem('wv-theme') === 'dark'
    );

    isDark$ = this._isDark.asObservable();

    get isDark(): boolean {
        return this._isDark.value;
    }

    init(): void {
        this.apply(this._isDark.value);
    }

    toggle(): void {
        const next = !this._isDark.value;
        this._isDark.next(next);
        localStorage.setItem('wv-theme', next ? 'dark' : 'light');
        this.apply(next);
    }

    private apply(dark: boolean): void {
        document.body.setAttribute('data-theme', dark ? 'dark' : 'light');
    }
}
