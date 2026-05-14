import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from './sidebar';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [RouterOutlet, Sidebar],
    templateUrl: './main-layout.html'
})
export class MainLayout {}