import {
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { ShoppingCartService } from '../../core/services/shopping-cart/shopping-cart.service';
import { ProductService } from '../../core/services/product/product.service';
import { Product } from '../../core/models/product';
import { CartVisibilityService } from '../../core/services/cart-visibility/cart-visibility.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faXmark,
  faCirclePlus,
  faCircleMinus,
} from '@fortawesome/free-solid-svg-icons';
import { Router } from '@angular/router';
import { delay } from 'rxjs';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [FaIconComponent, MatProgressBarModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent {
  private cartVisibilityService = inject(
    CartVisibilityService
  );
  private productService = inject(ProductService);
  private shoppingCartService = inject(ShoppingCartService);
  private router = inject(Router);
  isLoading = signal(false);
  private loadedProducts = signal<Map<number, Product>>(
    new Map()
  );
  cartWithProducts = computed(() => {
    const products = this.loadedProducts();
    return this.shoppingCartService.cart().map((item) => ({
      ...item,
      product:
        products.get(item.productId) || ({} as Product),
    }));
  });
  totalPrice = computed(() => {
    return this.cartWithProducts()
      .reduce((total, item) => {
        return total + item.product.price * item.quantity;
      }, 0)
      .toFixed(2);
  });
  isCartVisible = computed(() =>
    this.cartVisibilityService.isCartVisible()
  );
  faXmark = faXmark;
  faCirclePlus = faCirclePlus;
  faCircleMinus = faCircleMinus;

  constructor() {
    effect(
      () => {
        if (this.isCartVisible()) {
          this.reloadProducts();
        }
      },
      { allowSignalWrites: true }
    );
  }

  private reloadProducts(): void {
    this.loadedProducts.set(new Map());
    this.loadCartProducts();
  }

  private loadCartProducts(): void {
    const cartItems = this.shoppingCartService.cart();
    let loadedCount = 0;
    this.isLoading.set(true);

    if (this.shoppingCartService.cart().length === 0) {
      this.isLoading.set(false);
      return;
    }

    cartItems.forEach((item) => {
      this.productService
        .getProductById(item.productId)
        .pipe(delay(1000))
        .subscribe({
          next: (product) => {
            const currentProducts = this.loadedProducts();
            const updatedProducts = new Map(
              currentProducts
            );
            updatedProducts.set(product.id, product);
            this.loadedProducts.set(updatedProducts);

            loadedCount++;
            if (loadedCount === cartItems.length) {
              this.isLoading.set(false);
            }
          },
          error: () => {
            loadedCount++;
            if (loadedCount === cartItems.length) {
              this.isLoading.set(false);
            }
          },
        });
    });
  }

  close() {
    this.cartVisibilityService.hideCart();
  }

  removeItem(productId: number) {
    this.shoppingCartService.removeItem(productId);
  }

  incrementQuantity(productId: number): void {
    const currentQuantity =
      this.shoppingCartService.getItemQuantity(productId);
    if (currentQuantity > 0) {
      this.shoppingCartService.updateItem(
        productId,
        currentQuantity + 1
      );
    }
  }

  decrementQuantity(productId: number): void {
    const currentQuantity =
      this.shoppingCartService.getItemQuantity(productId);
    if (currentQuantity > 0) {
      this.shoppingCartService.updateItem(
        productId,
        currentQuantity - 1
      );
    }
  }

  goToCatalog() {
    this.router.navigateByUrl('/catalog');
    this.cartVisibilityService.hideCart();
  }

  goToCheckout() {
    this.router.navigateByUrl('checkout');
    this.cartVisibilityService.hideCart();
  }
}
