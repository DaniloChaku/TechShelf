import {
  HttpClient,
  HttpParams,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { CreateOrderRequest } from '../../models/create-order-request';
import { StripeRedirectionResponse } from '../../models/stripe-redirection-response';
import { Order } from '../../models/order';
import { PagedResult } from '../../models/pagedResult';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'orders/';

  checkout(request: CreateOrderRequest) {
    return this.http.post<StripeRedirectionResponse>(
      this.baseUrl + 'checkout',
      request
    );
  }

  myorders(pageIndex: number, pageSize: number) {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Order>>(
      this.baseUrl + 'myorders',
      {
        params,
      }
    );
  }
}
