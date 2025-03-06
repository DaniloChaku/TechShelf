import { TestBed } from '@angular/core/testing';

import { UserService } from './user.service';
import { environment } from '../../../../environments/environment';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { RegisterCustomerRequest } from '../../models/account/register-customer-request';
import { TokenResponse } from '../../models/account/token-response';
import { LoginRequest } from '../../models/account/login-request';
import { User } from '../../models/account/user';

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
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('isAuthenticated', () => {
    it('should return true when currentUser is set', () => {
      service.currentUser.set({
        id: '1',
        firstName: 'Test',
        lastName: 'User',
        phoneNumber: '+1234567890',
        email: 'test@example.com',
        roles: ['customer'],
      });

      expect(service.isAuthenticated()).toBeTrue();
    });

    it('should return false when currentUser is null', () => {
      service.currentUser.set(null);
      expect(service.isAuthenticated()).toBeFalse();
    });
  });

  describe('register', () => {
    const mockRequest: RegisterCustomerRequest = {
      firstName: 'John',
      lastName: 'Doe',
      phoneNumber: '+123456789',
      email: 'test@example.com',
      password: 'password123',
    };

    it('should register and load the current user', () => {
      const mockResponse: TokenResponse = {
        token: 'mock-token',
      };
      const mockUser: User = {
        id: '1',
        firstName: 'John',
        lastName: 'Doe',
        phoneNumber: '+123456789',
        email: 'test@example.com',
        roles: ['customer'],
      };

      service.register(mockRequest).subscribe((user) => {
        expect(service.currentUser()).toEqual(mockUser);
      });

      const registerReq = httpMock.expectOne(
        `${environment.apiUrl}users/register`
      );
      expect(registerReq.request.method).toBe('POST');
      expect(registerReq.request.body).toEqual(mockRequest);
      registerReq.flush(mockResponse);

      const userReq = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      expect(userReq.request.method).toBe('GET');
      userReq.flush(mockUser);
    });

    it('should handle registration error', () => {
      service.register(mockRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.statusText).toBe('Bad Request');
          expect(localStorage.getItem('token')).toBeNull();
          expect(service.currentUser()).toBeNull();
        },
      });

      const registerReq = httpMock.expectOne(
        `${environment.apiUrl}users/register`
      );
      registerReq.flush('Registration failed', {
        status: 400,
        statusText: 'Bad Request',
      });
    });
  });

  describe('login', () => {
    const mockRequest: LoginRequest = {
      email: 'test@example.com',
      password: 'password123',
    };

    it('should log in and load the current user', () => {
      const mockResponse: TokenResponse = {
        token: 'mock-token',
      };
      const mockUser: User = {
        id: '1',
        firstName: 'Jane',
        lastName: 'Doe',
        phoneNumber: '+987654321',
        email: 'test@example.com',
        roles: ['admin'],
      };

      service.login(mockRequest).subscribe((user) => {
        expect(service.currentUser()).toEqual(mockUser);
      });

      const loginReq = httpMock.expectOne(
        `${environment.apiUrl}users/login`
      );
      expect(loginReq.request.method).toBe('POST');
      expect(loginReq.request.body).toEqual(mockRequest);
      loginReq.flush(mockResponse);

      const userReq = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      expect(userReq.request.method).toBe('GET');
      userReq.flush(mockUser);
    });

    it('should handle login error', () => {
      service.login(mockRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          expect(error.statusText).toBe('Unauthorized');
          expect(localStorage.getItem('token')).toBeNull();
          expect(service.currentUser()).toBeNull();
        },
      });

      const loginReq = httpMock.expectOne(
        `${environment.apiUrl}users/login`
      );
      loginReq.flush('Login failed', {
        status: 401,
        statusText: 'Unauthorized',
      });
    });
  });

  describe('logout', () => {
    it('should clear localStorage and reset currentUser', () => {
      spyOn(localStorage, 'removeItem');
      service.logout();
      expect(localStorage.removeItem).toHaveBeenCalledWith(
        'token'
      );
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('refreshToken', () => {
    it('should make a POST request to refresh token', () => {
      const mockResponse: TokenResponse = {
        token: 'new-mock-token',
      };

      service.refreshToken().subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}users/refresh-token`
      );
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });
  });

  describe('loadCurrentUser', () => {
    it('should update currentUser signal on successful load', (done) => {
      const mockUser: User = {
        id: '1',
        firstName: 'Jane',
        lastName: 'Doe',
        phoneNumber: '+1234567890',
        email: 'jane.doe@example.com',
        roles: ['customer'],
      };

      service.loadCurrentUser().subscribe();

      const req = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);

      // Use setTimeout to allow for signal update
      setTimeout(() => {
        expect(service.currentUser()).toEqual(mockUser);
        done();
      });
    });

    it('should set currentUser to null on error', () => {
      // Set an initial user to verify it gets cleared
      service.currentUser.set({
        id: '1',
        firstName: 'Initial',
        lastName: 'User',
        phoneNumber: '+1234567890',
        email: 'initial@example.com',
        roles: ['customer'],
      });

      service.loadCurrentUser().subscribe();

      const req = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      req.flush('Server error', {
        status: 500,
        statusText: 'Internal Server Error',
      });

      expect(service.currentUser()).toBeNull();
    });
  });
});
