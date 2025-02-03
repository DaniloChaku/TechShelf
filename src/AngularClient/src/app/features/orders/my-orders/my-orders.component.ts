import { Component, inject, signal } from '@angular/core';
import { Order } from '../../../core/models/order';
import { OrderService } from '../../../core/services/order/order.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { OrderDetailsComponent } from '../order-details/order-details.component';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatPaginatorModule,
    MatButtonModule,
  ],
  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.css',
})
export class MyOrdersComponent {
  private orderService = inject(OrderService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  pageSize = 10;
  orders = signal<Order[] | null>(null);
  totalCount = signal<number | null>(null);
  pageIndex = signal(1);

  constructor() {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.pageIndex.set(
        params['pageIndex'] ? +params['pageIndex'] : 1
      );
      this.fetchOrders();
    });
  }

  fetchOrders() {
    this.orderService
      .myorders(this.pageIndex(), this.pageSize)
      .subscribe({
        next: (result) => {
          this.orders.set(result.items);
          this.totalCount.set(result.totalCount);
        },
      });
  }

  onPageChange(event: any) {
    this.pageIndex.set(event.pageIndex + 1);

    this.router.navigate([], {
      queryParams: { pageIndex: this.pageIndex() },
      queryParamsHandling: 'merge',
    });

    this.fetchOrders();
  }

  openOrderDetails(order: Order) {
    this.dialog.open(OrderDetailsComponent, {
      width: '500px',
      data: order,
    });
  }
}
