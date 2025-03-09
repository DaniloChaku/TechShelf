import { Component, inject, signal } from '@angular/core';
import { UserService } from '../../../core/services/user/user.service';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { containsLowercase } from '../../../shared/validators/contains-lowercase';
import { containsUppercase } from '../../../shared/validators/contains-uppercase';
import { containsNumber } from '../../../shared/validators/contains-number';
import { containsSpecial } from '../../../shared/validators/contains-special';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../../../core/models/shared/api-error';
import { phoneNumber } from '../../../shared/validators/phone-number';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private userService = inject(UserService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private returnUrl;
  loginForm = new FormGroup({
    fullName: new FormControl('', {
      validators: [Validators.required],
      nonNullable: true,
    }),
    phoneNumber: new FormControl('', {
      validators: [Validators.required, phoneNumber],
      nonNullable: true,
    }),
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
    confirmPassword: new FormControl(''),
  });
  private firstNameErrors = signal<string[]>([]);
  private phoneNumberErrors = signal<string[]>([]);
  private emailErrors = signal<string[]>([]);
  private passwordErrors = signal<string[]>([]);
  errorMessage = signal<string | null>(null);

  get firstNameError() {
    const firstName = this.loginForm.controls.fullName;

    if (firstName.hasError('required')) {
      return 'First name is required.';
    }
    if (this.firstNameErrors()) {
      return this.firstNameErrors()[0];
    }

    return null;
  }

  get phoneNumberError() {
    const phoneNumber = this.loginForm.controls.phoneNumber;

    if (phoneNumber.hasError('required')) {
      return 'Phone number is required.';
    }
    if (phoneNumber.hasError('phoneNumber')) {
      return 'Enter a valid phone number. Sample format: +12345678901.';
    }
    if (this.phoneNumberErrors()) {
      return this.phoneNumberErrors()[0];
    }

    return null;
  }

  get emailError() {
    const email = this.loginForm.controls.email;

    if (email.hasError('required'))
      return 'Email address is required.';
    if (email.hasError('email'))
      return 'Please enter a valid email address.';
    if (this.passwordErrors()) return this.emailErrors()[0];

    return null;
  }

  get passwordError() {
    const password = this.loginForm.controls.password;

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

  get confirmPasswordError() {
    const confirmPassword =
      this.loginForm.controls.confirmPassword;

    if (confirmPassword.hasError('matches'))
      return 'Passwords do not match.';

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

  onPasswordChange() {
    if (
      this.loginForm.value.password !==
      this.loginForm.value.confirmPassword
    ) {
      this.loginForm.controls.confirmPassword.setErrors({
        matches: true,
      });
    }
  }

  onSubmit() {
    this.errorMessage.set(null);

    this.userService
      .register({
        ...this.loginForm.getRawValue(),
      })
      .subscribe({
        next: () => {
          this.router.navigateByUrl(this.returnUrl);
        },
        error: (error: HttpErrorResponse) => {
          const apiError = error.error as ApiError;
          if (error.status === 400 && apiError.errors) {
            const firstNameErrors =
              apiError.errors['FirstName'];
            if (firstNameErrors) {
              this.loginForm
                .get('firstName')
                ?.setErrors([]);
              this.firstNameErrors.set(firstNameErrors);
            }
            const phoneNumberErrors =
              apiError.errors['PhoneNumber'];
            if (phoneNumberErrors) {
              this.loginForm
                .get('phoneNumber')
                ?.setErrors([]);
              this.phoneNumberErrors.set(phoneNumberErrors);
            }
            const emailErrors = apiError.errors['Email'];
            if (emailErrors) {
              this.loginForm.get('email')?.setErrors([]);
              this.emailErrors.set(emailErrors);
            }
            const passwordErrors =
              apiError.errors['Password'];
            if (passwordErrors) {
              this.loginForm.get('password')?.setErrors([]);
              this.passwordErrors.set(passwordErrors);
            }
          } else {
            this.errorMessage.set(apiError.title);
          }
        },
      });
  }
}
