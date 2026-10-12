import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ProductList } from './product-list';
import { API_BASE_URL } from '../../api/api-client';

/**
 * The NSwag generated client requests `responseType: 'blob'` and parses the JSON
 * itself, so the test has to answer with a Blob rather than a plain object.
 */
function jsonBlob(data: unknown): Blob {
  return new Blob([JSON.stringify(data)], { type: 'application/json' });
}

/** Lets the FileReader callbacks in the generated client run. */
async function settle(): Promise<void> {
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve, 0));
  }
}

describe('ProductList', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ProductList],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: API_BASE_URL, useValue: '' },
      ],
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders the products returned by the API', async () => {
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ProductList);

    // The component loads groups and products from its constructor.
    http.expectOne('/api/product-groups').flush(
      jsonBlob([{ id: 1, code: 'LAP', name: 'Laptops', description: null, productCount: 1 }]),
    );

    http.expectOne('/api/products').flush(
      jsonBlob([
        {
          id: 1,
          sku: 'LAP-001',
          name: 'Lenovo ThinkPad T14 Gen 4',
          price: 1299,
          stockQuantity: 12,
          isActive: true,
          productGroupId: 1,
          productGroupName: 'Laptops',
        },
      ]),
    );

    // The generated client reads the Blob with a FileReader, which needs a few
    // turns of the event loop before the signal is updated.
    await settle();
    await fixture.whenStable();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('LAP-001');
    expect(rows[0].textContent).toContain('Lenovo ThinkPad T14 Gen 4');
    expect(rows[0].textContent).toContain('1,299.00');
    expect(rows[0].textContent).toContain('available');

    http.verify();
  });

  it('sends the selected group as a query parameter', async () => {
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ProductList);

    http.expectOne('/api/product-groups').flush(jsonBlob([]));
    http.expectOne('/api/products').flush(jsonBlob([]));
    await fixture.whenStable();

    fixture.componentInstance['onGroupChange']('3');
    http.expectOne('/api/products?productGroupId=3').flush(jsonBlob([]));
    await fixture.whenStable();

    http.verify();
  });
});
