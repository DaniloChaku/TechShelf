import {
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import {
  RouterLink,
  RouterLinkActive,
} from '@angular/router';
import { LoadingService } from '../../core/services/loading.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatButtonModule,
    RouterLinkActive,
    MatSidenavModule,
    MatProgressBarModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  loadingService = inject(LoadingService);
  isMenuOpen = false;

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }
}
