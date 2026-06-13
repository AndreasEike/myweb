import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatchListItem } from '../../core/api.models';
import { QuizApiService } from '../../core/quiz-api.service';

@Component({
  selector: 'app-match-list',
  imports: [RouterLink, DatePipe],
  templateUrl: './match-list.html',
})
export class MatchList {
  private readonly api = inject(QuizApiService);

  protected readonly matches = signal<MatchListItem[] | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.getMatches().subscribe({
      next: (matches) => this.matches.set(matches),
      error: () => this.error.set('Kunne ikke laste kampene'),
    });
  }
}
