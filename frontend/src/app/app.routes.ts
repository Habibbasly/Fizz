import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/fizzbuzz/pages/fizzbuzz-page/fizzbuzz-page').then((m) => m.FizzBuzzPage),
  },
  { path: '**', redirectTo: '' },
];
