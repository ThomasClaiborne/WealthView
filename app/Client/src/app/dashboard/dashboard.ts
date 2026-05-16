import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PortfolioService } from '../core/portfolio.service';
import { BankAccountService } from '../core/bank-account.service';
import { FundTransferService } from '../core/fund-transfer.service';
import { TradeService } from '../core/trade.service';
import { PortfolioResponse, BankAccountResponse, FundTransferResponse, Trade } from '../core/models';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './dashboard.html'
})
export class Dashboard implements OnInit {
    private portfolioService    = inject(PortfolioService);
    private bankAccountService  = inject(BankAccountService);
    private transferService     = inject(FundTransferService);
    private tradeService        = inject(TradeService);
    private cdr                 = inject(ChangeDetectorRef);

    portfolio:        PortfolioResponse | null = null;
    bankAccounts:     BankAccountResponse[]    = [];
    pendingTransfers: FundTransferResponse[]   = [];
    recentTrades:     Trade[]                  = [];
    errorMessage      = '';

    ngOnInit(): void {
        this.portfolioService.getPortfolio().subscribe({
            next: (data) => { this.portfolio = data; this.cdr.markForCheck(); },
            error: () => { this.errorMessage = 'Failed to load portfolio.'; this.cdr.markForCheck(); }
        });

        this.bankAccountService.getAll().subscribe({
            next: (data) => { this.bankAccounts = data; this.cdr.markForCheck(); },
            error: () => { }
        });

        this.transferService.getPending().subscribe({
            next: (data) => { this.pendingTransfers = data; this.cdr.markForCheck(); },
            error: () => { }
        });

        this.tradeService.getAll().subscribe({
            next: (data) => { this.recentTrades = data.slice(0, 3); this.cdr.markForCheck(); },
            error: () => { }
        });
    }

    bankLabel(bankName: string): string {
        return bankName === 'BankOfAmerica' ? 'Bank of America' : bankName;
    }

    tradeLabel(t: Trade): string {
        return `${t.tradeType.toUpperCase()} — ${t.ticker} — ${t.quantity} @ $${t.pricePerShare.toFixed(2)}`;
    }
}