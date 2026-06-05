import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './login.html'
})
export class Login {
    private auth    = inject(AuthService);
    private router  = inject(Router);
    private fb      = inject(FormBuilder);
    private cdr = inject(ChangeDetectorRef);

    form = this.fb.group({
        credential: ['', Validators.required],
        password:   ['', Validators.required]
    });

    error   = '';
    loading = false;

    submit() {
        if (this.form.invalid) return;
        this.loading = true;
        this.error   = '';

        this.auth.login(this.form.value).subscribe({
            next:  ()  => this.router.navigate(['/dashboard']),
            error: err => {
                this.loading = false;
                this.error   = err.error?.[0] ?? 'Login failed. Please try again.';
                this.cdr.markForCheck();
            }
        });
    }
}