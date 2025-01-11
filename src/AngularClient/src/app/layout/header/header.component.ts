import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatBadgeModule } from '@angular/material/badge';
import {
  RouterLink,
  RouterLinkActive,
} from '@angular/router';
import { LoadingService } from '../../core/services/loading/loading.service';
import { UserService } from '../../core/services/user/user.service';
import { MatMenuModule } from '@angular/material/menu';
import { ShoppingCartService } from '../../core/services/shopping-cart/shopping-cart.service';

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
    MatMenuModule,
    MatBadgeModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  loadingService = inject(LoadingService);
  userService = inject(UserService);
  shoppingCartService = inject(ShoppingCartService);
  isMenuOpen = false;
  cartItemsCount = computed(() =>
    this.shoppingCartService.totalItems()
  );
  cartCountHidden = computed(
    () => this.shoppingCartService.totalItems() == 0
  );

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout() {
    this.userService.logout();
  }
}
