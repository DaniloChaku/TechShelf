import {
  Component,
  computed,
  inject,
  input,
} from '@angular/core';
import { Product } from '../../../core/models/product';
import { MatCardModule } from '@angular/material/card';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { ShoppingCartService } from '../../../core/services/shopping-cart/shopping-cart.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCartPlus,
  faCheck,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    CurrencyPipe,
    RouterLink,
    FaIconComponent,
  ],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css',
})
export class ProductCardComponent {
  shoppingCartService = inject(ShoppingCartService);
  product = input.required<Product>();
  isInCart = computed<boolean>(() => {
    return (
      this.shoppingCartService
        .cart()
        .findIndex(
          (p) => p.productId == this.product().id
        ) > -1
    );
  });
  faCartPlus = faCartPlus;
  faCheck = faCheck;

  addToCart() {
    this.shoppingCartService.updateItem(
      this.product().id,
      1
    );
  }
}
