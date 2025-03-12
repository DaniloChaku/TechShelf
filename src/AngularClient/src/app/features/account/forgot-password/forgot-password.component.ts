import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ForgotPasswordRequest } from '../../../core/models/account/forgot-password-request';
import { UserService } from '../../../core/services/user/user.service';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  forgotPasswordForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  get emailError(): string | null {
    const control = this.forgotPasswordForm.get('email');
    if (
      control?.invalid &&
      (control.dirty || control.touched)
    ) {
      if (control.errors?.['required']) {
        return 'Email is required';
      }
      if (control.errors?.['email']) {
        return 'Please enter a valid email address';
      }
    }
    return null;
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const request: ForgotPasswordRequest = {
      email: this.forgotPasswordForm.get('email')?.value!,
    };

    this.userService
      .getPassowrdResetToken(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: () => {
          this.successMessage.set(
            'If the email exists in our system, you will receive a password reset link shortly.'
          );
        },
        error: (error: any) => {
          this.errorMessage.set(
            'An error occurred while processing your request. Please try again later.'
          );
        },
      });
  }
}
