import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './register.html'
})
export class Register {
    private auth   = inject(AuthService);
    private router = inject(Router);
    private fb     = inject(FormBuilder);

    form = this.fb.group({
        firstName: ['', [Validators.required, Validators.maxLength(50)]],
        lastName:  ['', [Validators.required, Validators.maxLength(50)]],
        username:  ['', [Validators.required, Validators.minLength(3),
                         Validators.maxLength(30),
                         Validators.pattern('^[a-zA-Z0-9_]+$')]],
        email:     ['', [Validators.required, Validators.email]],
        password:  ['', [Validators.required, Validators.minLength(8)]]
    });

    errors:  string[] = [];
    loading = false;

    f(name: string) { return this.form.get(name); }

    submit() {
        if (this.form.invalid) return;
        this.loading = true;
        this.errors  = [];

        this.auth.register(this.form.value).subscribe({
            next:  ()  => this.router.navigate(['/dashboard']),
            error: err => {
                this.loading = false;
                if (Array.isArray(err.error))
                    this.errors = err.error;
                else if (err.error?.errors)
                    this.errors = (Object.values(err.error.errors) as string[][]).flat();
                else
                    this.errors = ['Registration failed. Please try again.'];
            }
        });
    }
}