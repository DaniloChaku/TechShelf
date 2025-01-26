import {
  APP_INITIALIZER,
  ApplicationConfig,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';

import { AuthInterceptor } from './core/interceptors/auth/auth.interceptor';
import { loadingInterceptor } from './core/interceptors/loading/loading.interceptor';
import { UserService } from './core/services/user/user.service';
import { ShoppingCartService } from './core/services/shopping-cart/shopping-cart.service';
import { firstValueFrom } from 'rxjs';

export function initializeApp(
  userService: UserService,
  shoppingCartService: ShoppingCartService
) {
  return () => {
    shoppingCartService.loadCart();
    return firstValueFrom(userService.loadCurrentUser());
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withInterceptors([loadingInterceptor]),
      withInterceptorsFromDi()
    ),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [UserService, ShoppingCartService],
      multi: true,
    },
  ],
};
