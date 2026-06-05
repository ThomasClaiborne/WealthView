import { Routes } from '@angular/router';
import { authGuard }       from './core/auth.guard';
import { publicOnlyGuard } from './core/public-only.guard';
import { AuthLayout }      from './layout/auth-layout';
import { MainLayout }      from './layout/main-layout';
import { Landing }         from './auth/landing';
import { Login }           from './auth/login';
import { Register }        from './auth/register';
import { Dashboard }       from './dashboard/dashboard';
import { Trading }         from './trading/trading';
import { Holdings }        from './holdings/holdings';
import { TradeLog }        from './trade-log/trade-log';
import { BankAccounts }    from './bank-accounts/bank-accounts';
import { Transfers }       from './transfers/transfers';

export const routes: Routes = [
    {
        path: '',
        component: AuthLayout,
        children: [
            { path: '',         component: Landing,   canActivate: [publicOnlyGuard] },
            { path: 'login',    component: Login,     canActivate: [publicOnlyGuard] },
            { path: 'register', component: Register,  canActivate: [publicOnlyGuard] }
        ]
    },
    {
        path: '',
        component: MainLayout,
        canActivate: [authGuard],
        children: [
            { path: 'dashboard',     component: Dashboard     },
            { path: 'trading',       component: Trading       },
            { path: 'holdings',      component: Holdings      },
            { path: 'trade-log',     component: TradeLog      },
            { path: 'bank-accounts', component: BankAccounts  },
            { path: 'transfers',     component: Transfers     }
        ]
    },
    { path: '**', redirectTo: '' }
];