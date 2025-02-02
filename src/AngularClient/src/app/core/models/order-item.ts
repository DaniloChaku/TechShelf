import { ProductOrdered } from './product-ordered';

export interface OrderItem {
  price: number;
  quantity: number;
  productOrdered: ProductOrdered;
}
