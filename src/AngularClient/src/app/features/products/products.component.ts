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
import { MatFormField } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { ProductCardComponent } from './product-card/product-card.component';
import {
  MatPaginator,
  PageEvent,
} from '@angular/material/paginator';
import { Brand } from '../../core/models/brand';
import { Category } from '../../core/models/category';
import { BrandService } from '../../core/services/brand/brand.service';
import { CategoryService } from '../../core/services/category/category.service';
import { MatButtonModule } from '@angular/material/button';
import {
  MatMenuModule,
  MatMenuTrigger,
} from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    MatIconModule,
    ProductCardComponent,
    MatPaginator,
    MatFormField,
    MatSelectModule,
    MatButtonModule,
    MatMenuModule,
    MatInputModule,
    MatFormFieldModule,
    FormsModule,
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css',
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private brandService = inject(BrandService);
  private categoryService = inject(CategoryService);
  products = signal<PagedResult<Product> | undefined>(
    undefined
  );
  brands = signal<Brand[]>([]);
  categories = signal<Category[]>([]);
  searchParams = this.getDefaultSearchParams();
  tempSearchParams = { ...this.searchParams };

  ngOnInit(): void {
    this.getProducts();
    this.getBrands();
    this.getCategories();
  }

  private getProducts() {
    this.productService
      .getProducts(this.searchParams)
      .subscribe({
        next: (response) => this.products.set(response),
      });
  }

  private getCategories() {
    this.categoryService.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
    });
  }

  private getBrands() {
    this.brandService.getBrands().subscribe({
      next: (brands) => this.brands.set(brands),
    });
  }

  getDefaultSearchParams(): SearchProductsRequest {
    return {
      pageIndex: 1,
      pageSize: 10,
      sortBy:
        this.tempSearchParams?.sortBy ?? 'alphabetically',
    };
  }

  onPageChange(event: PageEvent) {
    this.searchParams.pageIndex = event.pageIndex + 1;
    this.getProducts();
  }

  onSortChange() {
    this.searchParams.sortBy = this.tempSearchParams.sortBy;
    this.getProducts();
  }

  applyFilters(menuTrigger: MatMenuTrigger) {
    this.searchParams = { ...this.tempSearchParams };
    this.getProducts();
    menuTrigger.closeMenu();
  }

  resetFilters(menuTrigger: MatMenuTrigger) {
    this.searchParams = {
      ...this.getDefaultSearchParams(),
    };
    this.tempSearchParams = { ...this.searchParams };
    this.getProducts();
    menuTrigger.closeMenu();
  }

  onSearch() {
    this.getProducts();
  }

  clearSearch() {
    this.searchParams.name = undefined;
    this.getProducts();
  }
}
