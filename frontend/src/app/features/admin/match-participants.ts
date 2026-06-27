import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminMatch, MatchParticipant } from '../../core/api.models';
import { AdminApiService } from '../../core/admin-api.service';

@Component({
  selector: 'app-match-participants',
  imports: [RouterLink, DatePipe],
  templateUrl: './match-participants.html',
})
export class MatchParticipants {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly matchId = Number(this.route.snapshot.paramMap.get('id'));
  protected readonly match = signal<AdminMatch | null>(null);
  protected readonly participants = signal<MatchParticipant[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.getMatches().subscribe({
      next: (matches) => this.match.set(matches.find((m) => m.id === this.matchId) ?? null),
    });
    this.api.getMatchParticipants(this.matchId).subscribe({
      next: (participants) => {
        this.participants.set(participants);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message ?? 'Kunne ikke laste deltakerne');
      },
    });
  }
}
