import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PurchaseDetailDto, PurchasesClient } from '../../api/api-client';

@Component({
  selector: 'app-purchase-detail',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './purchase-detail.html',
})
export class PurchaseDetail {
  private readonly purchasesClient = inject(PurchasesClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly purchase = signal<PurchaseDetailDto | undefined>(undefined);
  protected readonly notFound = signal(false);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.route.paramMap.subscribe((params) => this.load(Number(params.get('id'))));
  }

  private load(id: number): void {
    this.purchase.set(undefined);
    this.notFound.set(false);
    this.error.set(undefined);

    this.purchasesClient.getById(id).subscribe({
      next: (purchase) => this.purchase.set(purchase),
      error: (err) => {
        // 404 means "no such purchase", which is a normal answer, not a failure.
        if (err?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(err?.message ?? 'Unknown error');
        }
      },
    });
  }
}
