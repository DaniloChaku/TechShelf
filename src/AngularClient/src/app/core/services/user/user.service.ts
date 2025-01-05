import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { RegisterCustomerRequest } from '../../models/register-customer-request';
import { TokenResponse } from '../../models/token-response';
import { LoginRequest } from '../../models/login-request';
import { UserDto } from '../../models/user-dto';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl + 'users/';

  register(request: RegisterCustomerRequest) {
    return this.http.post<TokenResponse>(
      `${this.baseUrl}register`,
      request,
      {
        withCredentials: true,
      }
    );
  }

  login(request: LoginRequest) {
    return this.http.post<TokenResponse>(
      `${this.baseUrl}login`,
      request,
      {
        withCredentials: true,
      }
    );
  }

  refreshToken() {
    return this.http.post<TokenResponse>(
      `${this.baseUrl}refresh-token`,
      {},
      {
        withCredentials: true,
      }
    );
  }

  getCurrentUser() {
    return this.http.get<UserDto>(`${this.baseUrl}/me`);
  }
}
