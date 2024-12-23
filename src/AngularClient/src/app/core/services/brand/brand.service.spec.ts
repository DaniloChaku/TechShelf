import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { BrandService } from './brand.service';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('BrandService', () => {
  let service: BrandService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        BrandService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(BrandService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Verify that no unmatched requests are outstanding
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getBrands', () => {
    it('should make a GET request to the correct URL', () => {
      const mockBrands = [
        { id: 1, name: 'Brand 1' },
        { id: 2, name: 'Brand 2' },
      ];

      service.getBrands().subscribe((brands) => {
        expect(brands).toEqual(mockBrands);
      });

      const req = httpMock.expectOne(
        'https://localhost:7281/api/brands'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockBrands);
    });

    it('should handle errors appropriately', () => {
      const errorMessage = 'Server error';

      service.getBrands().subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        'https://localhost:7281/api/brands'
      );
      req.flush('Server error', {
        status: 500,
        statusText: errorMessage,
      });
    });
  });
});
