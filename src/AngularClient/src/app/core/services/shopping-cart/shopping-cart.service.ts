import {
  computed,
  Injectable,
  signal,
} from '@angular/core';
import { CartItem } from '../../models/cart/cart-item';

@Injectable({
  providedIn: 'root',
})
export class ShoppingCartService {
  private readonly CART_KEY = 'shopping_cart';
  private readonly cartSignal = signal<CartItem[]>([]);
  readonly cart = computed(() => this.cartSignal());
  readonly totalItems = computed(() =>
    this.cartSignal().reduce(
      (total, item) => total + item.quantity,
      0
    )
  );

  loadCart() {
    try {
      const cart = localStorage.getItem(this.CART_KEY);
      this.cartSignal.set(cart ? JSON.parse(cart) : []);
    } catch (error) {
      console.error(
        'Error loading cart from localStorage:',
        error
      );
    }
  }

  private saveCart(cart: CartItem[]): void {
    if (cart.length === 0) {
      localStorage.removeItem(this.CART_KEY);
    } else {
      localStorage.setItem(
        this.CART_KEY,
        JSON.stringify(cart)
      );
    }
    this.cartSignal.set(cart);
  }

  private isValidQuantity(quantity: number): boolean {
    return Number.isInteger(quantity) && quantity >= 0;
  }

  clearCart(): void {
    this.saveCart([]);
  }

  removeItem(productId: number): void {
    const updatedCart = this.cartSignal().filter(
      (item) => item.productId !== productId
    );
    this.saveCart(updatedCart);
  }

  updateItem(productId: number, quantity: number): void {
    if (!this.isValidQuantity(quantity)) {
      throw new Error(
        'Invalid quantity. Must be a non-negative integer.'
      );
    }

    const currentCart = this.cartSignal();
    const itemIndex = currentCart.findIndex(
      (item) => item.productId === productId
    );
    const updatedCart = [...currentCart];

    if (quantity === 0) {
      if (itemIndex > -1) {
        updatedCart.splice(itemIndex, 1);
      }
    } else if (itemIndex > -1) {
      updatedCart[itemIndex] = {
        ...updatedCart[itemIndex],
        quantity,
      };
    } else {
      updatedCart.push({ productId, quantity });
    }

    this.saveCart(updatedCart);
  }

  getItemQuantity(productId: number): number {
    return (
      this.cartSignal().find(
        (item) => item.productId === productId
      )?.quantity ?? 0
    );
  }
}
