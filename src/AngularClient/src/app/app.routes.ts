import { Routes } from '@angular/router';
import { CatalogComponent } from './features/catalog/catalog.component';
import { ProductDetailsComponent } from './features/catalog/product-details/product-details.component';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/account/login/login.component';
import { RegisterComponent } from './features/account/register/register.component';
import { ProfileComponent } from './features/account/profile/profile.component';
import { authGuard } from './core/guards/auth/auth.guard';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { CheckoutSuccessComponent } from './features/checkout/checkout-success/checkout-success.component';
import { MyOrdersComponent } from './features/orders/my-orders/my-orders.component';
import { ForgotPasswordComponent } from './features/account/forgot-password/forgot-password.component';
import { PasswordResetComponent } from './features/account/password-reset/password-reset.component';

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
  {
    path: 'register',
    component: RegisterComponent,
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent,
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [authGuard],
  },
  {
    path: 'checkout',
    component: CheckoutComponent,
    canActivate: [authGuard],
  },
  {
    path: 'checkout/success',
    component: CheckoutSuccessComponent,
    canActivate: [authGuard],
  },
  {
    path: 'myorders',
    component: MyOrdersComponent,
    canActivate: [authGuard],
  },
];
