import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  ProductGroupDto,
  ProductGroupsClient,
  ProductListItemDto,
  ProductsClient,
} from '../../api/api-client';

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './product-list.html',
})
export class ProductList {
  private readonly productsClient = inject(ProductsClient);
  private readonly productGroupsClient = inject(ProductGroupsClient);

  protected readonly products = signal<ProductListItemDto[]>([]);
  protected readonly groups = signal<ProductGroupDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | undefined>(undefined);

  protected groupId: number | undefined;
  protected search = '';

  constructor() {
    this.productGroupsClient.getAll().subscribe({
      next: (groups) => this.groups.set(groups),
      error: () => this.groups.set([]),
    });

    this.load();
  }

  protected onGroupChange(value: string): void {
    this.groupId = value ? Number(value) : undefined;
    this.load();
  }

  protected onSearchChange(value: string): void {
    this.search = value;
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(undefined);

    // The generated client takes the query parameters as plain arguments.
    this.productsClient.getList(this.groupId, this.search || undefined).subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Unknown error');
        this.loading.set(false);
      },
    });
  }
}
