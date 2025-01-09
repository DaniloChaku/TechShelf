import { TestBed } from '@angular/core/testing';

import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with isLoading as false', () => {
    expect(service.isLoading()).toBeFalse();
  });

  describe('addRequest', () => {
    it('should set isLoading to true when adding first request', () => {
      service.addRequest();
      expect(service.isLoading()).toBeTrue();
    });

    it('should keep isLoading true when adding multiple requests', () => {
      service.addRequest();
      service.addRequest();
      expect(service.isLoading()).toBeTrue();
    });
  });

  describe('removeRequest', () => {
    it('should keep isLoading true when removing request but others exist', () => {
      service.addRequest();
      service.addRequest();
      service.removeRequest();
      expect(service.isLoading()).toBeTrue();
    });

    it('should set isLoading to false when removing last request', () => {
      service.addRequest();
      service.removeRequest();
      expect(service.isLoading()).toBeFalse();
    });

    it('should handle multiple removeRequest calls when no requests exist', () => {
      service.removeRequest();
      service.removeRequest();
      expect(service.isLoading()).toBeFalse();
    });

    it('should maintain correct loading state through multiple add/remove cycles', () => {
      service.addRequest();
      service.addRequest();
      service.removeRequest();
      expect(service.isLoading()).toBeTrue();
      service.removeRequest();
      expect(service.isLoading()).toBeFalse();
    });
  });
});
