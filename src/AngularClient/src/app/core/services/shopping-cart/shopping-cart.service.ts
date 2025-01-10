import { Injectable } from '@angular/core';
import { CartItem } from '../../models/cart-item';

@Injectable({
  providedIn: 'root',
})
export class ShoppingCartService {
  private readonly CART_KEY = 'shopping_cart';

  private getCart(): CartItem[] {
    const cart = localStorage.getItem(this.CART_KEY);
    return cart ? JSON.parse(cart) : [];
  }

  private saveCart(cart: CartItem[]): void {
    if (cart.length === 0) {
      this.clearCart();
      return;
    }
    localStorage.setItem(
      this.CART_KEY,
      JSON.stringify(cart)
    );
  }

  private isValidQuantity(quantity: number): boolean {
    return Number.isInteger(quantity) && quantity >= 0;
  }

  getItems(): CartItem[] {
    return this.getCart();
  }

  getTotalItems(): number {
    const cart = this.getCart();
    return cart.reduce(
      (total, item) => total + item.quantity,
      0
    );
  }

  clearCart(): void {
    localStorage.removeItem(this.CART_KEY);
  }

  removeItem(productId: number): void {
    const cart = this.getCart().filter(
      (item) => item.productId !== productId
    );
    this.saveCart(cart);
  }

  updateItem(productId: number, quantity: number): void {
    if (!this.isValidQuantity(quantity)) {
      throw new Error(
        'Invalid quantity. Must be a non-negative integer.'
      );
    }

    const cart = this.getCart();
    const itemIndex = cart.findIndex(
      (item) => item.productId === productId
    );

    if (quantity === 0) {
      if (itemIndex > -1) {
        cart.splice(itemIndex, 1);
      }
    } else if (itemIndex > -1) {
      cart[itemIndex].quantity = quantity;
    } else {
      cart.push({ productId, quantity });
    }

    this.saveCart(cart);
  }
}
