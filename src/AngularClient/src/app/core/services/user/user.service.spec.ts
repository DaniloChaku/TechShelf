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

      const mockUser: UserDto = {
        firstName: 'John',
        lastName: 'Doe',
        phoneNumber: '+123456789',
        email: 'test@example.com',
        roles: ['customer'],
      };

      service
        .register(mockRequest)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      // Expect the POST request for registration
      const registerReq = httpMock.expectOne(
        `${environment.apiUrl}users/register`
      );
      expect(registerReq.request.method).toBe('POST');
      expect(registerReq.request.body).toEqual(mockRequest);
      registerReq.flush(mockResponse);

      // Expect the GET request for loading the current user
      const userReq = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      expect(userReq.request.method).toBe('GET');
      userReq.flush(mockUser);
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

      const mockUser: UserDto = {
        firstName: 'John',
        lastName: 'Doe',
        phoneNumber: '+123456789',
        email: 'test@example.com',
        roles: ['customer'],
      };

      service.login(mockRequest).subscribe((response) => {
        expect(response).toEqual(mockResponse);
      });

      // Expect the POST request for login
      const loginReq = httpMock.expectOne(
        `${environment.apiUrl}users/login`
      );
      expect(loginReq.request.method).toBe('POST');
      expect(loginReq.request.body).toEqual(mockRequest);
      loginReq.flush(mockResponse);

      // Expect the GET request for loading the current user
      const userReq = httpMock.expectOne(
        `${environment.apiUrl}users/me`
      );
      expect(userReq.request.method).toBe('GET');
      userReq.flush(mockUser);
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
        `${environment.apiUrl}users/me`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });
  });
});
