import { TestBed } from '@angular/core/testing';
import { ShoppingCartService } from './shopping-cart.service';
import { CartItem } from '../../models/cart-item';

describe('ShoppingCartService', () => {
  let service: ShoppingCartService;
  let storageSpy: jasmine.Spy;

  const mockCartItems: CartItem[] = [
    { productId: 1, quantity: 2 },
    { productId: 2, quantity: 3 },
  ];
  const totalMockItemsCount = 5;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ShoppingCartService],
    });
    service = TestBed.inject(ShoppingCartService);

    localStorage.clear();
    storageSpy = spyOn(
      localStorage,
      'setItem'
    ).and.callThrough();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getItems', () => {
    it('should return empty array when cart is empty', () => {
      const items = service.getItems();
      expect(items).toEqual([]);
    });

    it('should return cart items when they exist', () => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      const items = service.getItems();
      expect(items).toEqual(mockCartItems);
    });
  });

  describe('getTotalItems', () => {
    it('should return 0 when cart is empty', () => {
      const total = service.getTotalItems();
      expect(total).toBe(0);
    });

    it('should return sum of all item quantities', () => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      const total = service.getTotalItems();
      expect(total).toBe(totalMockItemsCount);
    });
  });

  describe('clearCart', () => {
    it('should remove cart from localStorage', () => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.clearCart();
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
    });
  });

  describe('removeItem', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
    });

    it('should remove specified item from cart', () => {
      service.removeItem(1);
      const updatedCart = JSON.parse(
        localStorage.getItem('shopping_cart') || '[]'
      );
      expect(updatedCart).toEqual([
        { productId: 2, quantity: 3 },
      ]);
    });

    it('should clear cart when removing last item', () => {
      service.removeItem(1);
      service.removeItem(2);
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
    });

    it('should not modify cart when removing non-existent item', () => {
      service.removeItem(999);
      const updatedCart = JSON.parse(
        localStorage.getItem('shopping_cart') || '[]'
      );
      expect(updatedCart).toEqual(mockCartItems);
    });
  });

  describe('updateItem', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
    });

    it('should update quantity of existing item', () => {
      service.updateItem(1, 5);
      const updatedCart = JSON.parse(
        localStorage.getItem('shopping_cart') || '[]'
      );
      expect(updatedCart).toContain(
        jasmine.objectContaining({
          productId: 1,
          quantity: 5,
        })
      );
    });

    it('should add new item when productId does not exist', () => {
      service.updateItem(3, 1);
      const updatedCart = JSON.parse(
        localStorage.getItem('shopping_cart') || '[]'
      );
      expect(updatedCart).toContain(
        jasmine.objectContaining({
          productId: 3,
          quantity: 1,
        })
      );
      expect(updatedCart.length).toBe(3);
    });

    it('should remove item when updating quantity to 0', () => {
      service.updateItem(1, 0);
      const updatedCart = JSON.parse(
        localStorage.getItem('shopping_cart') || '[]'
      );
      expect(updatedCart).not.toContain(
        jasmine.objectContaining({ productId: 1 })
      );
    });

    it('should throw error for negative quantity', () => {
      expect(() => service.updateItem(1, -1)).toThrowError(
        'Invalid quantity. Must be a non-negative integer.'
      );
    });

    it('should throw error for non-integer quantity', () => {
      expect(() => service.updateItem(1, 1.5)).toThrowError(
        'Invalid quantity. Must be a non-negative integer.'
      );
    });

    it('should clear cart when last item quantity is set to 0', () => {
      service.updateItem(1, 0);
      service.updateItem(2, 0);
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
    });
  });
});
