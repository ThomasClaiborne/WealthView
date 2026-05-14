import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortfolioService } from '../core/portfolio.service';
import { PortfolioResponse } from '../core/models';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './dashboard.html'
})
export class Dashboard implements OnInit {
    private portfolioService = inject(PortfolioService);
    private cdr = inject(ChangeDetectorRef);

    portfolio: PortfolioResponse | null = null;
    errorMessage = '';

    ngOnInit(): void {
        this.portfolioService.getPortfolio().subscribe({
            next: (data) => {
                this.portfolio = data;
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to load portfolio.';
                this.cdr.markForCheck();
            }
        });
    }
}