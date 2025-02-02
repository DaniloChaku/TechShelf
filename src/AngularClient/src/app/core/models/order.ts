import { Address } from './address';
import { OrderHistoryEntry } from './order-history-entry';
import { OrderItem } from './order-item';

export interface Order {
  id: string;
  email: string;
  phoneNumber: string;
  fullName: string;
  customerId?: string;
  paymentIntentId?: string;
  total: number;
  address: Address;
  orderItems: OrderItem[];
  history: OrderHistoryEntry[];
}
