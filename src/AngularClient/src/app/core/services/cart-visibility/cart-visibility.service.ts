import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CartVisibilityService {
  readonly isCartVisible = signal(false);

  displayCart() {
    this.isCartVisible.set(true);
  }

  hideCart() {
    this.isCartVisible.set(false);
  }
}
