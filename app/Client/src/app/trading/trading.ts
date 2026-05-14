import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SecurityService } from '../core/security.service';
import { Security } from '../core/models';

@Component({
    selector: 'app-trading',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './trading.html'
})
export class Trading implements OnInit {
    private securityService = inject(SecurityService);
    private cdr = inject(ChangeDetectorRef);

    securities: Security[] = [];
    errorMessage = '';

    ngOnInit(): void {
        this.securityService.getAll().subscribe({
            next: (data) => {
                this.securities = data;
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to load securities.';
                this.cdr.markForCheck();
            }
        });
    }
}