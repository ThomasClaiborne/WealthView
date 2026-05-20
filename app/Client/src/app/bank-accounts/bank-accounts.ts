import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BankAccountService } from '../core/bank-account.service';
import { BankAccountResponse } from '../core/models';

@Component({
    selector: 'app-bank-accounts',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './bank-accounts.html'
})
export class BankAccounts implements OnInit {
    private bankAccountService = inject(BankAccountService);
    private cdr                = inject(ChangeDetectorRef);

    accounts:       BankAccountResponse[] = [];
    showAddForm     = false;

    // add form fields
    newBankName     = 'Chase';
    newNickname     = '';
    newBalance      = 0;

    // inline adjust state
    selectedId:     number | null = null;
    adjustMode:     'deposit' | 'withdraw' | null = null;
    adjustAmount    = 0;

    loading         = false;
    errorMessage    = '';
    successMessage  = '';

    readonly banks  = ['Chase', 'BankOfAmerica', 'Chime'];

    ngOnInit(): void {
        this.loadAccounts();
    }

    loadAccounts(): void {
        this.bankAccountService.getAll().subscribe({
            next: (data) => {
                this.accounts = data;
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to load bank accounts.';
                this.cdr.markForCheck();
            }
        });
    }

    bankColor(bankName: string): string {
        switch (bankName) {
            case 'Chase':         return '#8dbdf0';
            case 'BankOfAmerica': return '#f79ca3';
            case 'Chime':         return '#7df89a';
            default:              return '#e2e3e5';
        }
    }

    bankLabel(bankName: string): string {
        return bankName === 'BankOfAmerica' ? 'Bank of America' : bankName;
    }

    submitAdd(): void {
        this.loading       = true;
        this.errorMessage  = '';
        this.successMessage = '';

        this.bankAccountService.add(this.newBankName, this.newNickname || null, this.newBalance).subscribe({
            next: () => {
                this.showAddForm    = false;
                this.newBankName    = 'Chase';
                this.newNickname    = '';
                this.newBalance     = 0;
                this.loading        = false;
                this.successMessage = 'Account added successfully.';
                this.loadAccounts();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Failed to add account.';
                this.loading      = false;
                this.cdr.markForCheck();
            }
        });
    }

    openAdjust(id: number, mode: 'deposit' | 'withdraw'): void {
        this.selectedId   = id;
        this.adjustMode   = mode;
        this.adjustAmount = 0;
        this.errorMessage = '';
        this.successMessage = '';
        this.cdr.markForCheck();
    }

    submitAdjust(): void {
        if (!this.selectedId || !this.adjustMode) return;

        this.loading      = true;
        this.errorMessage = '';

        const call = this.adjustMode === 'deposit'
            ? this.bankAccountService.deposit(this.selectedId, this.adjustAmount)
            : this.bankAccountService.withdraw(this.selectedId, this.adjustAmount);

        call.subscribe({
            next: () => {
                this.selectedId     = null;
                this.adjustMode     = null;
                this.loading        = false;
                this.successMessage = `${this.adjustMode === 'deposit' ? 'Deposit' : 'Withdrawal'} successful.`;
                this.loadAccounts();
                this.cdr.markForCheck();
            },
            error: (err) => {
                this.errorMessage = err.error?.[0] ?? 'Transaction failed.';
                this.loading      = false;
                this.cdr.markForCheck();
            }
        });
    }

    deactivate(id: number): void {
        this.errorMessage   = '';
        this.successMessage = '';

        this.bankAccountService.deactivate(id).subscribe({
            next: () => {
                this.successMessage = 'Account removed.';
                this.loadAccounts();
                this.cdr.markForCheck();
            },
            error: () => {
                this.errorMessage = 'Failed to remove account.';
                this.cdr.markForCheck();
            }
        });
    }
}