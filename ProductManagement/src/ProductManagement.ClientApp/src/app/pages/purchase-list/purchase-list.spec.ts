import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { PurchaseList } from './purchase-list';
import { API_BASE_URL } from '../../api/api-client';

/** The generated client requests `responseType: 'blob'` and parses the JSON itself. */
function jsonBlob(data: unknown): Blob {
  return new Blob([JSON.stringify(data)], { type: 'application/json' });
}

/** Lets the FileReader callbacks in the generated client run. */
async function settle(): Promise<void> {
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve, 0));
  }
}

describe('PurchaseList', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PurchaseList],
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

  it('renders the purchases and sums them up', async () => {
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PurchaseList);

    http.expectOne('/api/users').flush(jsonBlob([]));
    http.expectOne('/api/purchases').flush(
      jsonBlob([
        {
          id: 5,
          purchasedAtUtc: '2026-03-15T10:30:00Z',
          note: 'Winter set',
          userId: 2,
          userFullName: 'Kiss Anna',
          itemCount: 2,
          totalQuantity: 5,
          total: 778,
        },
        {
          id: 2,
          purchasedAtUtc: '2025-09-18T16:40:00Z',
          note: null,
          userId: 1,
          userFullName: 'Novak Janos',
          itemCount: 1,
          totalQuantity: 1,
          total: 89,
        },
      ]),
    );

    await settle();
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const rows = element.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Kiss Anna');
    expect(rows[0].textContent).toContain('Winter set');
    expect(rows[0].textContent).toContain('778.00');
    expect(rows[1].textContent).toContain('no note');

    // 778 + 89
    expect(element.querySelector('tfoot')?.textContent).toContain('867.00');

    http.verify();
  });

  it('sends the selected customer as a query parameter', async () => {
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PurchaseList);

    http.expectOne('/api/users').flush(jsonBlob([]));
    http.expectOne('/api/purchases').flush(jsonBlob([]));
    await settle();
    await fixture.whenStable();

    fixture.componentInstance['onUserChange']('2');
    http.expectOne('/api/purchases?userId=2').flush(jsonBlob([]));
    await settle();

    http.verify();
  });
});
