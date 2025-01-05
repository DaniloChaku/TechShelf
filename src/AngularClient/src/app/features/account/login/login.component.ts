import { Component, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../core/services/user/user.service';
import { TOKEN } from '../../../core/constants/token';
import { ApiError } from '../../../core/models/api-error';
import { HttpErrorResponse } from '@angular/common/http';
import { containsLowercase } from '../../../shared/validators/contains-lowercase';
import { containsUppercase } from '../../../shared/validators/contains-uppercase';
import { containsNumber } from '../../../shared/validators/contains-number';
import { containsSpecial } from '../../../shared/validators/contains-special';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
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
      validators: [
        Validators.required,
        Validators.minLength(8),
        containsLowercase,
        containsUppercase,
        containsNumber,
        containsSpecial,
      ],
      nonNullable: true,
    }),
  });
  private emailErrors = signal<string[]>([]);
  private passwordErrors = signal<string[]>([]);
  errorMessage = signal<string | null>(null);

  get emailError() {
    const email = this.loginForm.get('email');
    if (!email) return null;

    console.log(email);
    if (email.hasError('required'))
      return 'Email address is required.';
    if (email.hasError('email'))
      return 'Please enter a valid email address.';
    if (this.passwordErrors())
      return this.passwordErrors()[0];

    return null;
  }

  get passwordError() {
    const password = this.loginForm.get('password');
    if (!password) return null;

    console.log(password);
    if (password.hasError('required'))
      return 'Password is required.';
    if (password.hasError('minlength'))
      return 'Password should contain at least 8 characters.';
    if (password.hasError('containsLowercase'))
      return 'Password must contain at least one lowercase letter.';
    if (password.hasError('containsUppercase'))
      return 'Password must contain at least one uppercase letter.';
    if (password.hasError('containsNumber'))
      return 'Password must contain at least one number.';
    if (password.hasError('containsSpecial'))
      return 'Password must contain at least one special character.';
    if (this.passwordErrors())
      return this.passwordErrors()[0];

    return null;
  }

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
    this.errorMessage.set(null);

    this.userService
      .login({
        ...this.loginForm.getRawValue(),
      })
      .subscribe({
        next: () => {
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
