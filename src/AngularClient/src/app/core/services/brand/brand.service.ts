import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BrandService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getBrands() {
    return this.http.get(this.baseUrl + 'brands');
  }
}
