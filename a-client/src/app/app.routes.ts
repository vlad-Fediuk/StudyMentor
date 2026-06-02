import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/chat-page/chat-page.component').then(
        (m) => m.ChatPageComponent
      )
  }
];
