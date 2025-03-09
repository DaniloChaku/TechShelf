import { inject, Injectable } from '@angular/core';
import {
  Stripe,
  StripeAddressElement,
  StripeAddressElementOptions,
  StripeElements,
  loadStripe,
} from '@stripe/stripe-js';
import { environment } from '../../../../environments/environment';
import { UserService } from '../user/user.service';

@Injectable({
  providedIn: 'root',
})
export class StripeService {
  private userService = inject(UserService);
  private stripe: Promise<Stripe | null>;
  private elements?: StripeElements;
  private addressElement?: StripeAddressElement;

  constructor() {
    this.stripe = loadStripe(environment.stripeKey);
  }

  async initializeElements() {
    if (!this.elements) {
      const stripe = await this.stripe;
      if (stripe) {
        this.elements = stripe.elements({
          appearance: { labels: 'floating' },
        });
      } else {
        throw new Error('Stripe has not been loaded');
      }
    }
    return this.elements;
  }

  async createAddressElement(): Promise<StripeAddressElement> {
    if (!this.addressElement) {
      const elements = await this.initializeElements();
      if (!elements) {
        throw new Error(
          'Elements instance has not been loaded'
        );
      }

      const defaultValues = this.getDefaultValues();
      const options: StripeAddressElementOptions = {
        mode: 'shipping',
        allowedCountries: ['US'],
        fields: {
          phone: 'always',
        },
        validation: {
          phone: {
            required: 'always',
          },
        },
        defaultValues,
      };

      this.addressElement = elements.create(
        'address',
        options
      );
    }
    return this.addressElement;
  }

  private getDefaultValues(): StripeAddressElementOptions['defaultValues'] {
    const user = this.userService.currentUser();
    const defaultValues: StripeAddressElementOptions['defaultValues'] =
      {};

    if (user) {
      defaultValues.name = `${user.fullName}`;
      defaultValues.phone = user.phoneNumber;
    }

    return defaultValues;
  }

  disposeElements() {
    this.elements = undefined;
    this.addressElement = undefined;
  }
}
