import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LeaderboardEntry, MatchListItem } from '../../core/api.models';
import { AuthService } from '../../core/auth.service';
import { QuizApiService } from '../../core/quiz-api.service';

@Component({
  selector: 'app-match-leaderboard',
  imports: [RouterLink],
  templateUrl: './match-leaderboard.html',
})
export class MatchLeaderboard {
  private readonly api = inject(QuizApiService);
  private readonly route = inject(ActivatedRoute);
  protected readonly auth = inject(AuthService);

  protected readonly entries = signal<LeaderboardEntry[] | null>(null);
  protected readonly match = signal<MatchListItem | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly myName = this.auth.currentUser()?.email.split('@')[0] ?? '';

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getMatchLeaderboard(id).subscribe({
      next: (entries) => this.entries.set(entries),
      error: (err) => this.error.set(err.error?.message ?? 'Kunne ikke laste resultatene'),
    });
    this.api.getMatches().subscribe({
      next: (matches) => this.match.set(matches.find((m) => m.id === id) ?? null),
    });
  }
}
