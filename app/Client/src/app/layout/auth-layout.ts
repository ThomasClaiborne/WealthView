import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
    selector: 'app-auth-layout',
    standalone: true,
    imports: [RouterOutlet, RouterLink],
    templateUrl: './auth-layout.html'
})
export class AuthLayout {}