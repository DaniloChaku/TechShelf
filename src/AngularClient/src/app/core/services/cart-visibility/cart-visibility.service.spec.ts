import { TestBed } from '@angular/core/testing';

import { CartVisibilityService } from './cart-visibility.service';

describe('CartVisibilityService', () => {
  let service: CartVisibilityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CartVisibilityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with cart not visible', () => {
    expect(service.isCartVisible()).toBeFalse();
  });

  it('should display cart when displayCart is called', () => {
    service.displayCart();
    expect(service.isCartVisible()).toBeTrue();
  });

  it('should hide cart when hideCart is called', () => {
    service.displayCart();
    service.hideCart();
    expect(service.isCartVisible()).toBeFalse();
  });

  it('should handle multiple state changes correctly', () => {
    expect(service.isCartVisible()).toBeFalse();

    service.displayCart();
    expect(service.isCartVisible()).toBeTrue();

    service.hideCart();
    expect(service.isCartVisible()).toBeFalse();

    service.displayCart();
    expect(service.isCartVisible()).toBeTrue();
  });
});
