import { Brand } from './brand';
import { Category } from './category';

export interface Product {
  id: number;
  name: string;
  description: string;
  categoryId: number;
  brandId: number;
  price: number;
  stock: number;
  thumbnailUrl: string;
  imageUrls: string[];
  category: Category;
  brand: Brand;
}
