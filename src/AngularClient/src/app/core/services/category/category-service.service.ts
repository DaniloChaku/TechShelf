import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CategoryServiceService {
  http = inject(HttpClient);

  getCategories() {
    return this.http.get(
      'https://localhost:7281/api/categories'
    );
  }
}
