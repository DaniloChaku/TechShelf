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

  describe('loadCart', () => {
    it('should load empty cart when localStorage is empty', () => {
      service.loadCart();
      expect(service.cart()).toEqual([]);
      expect(service.totalItems()).toBe(0);
    });

    it('should load existing cart from localStorage', () => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.loadCart();
      expect(service.cart()).toEqual(mockCartItems);
      expect(service.totalItems()).toBe(
        totalMockItemsCount
      );
    });

    it('should handle invalid JSON in localStorage', () => {
      localStorage.setItem('shopping_cart', 'invalid-json');
      spyOn(console, 'error');
      service.loadCart();
      expect(service.cart()).toEqual([]);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('clearCart', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.loadCart();
    });

    it('should remove cart from localStorage and update signal', () => {
      service.clearCart();
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
      expect(service.cart()).toEqual([]);
      expect(service.totalItems()).toBe(0);
    });
  });

  describe('removeItem', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.loadCart();
    });

    it('should remove specified item and update signals', () => {
      service.removeItem(1);
      const expectedCart = [{ productId: 2, quantity: 3 }];
      expect(service.cart()).toEqual(expectedCart);
      expect(service.totalItems()).toBe(3);
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual(expectedCart);
    });

    it('should clear cart when removing last item', () => {
      service.removeItem(1);
      service.removeItem(2);
      expect(service.cart()).toEqual([]);
      expect(service.totalItems()).toBe(0);
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
    });

    it('should not modify cart when removing non-existent item', () => {
      service.removeItem(999);
      expect(service.cart()).toEqual(mockCartItems);
      expect(service.totalItems()).toBe(
        totalMockItemsCount
      );
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual(mockCartItems);
    });
  });

  describe('updateItem', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.loadCart();
    });

    it('should update quantity and signals for existing item', () => {
      service.updateItem(1, 5);
      const updatedCart = service.cart();
      expect(updatedCart).toContain(
        jasmine.objectContaining({
          productId: 1,
          quantity: 5,
        })
      );
      expect(service.totalItems()).toBe(8); // 5 + 3
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual(updatedCart);
    });

    it('should add new item and update signals', () => {
      service.updateItem(3, 1);
      const updatedCart = service.cart();
      expect(updatedCart).toContain(
        jasmine.objectContaining({
          productId: 3,
          quantity: 1,
        })
      );
      expect(updatedCart.length).toBe(3);
      expect(service.totalItems()).toBe(6); // 2 + 3 + 1
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual(updatedCart);
    });

    it('should remove item and update signals when quantity is 0', () => {
      service.updateItem(1, 0);
      const updatedCart = service.cart();
      expect(updatedCart).not.toContain(
        jasmine.objectContaining({ productId: 1 })
      );
      expect(service.totalItems()).toBe(3);
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual(updatedCart);
    });

    it('should throw error for negative quantity', () => {
      expect(() => service.updateItem(1, -1)).toThrowError(
        'Invalid quantity. Must be a non-negative integer.'
      );
      expect(service.cart()).toEqual(mockCartItems);
    });

    it('should throw error for non-integer quantity', () => {
      expect(() => service.updateItem(1, 1.5)).toThrowError(
        'Invalid quantity. Must be a non-negative integer.'
      );
      expect(service.cart()).toEqual(mockCartItems);
    });
  });

  describe('getItemQuantity', () => {
    beforeEach(() => {
      localStorage.setItem(
        'shopping_cart',
        JSON.stringify(mockCartItems)
      );
      service.loadCart();
    });

    it('should return correct quantity for existing item', () => {
      expect(service.getItemQuantity(1)).toBe(2);
      expect(service.getItemQuantity(2)).toBe(3);
    });

    it('should return 0 for non-existent item', () => {
      expect(service.getItemQuantity(999)).toBe(0);
    });
  });

  describe('computed values', () => {
    it('should update totalItems when cart changes', () => {
      expect(service.totalItems()).toBe(0);

      service.updateItem(1, 2);
      expect(service.totalItems()).toBe(2);

      service.updateItem(2, 3);
      expect(service.totalItems()).toBe(5);

      service.removeItem(1);
      expect(service.totalItems()).toBe(3);
    });
  });

  describe('saveCart', () => {
    it('should remove from localStorage when cart is empty', () => {
      service.updateItem(1, 2);
      service.removeItem(1);
      expect(
        localStorage.getItem('shopping_cart')
      ).toBeNull();
    });

    it('should save to localStorage when cart has items', () => {
      service.updateItem(1, 2);
      expect(
        JSON.parse(
          localStorage.getItem('shopping_cart') || '[]'
        )
      ).toEqual([{ productId: 1, quantity: 2 }]);
    });
  });
});
