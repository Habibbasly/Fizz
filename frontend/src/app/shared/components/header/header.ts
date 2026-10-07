import { Component } from '@angular/core';

@Component({
  selector: 'app-header',
  template: `<header><h1>FizzBuzz</h1></header>`,
  styles: `
    header { background: #3949ab; color: #fff; padding: 0.75rem 1rem; }
    h1 { margin: 0; font-size: 1.4rem; }
  `,
})
export class Header {}
