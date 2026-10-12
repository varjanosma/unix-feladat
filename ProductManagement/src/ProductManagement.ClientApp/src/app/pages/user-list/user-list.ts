import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UserListItemDto, UsersClient } from '../../api/api-client';

@Component({
  selector: 'app-user-list',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './user-list.html',
})
export class UserList {
  private readonly usersClient = inject(UsersClient);

  protected readonly users = signal<UserListItemDto[]>([]);
  protected readonly error = signal<string | undefined>(undefined);

  constructor() {
    this.usersClient.getList().subscribe({
      next: (users) => this.users.set(users),
      error: (err) => this.error.set(err?.message ?? 'Unknown error'),
    });
  }
}
