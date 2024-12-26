import {
  HttpClient,
  HttpParams,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { SearchProductsRequest } from './search-products-request';
import { PagedResult } from '../../models/pagedResult';
import { Product } from '../../models/product';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getProducts(request: SearchProductsRequest) {
    let params = new HttpParams();

    params = params.append(
      'pageIndex',
      request.pageIndex.toString()
    );
    params = params.append(
      'pageSize',
      request.pageSize.toString()
    );

    if (request.brandId) {
      params = params.append(
        'brandId',
        request.brandId.toString()
      );
    }

    if (request.categoryId) {
      params = params.append(
        'categoryId',
        request.categoryId.toString()
      );
    }

    if (request.name) {
      params = params.append('name', request.name);
    }

    if (request.minPrice) {
      params = params.append(
        'minPrice',
        request.minPrice.toString()
      );
    }

    if (request.maxPrice) {
      params = params.append(
        'maxPrice',
        request.maxPrice.toString()
      );
    }

    if (request.sortBy) {
      params = params.append('sortBy', request.sortBy);
    }

    if (request.isDescending) {
      params = params.append(
        'isDescending',
        request.isDescending.toString()
      );
    }

    return this.http.get<PagedResult<Product>>(
      this.baseUrl + 'products/search',
      {
        params,
      }
    );
  }

  getProductById(id: number) {
    return this.http.get<Product>(
      this.baseUrl + 'products/' + id
    );
  }
}
