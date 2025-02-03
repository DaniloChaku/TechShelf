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
import { Order } from '../../models/order';
import { PagedResult } from '../../models/pagedResult';

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

  describe('myorders', () => {
    const pageIndex = 1;
    const pageSize = 1;
    const expectedUrl = `${environment.apiUrl}orders/myorders?pageIndex=${pageIndex}&pageSize=${pageSize}`;

    it('should make a GET request to the correct URL and return a PagedResult of Orders', () => {
      const mockPagedResult: PagedResult<Order> = {
        items: [
          {
            id: '1',
            email: 'test@test.com',
            phoneNumber: '123456789',
            fullName: 'John Doe',
            total: 100,
            address: {
              line1: '123 Main St',
              line2: 'Apt 4B',
              city: 'Boston',
              state: 'MA',
              postalCode: '02108',
            },
            orderItems: [
              {
                price: 50,
                quantity: 1,
                productOrdered: {
                  productId: 1,
                  name: 'Product 1',
                  imageUrl:
                    'http://example.com/product1.jpg',
                },
              },
              {
                price: 50,
                quantity: 1,
                productOrdered: {
                  productId: 2,
                  name: 'Product 2',
                  imageUrl:
                    'http://example.com/product2.jpg',
                },
              },
            ],
            history: [
              {
                orderId: '1',
                status: 'Ordered',
                date: new Date(),
              },
            ],
          },
        ],
        totalCount: 1,
        pageIndex: 1,
        pageSize: 1,
      };

      service.myorders(1, 1).subscribe((pagedResult) => {
        expect(pagedResult).toEqual(mockPagedResult);
      });

      const req = httpMock.expectOne(expectedUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });

    it('should handle errors appropriately', () => {
      const errorMessage = 'Failed to fetch orders';

      service.myorders(1, 1).subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(expectedUrl);
      req.flush(errorMessage, {
        status: 500,
        statusText: errorMessage,
      });
    });
  });
});
