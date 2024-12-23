import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BrandService {
  http = inject(HttpClient);

  getBrands() {
    return this.http.get(
      'https://localhost:7281/api/brands'
    );
  }
}
