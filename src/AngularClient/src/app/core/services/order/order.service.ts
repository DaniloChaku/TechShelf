import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { CreateOrderRequest } from '../../models/create-order-request';
import { StripeRedirectionResponse } from '../../models/stripe-redirection-response';

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
}
