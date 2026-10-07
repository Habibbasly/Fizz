import { Component, input } from '@angular/core';

@Component({
  selector: 'app-fizzbuzz-list',
  template: `
    <ul>
      @for (value of items(); track $index) {
        <li [class.word]="value !== ($index + 1).toString()">
          <span class="number">{{ $index + 1 }}</span>
          <span class="value">{{ value }}</span>
        </li>
      }
    </ul>
  `,
  styles: `
    ul { list-style: none; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: 0.5rem; }
    li { background: #fff; border-radius: 6px; padding: 0.5rem; display: flex; flex-direction: column; align-items: center; box-shadow: 0 1px 2px rgba(0,0,0,.08); overflow-wrap: anywhere; }
    .number { font-size: 0.75rem; color: #777; }
    .value { font-weight: 600; }
    .word { background: #e8eaf6; }
  `,
})
export class FizzBuzzList {
  readonly items = input.required<string[]>();
}
