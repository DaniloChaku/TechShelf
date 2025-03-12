import { TestBed } from '@angular/core/testing';

import { UserService } from './user.service';
import { environment } from '../../../../environments/environment';
import {
  HttpErrorResponse,
  provideHttpClient,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { RegisterCustomerRequest } from '../../models/account/register-customer-request';
import { TokenResponse } from '../../models/account/token-response';
import { LoginRequest } from '../../models/account/login-request';
import { User } from '../../models/account/user';
import { UpdateFullNameRequest } from '../../models/account/update-full-name-request';
import { ForgotPasswordRequest } from '../../models/account/forgot-password-request';
import { ResetPasswordRequest } from '../../models/account/reset-password-request';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl + 'users/';

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
        fullName: 'Test User',
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
      fullName: 'John Doe',
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
        fullName: 'John Doe',
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
        fullName: 'Jane Doe',
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
        fullName: 'Jane Doe',
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
        fullName: 'Initial User',
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

  describe('updateFullName', () => {
    const mockRequest: UpdateFullNameRequest = {
      fullName: 'New Name',
    };
    const expectedUpdateUrl = baseUrl + 'me/name';
    const expectedLoadUserUrl = baseUrl + 'me';
    const mockUserResponse = {
      id: '123',
      fullName: 'New Name',
      email: 'test@example.com',
    };

    it('should send PUT request to update name and then load current user', () => {
      service
        .updateFullName(mockRequest)
        .subscribe((response) => {
          expect(response).toBeTruthy();
        });

      // Test the first request (update name)
      const updateReq = httpMock.expectOne(
        expectedUpdateUrl
      );
      expect(updateReq.request.method).toBe('PUT');
      expect(updateReq.request.body).toEqual(mockRequest);
      updateReq.flush({});

      // Test the second request (load user)
      const loadUserReq = httpMock.expectOne(
        expectedLoadUserUrl
      );
      expect(loadUserReq.request.method).toBe('GET');
      loadUserReq.flush(mockUserResponse);
    });

    it('should propagate errors from the update request', () => {
      const errorResponse = new HttpErrorResponse({
        error: 'Update failed',
        status: 400,
        statusText: 'Bad Request',
      });

      service.updateFullName(mockRequest).subscribe({
        next: () =>
          fail('Expected error, got success response'),
        error: (error) => expect(error).toBeTruthy(),
      });

      const updateReq = httpMock.expectOne(
        expectedUpdateUrl
      );
      updateReq.flush('Update failed', errorResponse);

      // No load user request should be made
      httpMock.expectNone(expectedLoadUserUrl);
    });
  });

  describe('getPassowrdResetToken', () => {
    const mockRequest: ForgotPasswordRequest = {
      email: 'test@example.com',
    };
    const expectedUrl = `${baseUrl}forgot-password`;

    it('should send POST request for password reset token', () => {
      const mockResponse = { message: 'Reset token sent' };

      service
        .getPassowrdResetToken(mockRequest)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      const req = httpMock.expectOne(expectedUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });

    it('should handle error when getting password reset token', () => {
      service.getPassowrdResetToken(mockRequest).subscribe({
        next: () =>
          fail('Expected error, got success response'),
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.statusText).toBe('Bad Request');
        },
      });

      const req = httpMock.expectOne(expectedUrl);
      req.flush('Invalid email', {
        status: 400,
        statusText: 'Bad Request',
      });
    });
  });

  describe('resetPassword', () => {
    const mockRequest: ResetPasswordRequest = {
      token: 'reset-token-123',
      email: 'some@example.com',
      password: 'newPassword123',
    };
    const expectedUrl = `${baseUrl}reset-password`;

    it('should send POST request to reset password', () => {
      const mockResponse = {
        message: 'Password reset successful',
      };

      service
        .resetPassword(mockRequest)
        .subscribe((response) => {
          expect(response).toEqual(mockResponse);
        });

      const req = httpMock.expectOne(expectedUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });

    it('should handle error when resetting password', () => {
      service.resetPassword(mockRequest).subscribe({
        next: () =>
          fail('Expected error, got success response'),
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.statusText).toBe('Bad Request');
        },
      });

      const req = httpMock.expectOne(expectedUrl);
      req.flush('Invalid reset token', {
        status: 400,
        statusText: 'Bad Request',
      });
    });
  });
});
