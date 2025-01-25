import { TestBed } from '@angular/core/testing';
import { OrderService } from './order.service';
import { CreateOrderRequest } from '../../models/create-order-request';
import { environment } from '../../../../environments/environment';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { StripeRedirectionResponse } from '../../models/stripe-redirection-response';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        OrderService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('checkout', () => {
    const mockRequest: CreateOrderRequest = {
      email: 'test@test.com',
      phoneNumber: '123456789',
      name: 'John Doe',
      shippingAddress: {
        line1: '123 Main St',
        line2: 'Apt 4B',
        city: 'Boston',
        state: 'MA',
        postalCode: '02108',
      },
      shoppingCartItems: [
        { productId: 1, quantity: 2 },
        { productId: 2, quantity: 1 },
      ],
    };
    it('should make a POST request to the correct URL with request body', () => {
      const mockResponse: StripeRedirectionResponse = {
        stripeUrl: 'https://stripe.com/checkout',
      };

      service
        .checkout(mockRequest)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      const req = httpMock.expectOne(
        environment.apiUrl + 'orders/checkout'
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });

    it('should handle errors appropriately', () => {
      const errorMessage = 'Checkout failed';

      service.checkout(mockRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        environment.apiUrl + 'orders/checkout'
      );
      req.flush(errorMessage, {
        status: 500,
        statusText: errorMessage,
      });
    });
  });
});
