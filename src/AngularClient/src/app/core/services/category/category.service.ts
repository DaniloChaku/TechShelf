import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Category } from '../../models/category';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  baseUrl = environment.apiUrl;
  http = inject(HttpClient);

  getCategories() {
    return this.http.get<Category[]>(
      this.baseUrl + 'categories'
    );
  }
}
