import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { Order } from '../../../core/models/order';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatDialogModule,
    MatButtonModule,
    DatePipe,
  ],
  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.css',
})
export class OrderDetailsComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public order: Order,
    private dialogRef: MatDialogRef<OrderDetailsComponent>
  ) {}

  close() {
    this.dialogRef.close();
  }
}
