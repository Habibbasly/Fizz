import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, TestRequest, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FizzBuzzPage } from './fizzbuzz-page';

// Intégration page + liste + service HTTP : seul le backend est simulé (HttpTestingController).
describe('FizzBuzzPage', () => {
  let fixture: ComponentFixture<FizzBuzzPage>;
  let httpMock: HttpTestingController;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FizzBuzzPage],
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(FizzBuzzPage);
    httpMock = TestBed.inject(HttpTestingController);
    element = fixture.nativeElement;
    await fixture.whenStable();
  });

  afterEach(() => httpMock.verify());

  const button = () => element.querySelector<HTMLButtonElement>('button[type=submit]')!;
  const items = () => Array.from(element.querySelectorAll('li .value')).map((li) => li.textContent?.trim());
  const errorMessage = () => element.querySelector('.error')?.textContent?.trim();

  function setInput(name: string, value: string): void {
    const input = element.querySelector<HTMLInputElement>(`input[name=${name}]`)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<TestRequest> {
    button().click();
    await fixture.whenStable();
    return httpMock.expectOne((r) => r.url === '/api/fizzbuzz');
  }

  it('envoie les valeurs du formulaire et affiche la séquence reçue', async () => {
    setInput('int1', '2');
    setInput('str1', 'Foo');
    setInput('limit', '4');

    const req = await submit();
    expect(req.request.params.get('int1')).toBe('2');
    expect(req.request.params.get('str1')).toBe('Foo');
    expect(req.request.params.get('int2')).toBe('5');
    expect(req.request.params.get('limit')).toBe('4');

    req.flush(['1', 'Foo', '3', 'Foo']);
    await fixture.whenStable();

    expect(items()).toEqual(['1', 'Foo', '3', 'Foo']);
    expect(element.querySelectorAll('li.word').length).toBe(2);
    expect(errorMessage()).toBeUndefined();
  });

  it('désactive le bouton pendant le chargement', async () => {
    const req = await submit();

    expect(button().disabled).toBeTrue();
    expect(button().textContent).toContain('Chargement');

    req.flush([]);
    await fixture.whenStable();

    expect(button().disabled).toBeFalse();
    expect(button().textContent).toContain('Générer');
  });

  it('affiche le détail du ProblemDetails renvoyé par l’API en cas de 400', async () => {
    const req = await submit();
    req.flush(
      { title: 'Bad Request', status: 400, detail: 'La limite ne peut pas dépasser 10000.' },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    expect(errorMessage()).toBe('La limite ne peut pas dépasser 10000.');
    expect(items()).toEqual([]);
  });

  it('affiche les erreurs de validation par paramètre', async () => {
    const req = await submit();
    req.flush(
      {
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: { int1: ['Le diviseur doit être strictement positif.'], limit: ['La limite ne peut pas dépasser 10000.'] },
      },
      { status: 400, statusText: 'Bad Request' },
    );
    await fixture.whenStable();

    expect(errorMessage()).toBe('int1 : Le diviseur doit être strictement positif. limit : La limite ne peut pas dépasser 10000.');
    expect(items()).toEqual([]);
  });

  it('vide la séquence précédente quand une erreur survient', async () => {
    (await submit()).flush(['1', '2']);
    await fixture.whenStable();
    expect(items().length).toBe(2);

    (await submit()).flush({ title: 'Bad Request' }, { status: 400, statusText: 'Bad Request' });
    await fixture.whenStable();

    expect(errorMessage()).toBe('Bad Request');
    expect(items()).toEqual([]);
  });

  it('affiche un message générique si le serveur est injoignable', async () => {
    const req = await submit();
    req.error(new ProgressEvent('error'));
    await fixture.whenStable();

    expect(errorMessage()).toBe('Impossible de contacter le serveur.');
  });
});
