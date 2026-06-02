import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/auth-page/auth-page.component').then((m) => m.AuthPageComponent)
  },
  {
    path: 'auth',
    loadComponent: () =>
      import('./pages/auth-redirect-page/auth-redirect-page.component').then(
        (m) => m.AuthRedirectPageComponent
      )
  }
];

