import {
  Component,
  inject,
  OnDestroy,
  OnInit,
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
import { CreateOrderRequest } from '../../core/models/create-order-request';
import { OrderService } from '../../core/services/order/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    MatStepperModule,
    MatFormFieldModule,
    MatButtonModule,
    MatInputModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    FormsModule,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent
  implements OnInit, OnDestroy
{
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

    var request: CreateOrderRequest = {
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
    });
  }

  ngOnDestroy(): void {
    this.stripeService.disposeElements();
  }
}
