import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../core/services/user/user.service';
import { containsUppercase } from '../../../shared/validators/contains-uppercase';
import { containsLowercase } from '../../../shared/validators/contains-lowercase';
import { containsNumber } from '../../../shared/validators/contains-number';
import { containsSpecial } from '../../../shared/validators/contains-special';
import { ResetPasswordRequest } from '../../../core/models/account/reset-password-request';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-password-reset',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    ReactiveFormsModule,
  ],
  templateUrl: './password-reset.component.html',
  styleUrl: './password-reset.component.css',
})
export class PasswordResetComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);

  token: string = '';
  email: string = '';
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  resetPasswordForm = new FormGroup({
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

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'];
      this.email = params['email'];
    });
  }

  get passwordError(): string | null {
    const password =
      this.resetPasswordForm.controls.password;

    if (
      password.invalid &&
      (password.dirty || password.touched)
    ) {
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
    }
    return null;
  }

  get confirmPasswordError(): string | null {
    const control =
      this.resetPasswordForm.controls.confirmPassword;
    if (
      control.invalid &&
      (control.dirty || control.touched)
    ) {
      if (control.errors?.['required']) {
        return 'Please confirm your password';
      }
      if (control.errors?.['matches']) {
        return 'Passwords do not match';
      }
    }
    return null;
  }

  onPasswordChange() {
    if (
      this.resetPasswordForm.value.password !==
      this.resetPasswordForm.value.confirmPassword
    ) {
      this.resetPasswordForm.controls.confirmPassword.setErrors(
        {
          matches: true,
        }
      );
    }
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const request: ResetPasswordRequest = {
      email: this.email,
      token: this.token,
      password: this.resetPasswordForm.value.password!,
    };

    this.userService
      .resetPassword(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: () => {
          this.successMessage.set(
            'Your password has been reset successfully!'
          );
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        },
        error: (error) => {
          this.errorMessage.set(
            'An error occurred while resetting your password. Please try again later.'
          );
        },
      });
  }
}
