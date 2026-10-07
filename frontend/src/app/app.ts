import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/components/header/header';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  template: `
    <app-header />
    <main class="container">
      <router-outlet />
    </main>
  `,
  styles: `.container { max-width: 960px; margin: 0 auto; padding: 1.5rem 1rem; }`,
})
export class App {}
