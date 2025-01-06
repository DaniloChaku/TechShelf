import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';

import { authGuard } from './auth.guard';
import { UserService } from '../../services/user/user.service';

describe('authGuard', () => {
  let router: jasmine.SpyObj<Router>;
  let userService: jasmine.SpyObj<UserService>;

  const executeGuard: CanActivateFn = (
    ...guardParameters
  ) =>
    TestBed.runInInjectionContext(() =>
      authGuard(...guardParameters)
    );

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigate']);
    userService = jasmine.createSpyObj('UserService', [], {
      isAuthenticated: jasmine.createSpy(),
    });

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: router },
        { provide: UserService, useValue: userService },
      ],
    });
  });

  it('should allow access when user is authenticated', () => {
    (
      userService.isAuthenticated as jasmine.Spy
    ).and.returnValue(true);

    const result = executeGuard(
      {} as any,
      { url: '/protected' } as any
    );

    expect(result).toBe(true);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should redirect to login when user is not authenticated', () => {
    (
      userService.isAuthenticated as jasmine.Spy
    ).and.returnValue(false);

    const result = executeGuard(
      {} as any,
      { url: '/protected' } as any
    );

    expect(result).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(
      ['/login'],
      { queryParams: { returnUrl: '/protected' } }
    );
  });

  it('should pass return URL when redirecting to login', () => {
    (
      userService.isAuthenticated as jasmine.Spy
    ).and.returnValue(false);

    executeGuard(
      {} as any,
      { url: '/dashboard/settings' } as any
    );

    expect(router.navigate).toHaveBeenCalledWith(
      ['/login'],
      { queryParams: { returnUrl: '/dashboard/settings' } }
    );
  });
});
