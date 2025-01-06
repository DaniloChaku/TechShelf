import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  filter,
  finalize,
  Observable,
  switchMap,
  take,
  tap,
  throwError,
} from 'rxjs';
import { UserService } from '../../services/user/user.service';
import {
  IS_REFRESH_TOKEN,
  TOKEN,
} from '../../constants/token';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private userService = inject(UserService);
  private refreshTokenInProgress = false;
  private refreshTokenSubject = new BehaviorSubject<
    string | null
  >(null);

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const token = localStorage.getItem(TOKEN);
    if (token) {
      request = this.addAuthHeader(request, token);
    }

    if (request.context.get(IS_REFRESH_TOKEN) || !token) {
      return next.handle(request);
    }

    return next.handle(request).pipe(
      catchError((error) => {
        if (
          error instanceof HttpErrorResponse &&
          error.status === 401
        ) {
          if (!this.refreshTokenInProgress) {
            this.refreshTokenInProgress = true;
            this.refreshTokenSubject.next(null);

            return this.userService.refreshToken().pipe(
              tap((response) => {
                const newToken = response.token;
                localStorage.setItem(TOKEN, newToken);
                this.refreshTokenSubject.next(newToken);
                this.refreshTokenInProgress = false;
              }),
              switchMap((response) => {
                const updatedRequest = this.addAuthHeader(
                  request,
                  response.token
                );
                return next.handle(updatedRequest);
              }),
              catchError((err) => {
                this.userService.logout();
                this.refreshTokenSubject.error(err);
                return throwError(() => err);
              }),
              finalize(() => {
                this.refreshTokenInProgress = false;
              })
            );
          } else {
            return this.refreshTokenSubject.pipe(
              filter(
                (token): token is string => token !== null
              ),
              take(1),
              switchMap((newToken) => {
                const updatedRequest = this.addAuthHeader(
                  request,
                  newToken
                );
                return next.handle(updatedRequest);
              }),
              catchError((err) => {
                return throwError(() => err);
              })
            );
          }
        }

        return throwError(() => error);
      })
    );
  }

  private addAuthHeader(
    request: HttpRequest<any>,
    token: string
  ): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }
}
