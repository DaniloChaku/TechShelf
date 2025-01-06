import { Component, inject } from '@angular/core';
import { UserService } from '../../../core/services/user/user.service';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { NotFoundComponent } from '../../../shared/components/not-found/not-found.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    NotFoundComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent {
  userService = inject(UserService);
}
