import { Address } from './address';
import { CartItem } from './cart-item';

export interface CreateOrderRequest {
  email: string;
  phoneNumber: string;
  name: string;
  shippingAddress: Address;
  shoppingCartItems: CartItem[];
}
