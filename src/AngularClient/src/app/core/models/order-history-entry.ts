export interface OrderHistoryEntry {
  orderId: string;
  status: string;
  date: Date;
  notes?: string;
}
