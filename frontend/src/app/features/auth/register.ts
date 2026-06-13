import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected passwordRepeat = '';
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  protected submit(): void {
    if (this.busy()) {
      return;
    }
    if (this.password !== this.passwordRepeat) {
      this.error.set('Passordene er ikke like');
      return;
    }
    if (this.password.length < 8) {
      this.error.set('Passordet må ha minst 8 tegn');
      return;
    }
    this.error.set(null);
    this.busy.set(true);
    const email = this.email.trim();
    this.auth.register(email, this.password).subscribe({
      next: () => {
        // Logg inn automatisk etter registrering
        this.auth.login(email, this.password).subscribe({
          next: () => this.router.navigate(['/kamper']),
          error: () => this.router.navigate(['/logg-inn']),
        });
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(err.error?.message ?? 'Registrering feilet – prøv igjen');
      },
    });
  }
}
