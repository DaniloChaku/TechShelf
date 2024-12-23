import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  http = inject(HttpClient);

  getCategories() {
    return this.http.get(
      'https://localhost:7281/api/categories'
    );
  }
}
