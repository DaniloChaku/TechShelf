import {
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ShoppingCartService } from '../../core/services/shopping-cart/shopping-cart.service';
import { ProductService } from '../../core/services/product/product.service';
import { Product } from '../../core/models/product';
import { CommonModule } from '@angular/common';
import { CartVisibilityService } from '../../core/services/cart-visibility/cart-visibility.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faXmark,
  faCirclePlus,
  faCircleMinus,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FaIconComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent {
  private cartVisibilityService = inject(
    CartVisibilityService
  );
  private productService = inject(ProductService);
  private shoppingCartService = inject(ShoppingCartService);
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
    this.loadCartProducts();
  }

  private loadCartProducts(): void {
    const cartItems = this.shoppingCartService.cart();
    cartItems.forEach((item) => {
      this.productService
        .getProductById(item.productId)
        .subscribe({
          next: (product) => {
            const currentProducts = this.loadedProducts();
            const updatedProducts = new Map(
              currentProducts
            );
            updatedProducts.set(product.id, product);
            this.loadedProducts.set(updatedProducts);
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
    this.shoppingCartService.updateItem(
      productId,
      currentQuantity + 1
    );
  }

  decrementQuantity(productId: number): void {
    const currentQuantity =
      this.shoppingCartService.getItemQuantity(productId);
    if (currentQuantity > 1) {
      this.shoppingCartService.updateItem(
        productId,
        currentQuantity - 1
      );
    }
  }
}
