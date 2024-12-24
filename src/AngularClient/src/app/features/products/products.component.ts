import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { PagedResult } from '../../core/models/pagedResult';
import { Product } from '../../core/models/product';
import { SearchProductsRequest } from '../../core/services/product/search-products-request';
import { ProductService } from '../../core/services/product/product.service';
import { MatIconModule } from '@angular/material/icon';
import { ProductCardComponent } from './product-card/product-card.component';
import {
  MatPaginator,
  PageEvent,
} from '@angular/material/paginator';
import { Brand } from '../../core/models/brand';
import { Category } from '../../core/models/category';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    MatIconModule,
    ProductCardComponent,
    MatPaginator,
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css',
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  products = signal<PagedResult<Product> | undefined>(
    undefined
  );
  brands = signal<Brand[]>([]);
  categories = signal<Category[]>([]);
  searchParams = signal<SearchProductsRequest>(
    this.getDefaultSearchParams()
  );

  ngOnInit(): void {
    this.getProducts();
  }

  private getProducts() {
    this.productService
      .getProducts(this.searchParams())
      .subscribe({
        next: (response) => this.products.set(response),
      });
  }

  getDefaultSearchParams(): SearchProductsRequest {
    return {
      pageIndex: 1,
      pageSize: 10,
    };
  }

  onPageChange(event: PageEvent) {
    this.searchParams().pageIndex = event.pageIndex + 1;
    this.getProducts();
  }
}
