import { Routes } from '@angular/router';
import { ProductsComponent } from './features/products/products.component';
import { ProductDetailsComponent } from './features/products/product-details/product-details.component';
import { HomeComponent } from './features/home/home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'catalog', component: ProductsComponent },
  {
    path: 'catalog/:id',
    component: ProductDetailsComponent,
  },
];
