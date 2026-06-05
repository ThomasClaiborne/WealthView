import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { ThemeService } from '../core/theme.service';
import { AppUser } from '../core/models';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [RouterLink, RouterLinkActive, CommonModule],
    templateUrl: './sidebar.html'
})
export class Sidebar implements OnInit, OnDestroy {
    private authService  = inject(AuthService);
    private themeService = inject(ThemeService);
    private sub!: Subscription;

    user: AppUser | null = null;

    get isDark(): boolean {
        return this.themeService.isDark;
    }

    ngOnInit() {
        this.sub = this.authService.currentUser$.subscribe(u => this.user = u);
        this.themeService.init();
    }

    ngOnDestroy() {
        this.sub.unsubscribe();
    }

    toggleTheme() {
        this.themeService.toggle();
    }

    logout() {
        this.authService.logout();
    }
}
