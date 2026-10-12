import { Component, inject, signal } from '@angular/core';
import { HealthClient, HealthResponse } from '../../api/api-client';

@Component({
  selector: 'app-health',
  templateUrl: './health.html',
})
export class Health {
  private readonly healthClient = inject(HealthClient);

  protected readonly health = signal<HealthResponse | undefined>(undefined);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.check();
  }

  protected check(): void {
    this.health.set(undefined);
    this.error.set(undefined);

    this.healthClient.get().subscribe({
      next: (response) => this.health.set(response),
      error: (err) => this.error.set(err?.message ?? 'Unknown error'),
    });
  }
}
