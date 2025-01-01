import { Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import {
  MatError,
  MatFormFieldModule,
  MatLabel,
} from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../core/services/user/user.service';
import { TOKEN } from '../../../core/constants/token';
import { ApiError } from '../../../core/models/api-error';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatLabel,
    MatButtonModule,
    MatError,
    ReactiveFormsModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private userService = inject(UserService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private returnUrl;
  loginForm = new FormGroup({
    email: new FormControl('', {
      validators: [Validators.required, Validators.email],
      nonNullable: true,
    }),
    password: new FormControl('', {
      validators: [Validators.required],
      nonNullable: true,
    }),
  });
  emailErrors = signal<string[]>([]);
  passwordErrors = signal<string[]>([]);
  errorMessage = signal<string | null>(null);

  constructor() {
    const returnUrl =
      this.activatedRoute.snapshot.queryParams['returnUrl'];

    this.returnUrl =
      returnUrl &&
      returnUrl.startsWith('/') &&
      !returnUrl.includes('//')
        ? returnUrl
        : '/catalog';
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);

    this.userService
      .login({
        ...this.loginForm.getRawValue(),
      })
      .subscribe({
        next: (tokenResponse) => {
          localStorage.setItem(TOKEN, tokenResponse.token);
          this.router.navigateByUrl(this.returnUrl);
        },
        error: (error: HttpErrorResponse) => {
          const apiError = error.error as ApiError;
          if (error.status === 400 && apiError.errors) {
            if (apiError.errors['Email']) {
              this.loginForm.get('email')?.setErrors([]);
              this.emailErrors.set(
                apiError.errors['Email']
              );
            }
            if (apiError.errors['Password']) {
              this.loginForm.get('password')?.setErrors([]);
              this.passwordErrors.set(
                apiError.errors['Password']
              );
            }
          } else if (error.status === 401) {
            this.errorMessage.set(apiError.title);
          }
        },
      });
  }
}
