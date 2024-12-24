export class SearchProductsRequest {
  pageIndex = 1;
  pageSize = 10;
  brandId?: number;
  categoryId?: number;
  name?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: 'name' | 'price';
  isDescending?: boolean;
}
