import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: '',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES)
  },
  {
    path: 'chat',
    loadComponent: () =>
      import('./pages/chat-page/chat-page.component').then((m) => m.ChatPageComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
