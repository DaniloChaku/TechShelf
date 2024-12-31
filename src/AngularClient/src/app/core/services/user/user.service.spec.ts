import { TestBed } from '@angular/core/testing';

import { UserService } from './user.service';
import { environment } from '../../../../environments/environment';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { RegisterCustomerRequest } from '../../models/register-customer-request';
import { TokenResponse } from '../../models/token-response';
import { LoginRequest } from '../../models/login-request';
import { UserDto } from '../../models/user-dto';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('register', () => {
    const mockRequest: RegisterCustomerRequest = {
      firstName: 'name',
      lastName: 'last name',
      phoneNumber: '+123456789',
      email: 'test@example.com',
      password: 'password123',
    };
    it('should make a POST request to the correct URL with provided data', () => {
      const mockResponse: TokenResponse = {
        token: 'mock-token',
      };

      service
        .register(mockRequest)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/register`
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });

    it('should handle registration errors appropriately', () => {
      const errorMessage = 'Failed to add user.';

      service.register(mockRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/register`
      );
      req.flush(errorMessage, {
        status: 400,
        statusText: errorMessage,
      });
    });
  });

  describe('login', () => {
    const mockRequest: LoginRequest = {
      email: 'test@example.com',
      password: 'password123',
    };
    it('should make a POST request to the correct URL with credentials', () => {
      const mockResponse: TokenResponse = {
        token: 'mock-token',
      };

      service.login(mockRequest).subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/login`
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });

    it('should handle login errors appropriately', () => {
      const errorMessage = 'Invalid credentials';

      service.login(mockRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/login`
      );
      req.flush(errorMessage, {
        status: 401,
        statusText: errorMessage,
      });
    });
  });

  describe('refreshToken', () => {
    it('should make a POST request to refresh token with credentials flag', () => {
      const mockResponse: TokenResponse = {
        token: 'new-mock-token',
      };

      service.refreshToken().subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/refresh-token`
      );
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBeTrue();
      req.flush(mockResponse);
    });

    it('should handle refresh token errors appropriately', () => {
      const errorMessage = 'Invalid refresh token';

      service.refreshToken().subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/refresh-token`
      );
      req.flush(errorMessage, {
        status: 401,
        statusText: errorMessage,
      });
    });
  });

  describe('getCurrentUser', () => {
    it('should make a GET request to fetch current user', () => {
      const mockUser: UserDto = {
        firstName: 'first name',
        lastName: 'last name',
        phoneNumber: '+1234567890',
        email: 'test@example.com',
        roles: ['customer'],
      };

      service.getCurrentUser().subscribe((user) => {
        expect(user).toEqual(mockUser);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/me`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });

    it('should handle getCurrentUser errors appropriately', () => {
      const errorMessage = 'Unauthorized';

      service.getCurrentUser().subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/me`
      );
      req.flush(errorMessage, {
        status: 401,
        statusText: errorMessage,
      });
    });
  });
});
