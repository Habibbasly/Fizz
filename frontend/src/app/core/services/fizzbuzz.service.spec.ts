import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FizzBuzzService } from './fizzbuzz.service';

describe('FizzBuzzService', () => {
  let service: FizzBuzzService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(FizzBuzzService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('appelle GET /api/fizzbuzz avec les 5 paramètres en query string', () => {
    let result: string[] | undefined;
    service.generate({ int1: 3, int2: 5, limit: 15, str1: 'Fizz', str2: 'Buzz' }).subscribe((r) => (result = r));

    const req = httpMock.expectOne((r) => r.url === '/api/fizzbuzz');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('int1')).toBe('3');
    expect(req.request.params.get('int2')).toBe('5');
    expect(req.request.params.get('limit')).toBe('15');
    expect(req.request.params.get('str1')).toBe('Fizz');
    expect(req.request.params.get('str2')).toBe('Buzz');

    req.flush(['1', '2', 'Fizz']);
    expect(result).toEqual(['1', '2', 'Fizz']);
  });

  it('encode les caractères spéciaux des chaînes', () => {
    service.generate({ int1: 3, int2: 5, limit: 15, str1: 'A&B', str2: 'été' }).subscribe();

    const req = httpMock.expectOne((r) => r.url === '/api/fizzbuzz');
    expect(req.request.urlWithParams).toContain('str1=A%26B');
    expect(req.request.params.get('str2')).toBe('été');
    req.flush([]);
  });
});
