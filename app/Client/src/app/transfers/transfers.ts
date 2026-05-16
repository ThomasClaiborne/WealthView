import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FundTransferService } from '../core/fund-transfer.service';
import { BankAccountService } from '../core/bank-account.service';
import { BankAccountResponse, FundTransferResponse } from '../core/models';

@Component({
    selector: 'app-transfers',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './transfers.html'
})
export class Transfers implements OnInit {
    private transferService    = inject(FundTransferService);
    private bankAccountService = inject(BankAccountService);
    private cdr                = inject(ChangeDetectorRef);

    accounts:  BankAccountResponse[]    = [];
    pending:   FundTransferResponse[]   = [];
    history:   FundTransferResponse[]   = [];
    activeTab: 'pending' | 'history'    = 'pending';

    // form fields
    selectedBankAccountId: number | null = null;
    direction:             'Deposit' | 'Withdrawal' = 'Deposit';
    amount                = 0;

    loading        = false;
    errorMessage   = '';
    successMessage = '';

    ngOnInit(): void {
        this.loadAll();
    }

    loadAll(): void {
        this.bankAccountService.getAll().subscribe({
            next: (data) => {
                this.accounts = data;
                if (data.length > 0) this.selectedBankAccountId = data[0].bankAccountId;
                this.cdr.markForCheck();
            },
            error: () => { }
        });

        this.transferService.getPending().subscribe({
            next: (data) => { this.pending = data; this.cdr.markForCheck(); },
            error: () => { }
        });

        this.transferService.getHistory().subscribe({
            next: (data) => { this.history = data; this.cdr.markForCheck(); },
            error: () => { }
        });
    }

    get selectedAccount(): BankAccountResponse | null {
        return this.accounts.find(a => a.bankAccountId === this.selectedBankAccountId) ?? null;
    }

    submitRequest(): void {
        if (!this.selectedBankAccountId || this.amount <= 0) return;

        this.loading        = true;
        this.errorMessage   = '';
        this.successMessage = '';

        const call = this.direction === 'Deposit'
            ? this.transferService.requestDeposit(this.selectedBankAccountId, this.amount)
            : this.transferService.requestWithdrawal(this.selectedBankAccountId, this.amount);

        call.subscribe({
            next: () => {
                this.amount         = 0;
                this.loading        = false;
                this.successMessage = 'Transfer request submitted.';
                this.activeTab      = 'pending';
                this.loadAll();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Request failed.';
                this.loading      = false;
                this.cdr.markForCheck();
            }
        });
    }

    approve(id: number): void {
        this.errorMessage   = '';
        this.successMessage = '';

        this.transferService.approve(id).subscribe({
            next: () => {
                this.successMessage = 'Transfer approved.';
                this.loadAll();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Approval failed.';
                this.cdr.markForCheck();
            }
        });
    }

    reject(id: number): void {
        this.errorMessage   = '';
        this.successMessage = '';

        this.transferService.reject(id).subscribe({
            next: () => {
                this.successMessage = 'Transfer rejected.';
                this.loadAll();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Rejection failed.';
                this.cdr.markForCheck();
            }
        });
    }

    directionBadgeClass(direction: string): string {
        return direction === 'Deposit' ? 'bg-success' : 'bg-danger';
    }

    statusBadgeClass(status: string): string {
        switch (status) {
            case 'Pending':  return 'bg-warning text-dark';
            case 'Approved': return 'bg-success';
            case 'Rejected': return 'bg-danger';
            default:         return 'bg-secondary';
        }
    }
}