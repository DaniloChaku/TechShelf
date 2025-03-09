import { TestBed } from '@angular/core/testing';
import { StripeService } from './stripe.service';
import { UserService } from '../user/user.service';
import { User } from '../../models/account/user';

// Mock Stripe and StripeElements
const mockStripe = {
  elements: jasmine.createSpy('elements').and.returnValue({
    create: jasmine.createSpy('create').and.returnValue({}),
  }),
};

const mockStripePromise = Promise.resolve(mockStripe);

// Mock UserService
const mockUserService = {
  currentUser: jasmine
    .createSpy('currentUser')
    .and.returnValue({
      id: '1',
      fullName: 'John Doe',
      phoneNumber: '123-456-7890',
      email: 'email@example.com',
    } as User),
};

describe('StripeService', () => {
  let service: StripeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StripeService,
        { provide: UserService, useValue: mockUserService },
      ],
    });

    service = TestBed.inject(StripeService);

    // Override the stripe property directly
    (service as any).stripe = mockStripePromise;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initializeElements', () => {
    it('should initialize Stripe elements', async () => {
      const elements = await service.initializeElements();
      expect(elements).toBeTruthy();
      expect(mockStripe.elements).toHaveBeenCalledWith({
        appearance: { labels: 'floating' },
      });
    });

    it('should throw an error if Stripe is not loaded', async () => {
      // Simulate Stripe not being loaded
      (service as any).stripe = Promise.resolve(null);

      await expectAsync(
        service.initializeElements()
      ).toBeRejectedWithError('Stripe has not been loaded');
    });
  });

  describe('createAddressElement', () => {
    it('should create an address element with default values', async () => {
      const addressElement =
        await service.createAddressElement();
      expect(addressElement).toBeTruthy();
      expect(
        mockStripe.elements().create
      ).toHaveBeenCalledWith('address', {
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
        defaultValues: {
          name: 'John Doe',
          phone: '123-456-7890',
        },
      });
    });

    it('should throw an error if elements are not initialized', async () => {
      // Simulate elements not being initialized
      spyOn(service, 'initializeElements').and.resolveTo(
        undefined
      );

      await expectAsync(
        service.createAddressElement()
      ).toBeRejectedWithError(
        'Elements instance has not been loaded'
      );
    });
  });

  describe('disposeElements', () => {
    it('should dispose of elements and addressElement', () => {
      // Initialize elements and addressElement
      (service as any).elements = {};
      (service as any).addressElement = {};

      service.disposeElements();

      expect((service as any).elements).toBeUndefined();
      expect(
        (service as any).addressElement
      ).toBeUndefined();
    });
  });
});
