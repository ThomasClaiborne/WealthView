import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SecurityService } from '../core/security.service';
import { TradeService } from '../core/trade.service';
import { PortfolioService } from '../core/portfolio.service';
import { Security, TradeResponse } from '../core/models';

@Component({
    selector: 'app-trading',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './trading.html'
})
export class Trading implements OnInit {
    private securityService  = inject(SecurityService);
    private tradeService     = inject(TradeService);
    private portfolioService = inject(PortfolioService);
    private cdr              = inject(ChangeDetectorRef);

    securities:     Security[] = [];
    cashBalance:    number = 0;
    selectedTicker: string | null = null;
    buyQuantity:    number = 1;
    loading         = false;
    errorMessage    = '';
    successMessage  = '';

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

        this.portfolioService.getPortfolio().subscribe({
            next: (data) => {
                this.cashBalance = data.cashBalance;
                this.cdr.markForCheck();
            },
            error: () => { }
        });
    }

    get selectedSecurity(): Security | null {
        return this.securities.find(s => s.ticker === this.selectedTicker) ?? null;
    }

    get estimatedCost(): number {
        if (!this.selectedSecurity?.lastPrice) return 0;
        return this.buyQuantity * this.selectedSecurity.lastPrice;
    }

    selectSecurity(ticker: string): void {
        this.selectedTicker = ticker;
        this.buyQuantity    = 1;
        this.successMessage = '';
        this.errorMessage   = '';
        this.cdr.markForCheck();
    }

    confirmBuy(): void {
        if (!this.selectedTicker || this.buyQuantity <= 0) return;

        this.loading        = true;
        this.errorMessage   = '';
        this.successMessage = '';

        this.tradeService.buy(this.selectedTicker, this.buyQuantity).subscribe({
            next: (res: TradeResponse) => {
                this.cashBalance    = res.newCashBalance;
                this.successMessage = `Bought ${this.buyQuantity} share(s) of ${this.selectedTicker} at $${res.pricePerShare.toFixed(2)}`;
                this.selectedTicker = null;
                this.loading        = false;
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Trade failed.';
                this.loading      = false;
                this.cdr.markForCheck();
            }
        });
    }
}