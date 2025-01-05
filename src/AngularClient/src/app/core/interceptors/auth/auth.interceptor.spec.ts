import {
  TestBed,
  fakeAsync,
  tick,
} from '@angular/core/testing';
import {
  HttpRequest,
  HttpHandler,
  HttpErrorResponse,
  HttpResponse,
  provideHttpClient,
  HttpContext,
} from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { AuthInterceptor } from './auth.interceptor';
import { UserService } from '../../services/user/user.service';
import {
  TOKEN,
  IS_REFRESH_TOKEN,
} from '../../constants/token';
import { of, throwError } from 'rxjs';

describe('AuthInterceptor', () => {
  let interceptor: AuthInterceptor;
  let httpMock: HttpTestingController;
  let userService: jasmine.SpyObj<UserService>;
  let httpHandler: jasmine.SpyObj<HttpHandler>;

  beforeEach(() => {
    userService = jasmine.createSpyObj('UserService', [
      'refreshToken',
      'logout',
    ]);
    httpHandler = jasmine.createSpyObj('HttpHandler', [
      'handle',
    ]);

    TestBed.configureTestingModule({
      providers: [
        AuthInterceptor,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: UserService, useValue: userService },
      ],
    });

    interceptor = TestBed.inject(AuthInterceptor);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(interceptor).toBeTruthy();
  });

  it('should not modify request when no token exists', () => {
    const request = new HttpRequest('GET', '/api/data');
    httpHandler.handle.and.returnValue(
      of(new HttpResponse())
    );

    interceptor.intercept(request, httpHandler).subscribe();

    expect(httpHandler.handle).toHaveBeenCalledWith(
      request
    );
    expect(
      request.headers.has('Authorization')
    ).toBeFalse();
  });

  it('should pass through request when IS_REFRESH_TOKEN context is set, but still include auth header', () => {
    localStorage.setItem(TOKEN, 'test-token');
    const context = new HttpContext();
    context.set(IS_REFRESH_TOKEN, true);
    const request = new HttpRequest(
      'POST',
      '/api/refresh',
      null,
      { context }
    );
    httpHandler.handle.and.returnValue(
      of(new HttpResponse())
    );

    interceptor.intercept(request, httpHandler).subscribe();

    const handledRequest =
      httpHandler.handle.calls.first().args[0];
    expect(
      handledRequest.headers.get('Authorization')
    ).toBe('Bearer test-token');
    expect(userService.refreshToken).not.toHaveBeenCalled();
  });

  it('should add auth header when token exists', () => {
    localStorage.setItem(TOKEN, 'test-token');
    const request = new HttpRequest('GET', '/api/data');
    httpHandler.handle.and.returnValue(
      of(new HttpResponse())
    );

    interceptor.intercept(request, httpHandler).subscribe();

    const modifiedRequest =
      httpHandler.handle.calls.first().args[0];
    expect(
      modifiedRequest.headers.get('Authorization')
    ).toBe('Bearer test-token');
  });

  it('should refresh token on 401 error and retry request', fakeAsync(() => {
    localStorage.setItem(TOKEN, 'old-token');
    const request = new HttpRequest('GET', '/api/data');
    const newToken = 'new-token';

    // First request fails with 401
    httpHandler.handle.and.returnValues(
      throwError(
        () => new HttpErrorResponse({ status: 401 })
      ),
      of(new HttpResponse())
    );

    userService.refreshToken.and.returnValue(
      of({ token: newToken })
    );

    interceptor.intercept(request, httpHandler).subscribe({
      next: () => {
        expect(localStorage.getItem(TOKEN)).toBe(newToken);
      },
    });

    tick();

    expect(userService.refreshToken).toHaveBeenCalled();
    expect(httpHandler.handle).toHaveBeenCalledTimes(2);

    const retryRequest =
      httpHandler.handle.calls.mostRecent().args[0];
    expect(retryRequest.headers.get('Authorization')).toBe(
      `Bearer ${newToken}`
    );
  }));

  it('should logout and throw error when token refresh fails', fakeAsync(() => {
    localStorage.setItem(TOKEN, 'old-token');
    const request = new HttpRequest('GET', '/api/data');
    const refreshError = new Error('Refresh failed');

    httpHandler.handle.and.returnValue(
      throwError(
        () => new HttpErrorResponse({ status: 401 })
      )
    );
    userService.refreshToken.and.returnValue(
      throwError(() => refreshError)
    );

    interceptor.intercept(request, httpHandler).subscribe({
      error: (error) => {
        expect(error).toBe(refreshError);
        expect(userService.logout).toHaveBeenCalled();
      },
    });

    tick();
  }));
});
