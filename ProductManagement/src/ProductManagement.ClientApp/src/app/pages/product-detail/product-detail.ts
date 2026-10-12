import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductDetailDto, ProductsClient } from '../../api/api-client';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './product-detail.html',
})
export class ProductDetail {
  private readonly productsClient = inject(ProductsClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly product = signal<ProductDetailDto | undefined>(undefined);
  protected readonly notFound = signal(false);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.route.paramMap.subscribe((params) => this.load(Number(params.get('id'))));
  }

  private load(id: number): void {
    this.product.set(undefined);
    this.notFound.set(false);
    this.error.set(undefined);

    this.productsClient.getById(id).subscribe({
      next: (product) => this.product.set(product),
      error: (err) => {
        // The API answers with 404 when there is no such product - that is not the
        // same as a failed request, and the UI can tell the two apart.
        if (err?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(err?.message ?? 'Unknown error');
        }
      },
    });
  }
}
