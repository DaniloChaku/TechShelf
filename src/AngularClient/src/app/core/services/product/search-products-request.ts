import { ProductSortOption } from './product-sort-option';

export interface SearchProductsRequest {
  pageIndex: number;
  pageSize: number;
  brandId?: number;
  categoryId?: number;
  name?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: ProductSortOption;
}
