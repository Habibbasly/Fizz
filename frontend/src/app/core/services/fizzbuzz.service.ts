import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FizzBuzzParams } from '../models/fizzbuzz.model';

@Injectable({ providedIn: 'root' })
export class FizzBuzzService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/fizzbuzz`;

  generate(params: FizzBuzzParams): Observable<string[]> {
    const httpParams = new HttpParams({ fromObject: { ...params } });
    return this.http.get<string[]>(this.baseUrl, { params: httpParams });
  }
}
