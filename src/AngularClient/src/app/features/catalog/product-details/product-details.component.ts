import {
  Component,
  computed,
  inject,
  OnInit,
} from '@angular/core';
import { ProductService } from '../../../core/services/product/product.service';
import {
  ActivatedRoute,
  RouterLink,
} from '@angular/router';
import { Product } from '../../../core/models/product';
import { CurrencyPipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatDivider } from '@angular/material/divider';
import { NotFoundComponent } from '../../../shared/components/not-found/not-found.component';
import { CarouselComponent } from '../../../shared/components/carousel/carousel.component';
import { ShoppingCartService } from '../../../core/services/shopping-cart/shopping-cart.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCartPlus,
  faCheck,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    MatInputModule,
    MatDivider,
    NotFoundComponent,
    CarouselComponent,
    RouterLink,
    FormsModule,
    FaIconComponent,
  ],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
})
export class ProductDetailsComponent implements OnInit {
  private productService = inject(ProductService);
  private activatedRoute = inject(ActivatedRoute);
  private shoppingCartService = inject(ShoppingCartService);
  product?: Product;
  isInCart = computed<boolean>(() => {
    if (!this.product) return false;
    return (
      this.shoppingCartService
        .cart()
        .findIndex((p) => p.productId == this.product!.id) >
      -1
    );
  });
  faCartPlus = faCartPlus;
  faCheck = faCheck;

  get unavailable() {
    return this.product?.stock === 0;
  }

  ngOnInit(): void {
    var id =
      this.activatedRoute.snapshot.paramMap.get('id');
    if (id) {
      this.productService.getProductById(+id).subscribe({
        next: (product) => {
          this.product = product;
        },
      });
    }
  }

  addToCart() {
    if (this.product) {
      this.shoppingCartService.updateItem(
        this.product.id,
        1
      );
    }
  }
}
