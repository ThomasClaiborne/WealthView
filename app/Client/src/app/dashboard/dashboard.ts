import {
    Component,
    inject,
    OnInit,
    ChangeDetectorRef,
    ElementRef,
    ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { PortfolioService } from '../core/portfolio.service';
import { BankAccountService } from '../core/bank-account.service';
import { FundTransferService } from '../core/fund-transfer.service';
import { TradeService } from '../core/trade.service';
import { HoldingService } from '../core/holding.service';
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
export class Dashboard implements OnInit {
    private portfolioService   = inject(PortfolioService);
    private bankAccountService = inject(BankAccountService);
    private transferService    = inject(FundTransferService);
    private tradeService       = inject(TradeService);
    private holdingService     = inject(HoldingService);
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

    ngOnInit(): void {
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

    private buildPerformanceChart(): void {
        if (this.performanceChart) {
            this.performanceChart.destroy();
        }

        const ctx = this.performanceCanvas!.nativeElement.getContext('2d');
        if (!ctx) return;

        this.performanceChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: this.snapshots.map(s =>
                    new Date(s.snapshotDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
                ),
                datasets: [{
                    label: 'Portfolio Value',
                    data: this.snapshots.map(s => s.totalValue),
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.08)',
                    borderWidth: 2,
                    pointRadius: 3,
                    fill: true,
                    tension: 0.3,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        ticks: {
                            callback: (value) => '$' + Number(value).toLocaleString()
                        }
                    }
                }
            }
        });
    }

    private buildAllocationChart(): void {
        if (this.allocationChart) {
            this.allocationChart.destroy();
        }

        const ctx = this.allocationCanvas!.nativeElement.getContext('2d');
        if (!ctx || !this.portfolio) return;

        const groups: Record<string, number> = {};
        for (const h of this.holdings) {
            groups[h.assetClass] = (groups[h.assetClass] ?? 0) + h.marketValue;
        }

        if (this.portfolio.cashBalance > 0) {
            groups['Cash'] = this.portfolio.cashBalance;
        }

        const labels = Object.keys(groups);
        const values = Object.values(groups);

        const colorMap: Record<string, string> = {
            'Equity':      '#0d6efd',
            'ETF':         '#198754',
            'FixedIncome': '#fd7e14',
            'Cash':        '#adb5bd',
        };

        const colors = labels.map(l => colorMap[l] ?? '#6c757d');

        this.allocationChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
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
                            boxWidth: 12,
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