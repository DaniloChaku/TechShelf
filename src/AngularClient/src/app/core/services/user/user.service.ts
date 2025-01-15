import {
  HttpClient,
  HttpContext,
} from '@angular/common/http';
import {
  computed,
  inject,
  Injectable,
  signal,
} from '@angular/core';
import { environment } from '../../../../environments/environment';
import { RegisterCustomerRequest } from '../../models/register-customer-request';
import { TokenResponse } from '../../models/token-response';
import { LoginRequest } from '../../models/login-request';
import { User } from '../../models/user';
import {
  catchError,
  Observable,
  of,
  switchMap,
  tap,
} from 'rxjs';
import {
  IS_REFRESH_TOKEN,
  TOKEN,
} from '../../constants/token';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl + 'users/';
  currentUser = signal<User | null>(null);
  isAuthenticated = computed(
    () => this.currentUser() !== null
  );

  private handleUserResponse(
    userResponse$: Observable<User>
  ) {
    return userResponse$.pipe(
      tap({
        next: (user) => this.currentUser.set(user),
      }),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      })
    );
  }

  private handleAuthentication(response: TokenResponse) {
    localStorage.setItem(TOKEN, response.token);
    return this.handleUserResponse(this.getCurrentUser());
  }

  register(request: RegisterCustomerRequest) {
    return this.http
      .post<TokenResponse>(
        `${this.baseUrl}register`,
        request,
        {
          withCredentials: true,
        }
      )
      .pipe(
        switchMap((response) =>
          this.handleAuthentication(response)
        )
      );
  }

  login(request: LoginRequest) {
    return this.http
      .post<TokenResponse>(
        `${this.baseUrl}login`,
        request,
        {
          withCredentials: true,
        }
      )
      .pipe(
        switchMap((response) =>
          this.handleAuthentication(response)
        )
      );
  }

  refreshToken() {
    return this.http.post<TokenResponse>(
      `${this.baseUrl}refresh-token`,
      {},
      {
        withCredentials: true,
        context: new HttpContext().set(
          IS_REFRESH_TOKEN,
          true
        ),
      }
    );
  }

  private getCurrentUser() {
    return this.http.get<User>(`${this.baseUrl}me`);
  }

  loadCurrentUser() {
    return this.handleUserResponse(
      this.getCurrentUser()
    ).subscribe();
  }

  logout() {
    localStorage.removeItem('token');
    this.currentUser.set(null);
  }
}
