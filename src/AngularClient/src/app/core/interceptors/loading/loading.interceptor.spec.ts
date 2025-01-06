import { TestBed } from '@angular/core/testing';
import {
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';

import { loadingInterceptor } from './loading.interceptor';
import { LoadingService } from '../../services/loading.service';
import { map, of } from 'rxjs';

describe('loadingInterceptor', () => {
  let loadingService: jasmine.SpyObj<LoadingService>;
  const interceptor: HttpInterceptorFn = (req, next) =>
    TestBed.runInInjectionContext(() =>
      loadingInterceptor(req, next)
    );

  beforeEach(() => {
    loadingService = jasmine.createSpyObj(
      'LoadingService',
      ['addRequest', 'removeRequest']
    );

    TestBed.configureTestingModule({
      providers: [
        {
          provide: LoadingService,
          useValue: loadingService,
        },
      ],
    });
  });

  it('should add request to loading service when request starts', (done) => {
    // Arrange
    const req = new HttpRequest('GET', '/test');
    const next: HttpHandlerFn = (request) => of();

    // Act
    interceptor(req, next).subscribe({
      complete: () => {
        // Assert
        expect(
          loadingService.addRequest
        ).toHaveBeenCalledTimes(1);
        done();
      },
    });
  });

  it('should remove request from loading service when request completes successfully', (done) => {
    // Arrange
    const req = new HttpRequest('GET', '/test');
    const next: HttpHandlerFn = (request) => of();

    // Act
    interceptor(req, next).subscribe({
      complete: () => {
        done();
      },
    });

    // Assert
    expect(
      loadingService.removeRequest
    ).toHaveBeenCalledTimes(1);
  });

  it('should remove request from loading service when request fails', (done) => {
    // Arrange
    const req = new HttpRequest('GET', '/test');
    const next: HttpHandlerFn = (request) =>
      of(new Response()).pipe(
        map(() => {
          throw new Error('Test error');
        })
      );

    // Act
    interceptor(req, next).subscribe({
      error: () => {
        done();
      },
    });

    // Assert
    expect(
      loadingService.removeRequest
    ).toHaveBeenCalledTimes(1);
  });

  it('should properly chain multiple requests', (done) => {
    // Arrange
    const req = new HttpRequest('GET', '/test');
    const next: HttpHandlerFn = (request) => of();

    // Act
    interceptor(req, next).subscribe();
    interceptor(req, next).subscribe({
      complete: () => {
        done();
      },
    });

    // Assert
    expect(loadingService.addRequest).toHaveBeenCalledTimes(
      2
    );
    expect(
      loadingService.removeRequest
    ).toHaveBeenCalledTimes(2);
  });
});
