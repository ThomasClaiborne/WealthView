import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TradeService } from '../core/trade.service';
import { Trade } from '../core/models';

@Component({
    selector: 'app-trade-log',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './trade-log.html'
})
export class TradeLog implements OnInit {
    private tradeService = inject(TradeService);
    private cdr          = inject(ChangeDetectorRef);

    trades:      Trade[] = [];
    errorMessage = '';

    ngOnInit(): void {
        this.tradeService.getAll().subscribe({
            next: (data) => {
                this.trades = data;
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to load trade log.';
                this.cdr.markForCheck();
            }
        });
    }
}