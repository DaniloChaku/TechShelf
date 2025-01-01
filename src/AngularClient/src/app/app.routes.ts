import { Routes } from '@angular/router';
import { CatalogComponent } from './features/catalog/catalog.component';
import { ProductDetailsComponent } from './features/catalog/product-details/product-details.component';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/account/login/login.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'catalog', component: CatalogComponent },
  {
    path: 'catalog/:id',
    component: ProductDetailsComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
];
