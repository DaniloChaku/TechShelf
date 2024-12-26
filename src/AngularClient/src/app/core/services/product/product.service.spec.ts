import { HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../../environments/environment';
import { TestBed } from '@angular/core/testing';
import { ProductService } from './product.service';
import { SearchProductsRequest } from './search-products-request';
import { PagedResult } from '../../models/pagedResult';
import { Product } from '../../models/product';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getProducts', () => {
    it('should make a GET request to the correct URL with the correct parameters', () => {
      const mockProducts: PagedResult<Product> = {
        items: [
          {
            id: 1,
            name: 'Product 1',
            description: 'product 1',
            price: 100,
            categoryId: 1,
            brandId: 1,
            stock: 1,
            thumbnailUrl: '',
            imageUrls: [],
          },
          {
            id: 2,
            name: 'Product 2',
            description: 'product 2',
            price: 200,
            categoryId: 1,
            brandId: 1,
            stock: 1,
            thumbnailUrl: '',
            imageUrls: [],
          },
        ],
        totalCount: 2,
        pageIndex: 1,
        pageSize: 5,
      };

      const request: SearchProductsRequest = {
        pageIndex: 1,
        pageSize: 10,
        brandId: 1,
        categoryId: 2,
        name: 'Product',
        minPrice: 10,
        maxPrice: 100,
        sortBy: 'name',
        isDescending: true,
      };

      service.getProducts(request).subscribe((products) => {
        expect(products).toEqual(mockProducts);
      });

      const req = httpMock.expectOne((req) => {
        return (
          req.url ===
            environment.apiUrl + 'products/search' &&
          req.params.get('pageIndex') ===
            request.pageIndex.toString() &&
          req.params.get('pageSize') ===
            request.pageSize.toString() &&
          req.params.get('brandId') ===
            request.brandId!.toString() &&
          req.params.get('categoryId') ===
            request.categoryId!.toString() &&
          req.params.get('name') === request.name &&
          req.params.get('minPrice') ===
            request.minPrice!.toString() &&
          req.params.get('maxPrice') ===
            request.maxPrice!.toString() &&
          req.params.get('sortBy') === request.sortBy &&
          req.params.get('isDescending') ===
            request.isDescending!.toString()
        );
      });
      expect(req.request.method).toBe('GET');
      req.flush(mockProducts);
    });

    it('should handle errors appropriately', () => {
      const errorMessage = 'Server error';

      const request: SearchProductsRequest = {
        pageIndex: 1,
        pageSize: 10,
      };

      service.getProducts(request).subscribe({
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        (req) =>
          req.url ===
            environment.apiUrl + 'products/search' &&
          req.params.get('pageIndex') ===
            request.pageIndex!.toString() &&
          req.params.get('pageSize') ===
            request.pageSize!.toString()
      );
      req.flush('Server error', {
        status: 500,
        statusText: errorMessage,
      });
    });
  });

  describe('getProductById', () => {
    it('should make a GET request to the correct URL with the product ID', () => {
      const mockProduct: Product = {
        id: 1,
        name: 'Product 1',
        description: 'product 1',
        price: 100,
        categoryId: 1,
        brandId: 1,
        stock: 1,
        thumbnailUrl: '',
        imageUrls: [],
      };

      service.getProductById(1).subscribe((product) => {
        expect(product).toEqual(mockProduct);
      });

      const req = httpMock.expectOne(
        environment.apiUrl + 'products/1'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockProduct);
    });

    it('should handle errors when fetching a product by ID', () => {
      const errorMessage = 'Product not found';
      const productId = 999;

      service.getProductById(productId).subscribe({
        error: (error) => {
          expect(error.status).toBe(404);
          expect(error.statusText).toBe(errorMessage);
        },
      });

      const req = httpMock.expectOne(
        environment.apiUrl + 'products/999'
      );
      expect(req.request.method).toBe('GET');
      req.flush('Product not found', {
        status: 404,
        statusText: errorMessage,
      });
    });
  });
});
