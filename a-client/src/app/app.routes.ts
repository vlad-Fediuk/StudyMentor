import { Routes } from '@angular/router';
import { authGuard } from './features/auth/data-access/auth.guard';

export const routes: Routes = [
  {
    path: 'ai-disclaimer',
    loadComponent: () =>
      import('./pages/ai-disclaimer-page/ai-disclaimer-page.component').then(
        (m) => m.AiDisclaimerPageComponent
      )
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'chat'
  },
  {
    path: '',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES)
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/chat-page/chat-page.component').then((m) => m.ChatPageComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
