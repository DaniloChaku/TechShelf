import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { UserService } from '../../../core/services/user/user.service';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { NotFoundComponent } from '../../../shared/components/not-found/not-found.component';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { ApiError } from '../../../core/models/shared/api-error';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule,
    NotFoundComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent {
  userService = inject(UserService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);
  nameForm = this.fb.group({
    fullName: [
      '',
      [Validators.required, Validators.maxLength(100)],
    ],
  });
  isSubmitting = signal(false);

  updateFirstName() {
    if (this.nameForm.valid) {
      this.isSubmitting.set(true);
      const fullName =
        this.nameForm.controls.fullName.value;

      if (!fullName) return;

      this.userService
        .updateFullName({ fullName })
        .subscribe({
          next: () => {
            this.snackBar.open(
              'Full name updated successfully',
              'Close',
              {
                duration: 3000,
              }
            );
            this.nameForm.reset();
            this.isSubmitting.set(false);
          },
          error: (error) => {
            const apiError = error.error as ApiError;
            console.log(apiError);
            const errorMessage = apiError.errors
              ? apiError.errors['fullName'][0]
              : 'Error updating first name';

            this.snackBar.open(errorMessage, 'Close', {
              duration: 3000,
            });
            this.isSubmitting.set(false);
          },
        });
    }
  }
}
