import { Component, inject, input } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.css',
})
export class NotFoundComponent {
  loadingService = inject(LoadingService);
  errorText = input.required<string>();
}
