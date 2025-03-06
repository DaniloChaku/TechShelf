import {
  Component,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ShoppingCartService } from '../../core/services/shopping-cart/shopping-cart.service';
import {
  MatSnackBar,
  MatSnackBarModule,
} from '@angular/material/snack-bar';
import { StripeService } from '../../core/services/stripe/stripe.service';
import { StripeAddressElement } from '@stripe/stripe-js';
import { UserService } from '../../core/services/user/user.service';
import { CreateOrderRequest } from '../../core/models/checkout/create-order-request';
import { OrderService } from '../../core/services/order/order.service';
import { ApiError } from '../../core/models/shared/api-error';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { ProductService } from '../../core/services/product/product.service';
import { Product } from '../../core/models/catalog/product';
import { CartItem } from '../../core/models/cart/cart-item';
import { CurrencyPipe } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { forkJoin, map } from 'rxjs';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    MatStepperModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule,
    MatSnackBarModule,
    MatCardModule,
    MatDividerModule,
    ReactiveFormsModule,
    FormsModule,
    CurrencyPipe,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent
  implements OnInit, OnDestroy
{
  private productService = inject(ProductService);
  private shoppingCartService = inject(ShoppingCartService);
  private userService = inject(UserService);
  private stripeService = inject(StripeService);
  private orderService = inject(OrderService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  checkoutForm = this.fb.group({
    email: [
      this.userService.currentUser()?.email,
      [Validators.required, Validators.email],
    ],
  });
  addressElement?: StripeAddressElement;
  validationErrors = signal<string | null>(null);
  productsInCart: {
    cart: CartItem;
    product: Product;
  }[] = [];
  totalPrice?: string;

  constructor() {
    const productRequests = this.shoppingCartService
      .cart()
      .map((item) =>
        this.productService
          .getProductById(item.productId)
          .pipe(
            map((product) => ({
              cart: item,
              product,
            }))
          )
      );

    forkJoin(productRequests).subscribe({
      next: (products) => {
        this.productsInCart = products;
        this.totalPrice = this.productsInCart
          .reduce(
            (sum, cur) =>
              sum + cur.cart.quantity * cur.product.price,
            0
          )
          .toFixed(2);
      },
    });
  }

  async ngOnInit() {
    try {
      this.addressElement =
        await this.stripeService.createAddressElement();
      this.addressElement.mount('#address-element');
    } catch (error) {
      console.error(
        'Error initializing Stripe Address Element:',
        error
      );
      this.snackBar.open(
        'Failed to load payment form. Please try reloading the page.',
        'Close',
        {
          duration: 5000,
        }
      );
    }
  }

  async pay() {
    this.validationErrors.set(null);
    if (this.checkoutForm.invalid) {
      this.checkoutForm.controls.email.markAsTouched();
      return;
    }

    if (!this.addressElement) {
      this.snackBar.open(
        'Address information is not loaded. Please try again.',
        'Close',
        {
          duration: 5000,
        }
      );
      return;
    }

    const addressElementValues =
      await this.addressElement.getValue();

    if (!addressElementValues.complete) {
      this.snackBar.open(
        'Please provide a complete and valid address.',
        'Close',
        {
          duration: 5000,
        }
      );
      return;
    }

    const address = addressElementValues.value.address;

    const request: CreateOrderRequest = {
      name: addressElementValues.value.name,
      email: this.checkoutForm.value.email!,
      phoneNumber: addressElementValues.value.phone!,
      shippingAddress: {
        line1: address.line1,
        line2: address.line2 ?? undefined,
        city: address.city,
        state: address.state,
        postalCode: address.postal_code,
      },
      shoppingCartItems: this.shoppingCartService.cart(),
    };

    this.orderService.checkout(request).subscribe({
      next: (response) =>
        (window.location.href = response.stripeUrl),
      error: (error: HttpErrorResponse) => {
        const apiError = error.error as ApiError;
        this.snackBar.open(
          apiError.detail || apiError.title,
          'Close'
        );

        if (!apiError.errors) return;

        const validationErrorsCombined = Object.values(
          apiError.errors
        ).reduce(
          (message, cur) => (message += cur.join('\n')),
          ''
        );
        this.validationErrors.set(validationErrorsCombined);
      },
    });
  }

  ngOnDestroy(): void {
    this.stripeService.disposeElements();
  }
}
