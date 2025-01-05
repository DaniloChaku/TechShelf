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
import { UserDto } from '../../models/user-dto';
import { tap } from 'rxjs';
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
  currentUser = signal<UserDto | null>(null);
  isAuthenticated = computed(
    () => this.currentUser() !== null
  );

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
        tap((respose) => {
          localStorage.setItem(TOKEN, respose.token);
          this.loadCurrentUser();
        })
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
        tap((respose) => {
          localStorage.setItem(TOKEN, respose.token);
          this.loadCurrentUser();
        })
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

  getCurrentUser() {
    return this.http.get<UserDto>(`${this.baseUrl}me`);
  }

  private loadCurrentUser() {
    this.getCurrentUser().subscribe({
      next: (user) => this.currentUser.set(user),
      error: () => this.currentUser.set(null),
    });
  }

  logout() {
    localStorage.removeItem('token');
    this.currentUser.set(null);
  }
}
