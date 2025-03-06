import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Brand } from '../../models/catalog/brand';

@Injectable({
  providedIn: 'root',
})
export class BrandService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getBrands() {
    return this.http.get<Brand[]>(this.baseUrl + 'brands');
  }
}
