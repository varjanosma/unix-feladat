import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  PurchaseListItemDto,
  PurchasesClient,
  UserListItemDto,
  UsersClient,
} from '../../api/api-client';

@Component({
  selector: 'app-purchase-list',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './purchase-list.html',
})
export class PurchaseList {
  private readonly purchasesClient = inject(PurchasesClient);
  private readonly usersClient = inject(UsersClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly purchases = signal<PurchaseListItemDto[]>([]);
  protected readonly users = signal<UserListItemDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | undefined>(undefined);

  protected userId: number | undefined;

  constructor() {
    this.usersClient.getList().subscribe({
      next: (users) => this.users.set(users),
      error: () => this.users.set([]),
    });

    // The user filter can also arrive from the URL (?userId=2), so a link from the
    // user page lands on a pre-filtered list.
    this.route.queryParamMap.subscribe((params) => {
      const raw = params.get('userId');
      this.userId = raw ? Number(raw) : undefined;
      this.load();
    });
  }

  protected onUserChange(value: string): void {
    this.userId = value ? Number(value) : undefined;
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(undefined);

    this.purchasesClient.getList(this.userId).subscribe({
      next: (purchases) => {
        this.purchases.set(purchases);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Unknown error');
        this.loading.set(false);
      },
    });
  }

  protected grandTotal(): number {
    return this.purchases().reduce((sum, p) => sum + (p.total ?? 0), 0);
  }
}
