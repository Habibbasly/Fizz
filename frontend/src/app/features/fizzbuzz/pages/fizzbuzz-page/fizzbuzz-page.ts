import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FizzBuzzParams } from '../../../../core/models/fizzbuzz.model';
import { FizzBuzzService } from '../../../../core/services/fizzbuzz.service';
import { FizzBuzzList } from '../../components/fizzbuzz-list/fizzbuzz-list';

@Component({
  selector: 'app-fizzbuzz-page',
  imports: [FormsModule, FizzBuzzList],
  templateUrl: './fizzbuzz-page.html',
  styleUrl: './fizzbuzz-page.css',
})
export class FizzBuzzPage {
  private readonly fizzBuzzService = inject(FizzBuzzService);

  readonly params: FizzBuzzParams = { int1: 3, int2: 5, limit: 15, str1: 'Fizz', str2: 'Buzz' };

  readonly items = signal<string[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  generate(): void {
    this.loading.set(true);
    this.error.set(null);

    this.fizzBuzzService.generate(this.params).subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(errorMessage(err?.error));
        this.items.set([]);
        this.loading.set(false);
      },
    });
  }
}

/**
 * Extrait un message lisible d'une réponse d'erreur de l'API (ProblemDetails / ValidationProblemDetails).
 * Les erreurs de validation sont listées par paramètre : "limit : La limite ne peut pas dépasser 10000."
 */
function errorMessage(problem: { errors?: Record<string, string[]>; detail?: string; title?: string } | null | undefined): string {
  if (problem?.errors) {
    const messages = Object.entries(problem.errors).flatMap(([field, list]) => list.map((m) => `${field} : ${m}`));
    if (messages.length > 0) return messages.join(' ');
  }
  return problem?.detail ?? problem?.title ?? 'Impossible de contacter le serveur.';
}
