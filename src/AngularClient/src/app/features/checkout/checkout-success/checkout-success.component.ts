import { Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { ActivatedRoute } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCircleCheck } from '@fortawesome/free-solid-svg-icons';
import { ShoppingCartService } from '../../../core/services/shopping-cart/shopping-cart.service';

@Component({
  selector: 'app-checkout-success',
  standalone: true,
  imports: [MatCardModule, FaIconComponent],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.css',
})
export class CheckoutSuccessComponent {
  private route = inject(ActivatedRoute);
  private shoppingCartService = inject(ShoppingCartService);
  orderId?: string;
  faCircleCheck = faCircleCheck;

  ngOnInit(): void {
    this.shoppingCartService.clearCart();
    this.route.queryParams.subscribe((params) => {
      this.orderId = params['orderId'];
    });
  }
}
