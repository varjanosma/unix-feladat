import { Component, inject, signal } from '@angular/core';
import { ProductGroupDto, ProductGroupsClient } from '../../api/api-client';

@Component({
  selector: 'app-product-group-list',
  templateUrl: './product-group-list.html',
})
export class ProductGroupList {
  private readonly productGroupsClient = inject(ProductGroupsClient);

  protected readonly groups = signal<ProductGroupDto[]>([]);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.productGroupsClient.getAll().subscribe({
      next: (groups) => this.groups.set(groups),
      error: (err) => this.error.set(err?.message ?? 'Unknown error'),
    });
  }
}
