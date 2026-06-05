import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HoldingService } from '../core/holding.service';
import { TradeService } from '../core/trade.service';
import { HoldingResponse, TradeResponse } from '../core/models';

@Component({
    selector: 'app-holdings',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './holdings.html'
})
export class Holdings implements OnInit {
    private holdingService = inject(HoldingService);
    private tradeService   = inject(TradeService);
    private cdr            = inject(ChangeDetectorRef);

    holdings:        HoldingResponse[] = [];
    selectedHolding: HoldingResponse | null = null;
    sellQuantity:    number = 1;
    loading          = false;
    errorMessage     = '';
    successMessage   = '';

    ngOnInit(): void {
        this.loadHoldings();
    }

    loadHoldings(): void {
        this.holdingService.getAll().subscribe({
            next: (data) => {
                this.holdings = data;
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to load holdings.';
                this.cdr.markForCheck();
            }
        });
    }

    get estimatedProceeds(): number {
        if (!this.selectedHolding) return 0;
        return this.sellQuantity * this.selectedHolding.currentPrice;
    }

    selectHolding(holding: HoldingResponse): void {
        this.selectedHolding = holding;
        this.sellQuantity    = 1;
        this.successMessage  = '';
        this.errorMessage    = '';
        this.cdr.markForCheck();
    }

    confirmSell(): void {
        if (!this.selectedHolding || this.sellQuantity <= 0) return;

        this.loading        = true;
        this.errorMessage   = '';
        this.successMessage = '';

        this.tradeService.sell(this.selectedHolding.ticker, this.sellQuantity).subscribe({
            next: (res: TradeResponse) => {
                this.successMessage  = `Sold ${this.sellQuantity} share(s) of ${res.ticker} at $${res.pricePerShare.toFixed(2)}`;
                this.selectedHolding = null;
                this.loading         = false;
                this.loadHoldings();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Sell failed.';
                this.loading      = false;
                this.cdr.markForCheck();
            }
        });
    }
}