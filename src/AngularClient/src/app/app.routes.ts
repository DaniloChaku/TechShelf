import { Routes } from '@angular/router';
import { ProductsComponent } from './features/products/products.component';
import { ProductDetailsComponent } from './features/products/product-details/product-details.component';

export const routes: Routes = [
  { path: '', component: ProductsComponent },
  {
    path: 'products/:id',
    component: ProductDetailsComponent,
  },
];
