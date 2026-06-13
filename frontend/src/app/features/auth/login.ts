import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  protected submit(): void {
    if (this.busy()) {
      return;
    }
    this.error.set(null);
    this.busy.set(true);
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigate(['/kamper']),
      error: (err) => {
        this.busy.set(false);
        this.error.set(err.error?.message ?? 'Innlogging feilet – prøv igjen');
      },
    });
  }
}
