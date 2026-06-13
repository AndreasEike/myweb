import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminMatch } from '../../core/api.models';
import { AdminApiService } from '../../core/admin-api.service';

@Component({
  selector: 'app-match-admin',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './match-admin.html',
})
export class MatchAdmin {
  private readonly api = inject(AdminApiService);

  protected readonly matches = signal<AdminMatch[]>([]);
  protected readonly error = signal<string | null>(null);

  protected homeTeam = '';
  protected awayTeam = '';
  protected kickoffDate = '';
  protected kickoffHour = '18';
  protected kickoffMinute = '00';

  protected readonly hours = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'));
  protected readonly minutes = Array.from({ length: 12 }, (_, i) => String(i * 5).padStart(2, '0'));

  constructor() {
    this.reload();
  }

  protected create(): void {
    if (!this.kickoffDate) {
      this.error.set('Velg avsparkdato');
      return;
    }
    const kickoffLocal = `${this.kickoffDate}T${this.kickoffHour}:${this.kickoffMinute}`;
    this.api
      .createMatch({
        homeTeam: this.homeTeam.trim(),
        awayTeam: this.awayTeam.trim(),
        kickoffUtc: new Date(kickoffLocal).toISOString(),
      })
      .subscribe({
        next: () => {
          this.homeTeam = '';
          this.awayTeam = '';
          this.kickoffDate = '';
          this.kickoffHour = '18';
          this.kickoffMinute = '00';
          this.error.set(null);
          this.reload();
        },
        error: (err) => this.error.set(err.error?.message ?? 'Kunne ikke opprette kampen'),
      });
  }

  protected remove(match: AdminMatch): void {
    if (!confirm(`Slette ${match.homeTeam} – ${match.awayTeam}? Alle svar slettes også.`)) {
      return;
    }
    this.api.deleteMatch(match.id).subscribe({
      next: () => this.reload(),
      error: (err) => this.error.set(err.error?.message ?? 'Kunne ikke slette kampen'),
    });
  }

  private reload(): void {
    this.api.getMatches().subscribe({
      next: (matches) => this.matches.set(matches),
      error: () => this.error.set('Kunne ikke laste kampene'),
    });
  }
}
