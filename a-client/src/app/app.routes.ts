import { Routes } from '@angular/router';

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
    loadComponent: () =>
      import('./pages/chat-page/chat-page.component').then(
        (m) => m.ChatPageComponent
      )
  }
];
