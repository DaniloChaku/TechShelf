import { TestBed } from '@angular/core/testing';
import { HttpTestingController } from '@angular/common/http/testing';
import { CategoryService } from './category.service';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../../environments/environment';

describe('CategoryService', () => {
  let service: CategoryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CategoryService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(CategoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCategories', () => {
    it('should make a GET request to the correct URL', () => {
      const mockCategories = [
        { id: 1, name: 'Category 1' },
        { id: 2, name: 'Category 2' },
      ];

      service.getCategories().subscribe((categories) => {
        expect(categories).toEqual(mockCategories);
      });

      const req = httpMock.expectOne(
        environment.apiUrl + 'categories'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockCategories);
    });

    it('should handle errors appropriately', () => {
      const errorMessage = 'Server error';

      service.getCategories().subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        environment.apiUrl + 'categories'
      );
      req.flush('Server error', {
        status: 500,
        statusText: errorMessage,
      });
    });
  });
});
