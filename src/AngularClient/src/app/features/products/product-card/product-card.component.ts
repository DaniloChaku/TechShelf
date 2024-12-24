import { Component, input } from '@angular/core';
import { Product } from '../../../core/models/product';
import { MatCardModule } from '@angular/material/card';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [MatCardModule, MatIconModule, CurrencyPipe],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css',
})
export class ProductCardComponent {
  product = input.required<Product>();
}
