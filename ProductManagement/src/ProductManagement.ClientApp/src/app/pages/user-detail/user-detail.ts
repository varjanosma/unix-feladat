import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { UserDetailDto, UsersClient } from '../../api/api-client';

@Component({
  selector: 'app-user-detail',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './user-detail.html',
})
export class UserDetail {
  private readonly usersClient = inject(UsersClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly user = signal<UserDetailDto | undefined>(undefined);
  protected readonly notFound = signal(false);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.route.paramMap.subscribe((params) => this.load(Number(params.get('id'))));
  }

  private load(id: number): void {
    this.user.set(undefined);
    this.notFound.set(false);
    this.error.set(undefined);

    this.usersClient.getById(id).subscribe({
      next: (user) => this.user.set(user),
      error: (err) => {
        // 404 means "no such user", which is a normal answer, not a failure.
        if (err?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(err?.message ?? 'Unknown error');
        }
      },
    });
  }
}
