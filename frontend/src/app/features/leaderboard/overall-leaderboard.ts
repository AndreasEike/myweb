import { Component, inject, signal } from '@angular/core';
import { LeaderboardEntry } from '../../core/api.models';
import { AuthService } from '../../core/auth.service';
import { QuizApiService } from '../../core/quiz-api.service';

@Component({
  selector: 'app-overall-leaderboard',
  templateUrl: './overall-leaderboard.html',
})
export class OverallLeaderboard {
  private readonly api = inject(QuizApiService);
  protected readonly auth = inject(AuthService);

  protected readonly entries = signal<LeaderboardEntry[] | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly myName = this.auth.currentUser()?.email.split('@')[0] ?? '';

  constructor() {
    this.api.getOverallLeaderboard().subscribe({
      next: (entries) => this.entries.set(entries),
      error: () => this.error.set('Kunne ikke laste topplisten'),
    });
  }
}
