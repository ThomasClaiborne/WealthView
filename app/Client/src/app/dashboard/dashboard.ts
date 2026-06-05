import {
    Component,
    inject,
    OnInit,
    OnDestroy,
    ChangeDetectorRef,
    ElementRef,
    ViewChild,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { PortfolioService } from '../core/portfolio.service';
import { BankAccountService } from '../core/bank-account.service';
import { FundTransferService } from '../core/fund-transfer.service';
import { TradeService } from '../core/trade.service';
import { HoldingService } from '../core/holding.service';
import { ThemeService } from '../core/theme.service';
import {
    PortfolioResponse,
    BankAccountResponse,
    FundTransferResponse,
    Trade,
    HoldingResponse,
    SnapshotResponse,
} from '../core/models';

Chart.register(...registerables);

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit, OnDestroy {
    private portfolioService   = inject(PortfolioService);
    private bankAccountService = inject(BankAccountService);
    private transferService    = inject(FundTransferService);
    private tradeService       = inject(TradeService);
    private holdingService     = inject(HoldingService);
    private themeService       = inject(ThemeService);
    private cdr                = inject(ChangeDetectorRef);

    @ViewChild('performanceCanvas') performanceCanvas?: ElementRef<HTMLCanvasElement>;
    @ViewChild('allocationCanvas')  allocationCanvas?:  ElementRef<HTMLCanvasElement>;

    portfolio:        PortfolioResponse | null = null;
    bankAccounts:     BankAccountResponse[]    = [];
    pendingTransfers: FundTransferResponse[]   = [];
    recentTrades:     Trade[]                  = [];
    holdings:         HoldingResponse[]        = [];
    snapshots:        SnapshotResponse[]       = [];
    errorMessage      = '';
    portfolioReady    = false;

    private performanceChart: Chart | null = null;
    private allocationChart:  Chart | null = null;
    private themeSub!: Subscription;

    ngOnDestroy(): void {
        this.themeSub?.unsubscribe();
    }

    ngOnInit(): void {
        this.themeSub = this.themeService.isDark$.subscribe(() => {
            this.tryBuildCharts();
        });

        this.loadPortfolioWithRetry();

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

        this.holdingService.getAll().subscribe({
            next: (data) => {
                this.holdings = data;
                this.tryBuildCharts();
                this.cdr.markForCheck();
            },
            error: () => { }
        });
    }

    private loadPortfolioWithRetry(attempts: number = 0): void {
        this.portfolioService.getPortfolio().subscribe({
            next: (data) => {
                this.portfolio      = data;
                this.portfolioReady = true;
                this.cdr.markForCheck();

                // Load snapshots AFTER portfolio succeeds — getPortfolio creates today's
                // snapshot as a side effect, so we must wait for it to finish first
                this.portfolioService.getSnapshotHistory().subscribe({
                    next: (snapshots) => {
                        this.snapshots = snapshots;
                        this.tryBuildCharts();
                        this.cdr.markForCheck();
                    },
                    error: () => { }
                });

                setTimeout(() => {
                    this.tryBuildCharts();
                    this.cdr.markForCheck();
                }, 50);
            },
            error: () => {
                if (attempts < 3) {
                    setTimeout(() => this.loadPortfolioWithRetry(attempts + 1), 800);
                } else {
                    this.errorMessage = 'Failed to load portfolio.';
                    this.cdr.markForCheck();
                }
            }
        });
    }

    private tryBuildCharts(): void {
        const perfCanvas  = this.performanceCanvas?.nativeElement;
        const allocCanvas = this.allocationCanvas?.nativeElement;
        if (!perfCanvas || !allocCanvas || !this.portfolio) return;

        this.buildPerformanceChart();
        this.buildAllocationChart();
    }

    private get isDark(): boolean {
        return this.themeService.isDark;
    }

    private get chartColors() {
        const dark = this.isDark;
        return {
            tickColor:  dark ? 'rgba(241,245,249,0.55)' : '#334155',
            gridColor:  dark ? 'rgba(51,65,85,0.6)'     : 'rgba(226,232,240,0.8)',
            labelColor: dark ? 'rgba(241,245,249,0.8)'  : '#334155',
            cardBg:     dark ? '#1E293B'                 : '#FFFFFF',
        };
    }

    private buildPerformanceChart(): void {
        if (this.performanceChart) this.performanceChart.destroy();

        const ctx = this.performanceCanvas!.nativeElement.getContext('2d');
        if (!ctx) return;

        const { tickColor, gridColor } = this.chartColors;

        this.performanceChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: this.snapshots.map(s =>
                    new Date(s.snapshotDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
                ),
                datasets: [{
                    label: 'Portfolio Value',
                    data: this.snapshots.map(s => s.totalValue),
                    borderColor: '#6366F1',
                    backgroundColor: 'rgba(99,102,241,0.08)',
                    borderWidth: 2,
                    pointRadius: 3,
                    pointBackgroundColor: '#6366F1',
                    fill: true,
                    tension: 0.3,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: {
                        ticks: { color: tickColor, font: { size: 11 } },
                        grid:  { color: gridColor },
                        border: { color: gridColor },
                    },
                    y: {
                        ticks: {
                            color: tickColor,
                            font: { size: 11 },
                            callback: (value) => '$' + Number(value).toLocaleString()
                        },
                        grid:  { color: gridColor },
                        border: { color: gridColor },
                    }
                }
            }
        });
    }

    private buildAllocationChart(): void {
        if (this.allocationChart) this.allocationChart.destroy();

        const ctx = this.allocationCanvas!.nativeElement.getContext('2d');
        if (!ctx || !this.portfolio) return;

        const { labelColor, cardBg } = this.chartColors;

        const groups: Record<string, number> = {};
        for (const h of this.holdings) {
            groups[h.assetClass] = (groups[h.assetClass] ?? 0) + h.marketValue;
        }
        if (this.portfolio.cashBalance > 0) groups['Cash'] = this.portfolio.cashBalance;

        const labels = Object.keys(groups);
        const values = Object.values(groups);

        const colorMap: Record<string, string> = {
            'Equity':      '#6366F1',
            'ETF':         '#10B981',
            'FixedIncome': '#F59E0B',
            'Cash':        '#94A3B8',
        };
        const colors = labels.map(l => colorMap[l] ?? '#64748B');

        this.allocationChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderColor: cardBg,
                    borderWidth: 2,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            color: labelColor,
                            boxWidth: 12,
                            font: { size: 12 },
                            generateLabels: (chart) => {
                                const data  = chart.data;
                                const total = (data.datasets[0].data as number[]).reduce((a, b) => a + b, 0);
                                return (data.labels as string[]).map((label, i) => {
                                    const value = (data.datasets[0].data as number[])[i];
                                    const pct   = ((value / total) * 100).toFixed(1);
                                    return {
                                        text:        `${label}  ${pct}%`,
                                        fillStyle:   (data.datasets[0].backgroundColor as string[])[i],
                                        strokeStyle: (data.datasets[0].backgroundColor as string[])[i],
                                        fontColor:   labelColor,
                                        lineWidth:   0,
                                        index:       i,
                                    };
                                });
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const val   = ctx.parsed as number;
                                const total = (ctx.dataset.data as number[]).reduce((a, b) => a + b, 0);
                                const pct   = ((val / total) * 100).toFixed(1);
                                return ` $${val.toLocaleString('en-US', { minimumFractionDigits: 2 })} (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    bankLabel(bankName: string): string {
        return bankName === 'BankOfAmerica' ? 'Bank of America' : bankName;
    }
}