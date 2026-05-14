import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { AppUser } from '../core/models';

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [RouterLink, RouterLinkActive],
    templateUrl: './sidebar.html'
})
export class Sidebar implements OnInit, OnDestroy {
    private authService = inject(AuthService);
    private sub!: Subscription;

    user: AppUser | null = null;

    ngOnInit() {
        this.sub = this.authService.currentUser$.subscribe(u => this.user = u);
    }

    ngOnDestroy() {
        this.sub.unsubscribe();
    }

    logout() {
        this.authService.logout();
    }
}