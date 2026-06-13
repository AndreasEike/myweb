import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminMatch, QuestionBankItem, QuestionType } from '../../core/api.models';
import { AdminApiService } from '../../core/admin-api.service';

interface SelectedQuestion {
  questionId: number;
  text: string;
  type: QuestionType;
  hasWildcard: boolean;
  wildcardValue: string;
}

@Component({
  selector: 'app-match-setup',
  imports: [FormsModule, RouterLink],
  templateUrl: './match-setup.html',
})
export class MatchSetup {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly matchId = Number(this.route.snapshot.paramMap.get('id'));
  protected readonly match = signal<AdminMatch | null>(null);
  protected readonly bank = signal<QuestionBankItem[]>([]);
  protected readonly selected = signal<SelectedQuestion[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly saved = signal(false);
  protected readonly busy = signal(false);

  protected readonly typeLabels: Record<QuestionType, string> = {
    yesNo: 'Ja/Nei',
    scoreGuess: 'Resultat',
    teamPick: 'Lagvalg',
    number: 'Antall',
  };

  protected readonly selectedIds = computed(
    () => new Set(this.selected().map((s) => s.questionId)),
  );

  constructor() {
    this.api.getMatches().subscribe({
      next: (matches) => this.match.set(matches.find((m) => m.id === this.matchId) ?? null),
    });
    this.api.getQuestions().subscribe({
      next: (questions) => this.bank.set(questions),
      error: () => this.error.set('Kunne ikke laste spørsmålsbanken'),
    });
    this.api.getMatchQuestions(this.matchId).subscribe({
      next: (assigned) =>
        this.selected.set(
          assigned.map((mq) => ({
            questionId: mq.questionId,
            text: mq.text,
            type: mq.type,
            hasWildcard: mq.hasWildcard,
            wildcardValue: mq.wildcardValue ?? '',
          })),
        ),
      error: () => this.error.set('Kunne ikke laste kampens spørsmål'),
    });
  }

  protected add(question: QuestionBankItem): void {
    if (this.selected().length >= 20) {
      return;
    }
    if (!question.hasWildcard && this.selectedIds().has(question.id)) {
      return;
    }
    this.selected.update((list) => [
      ...list,
      {
        questionId: question.id,
        text: question.text,
        type: question.type,
        hasWildcard: question.hasWildcard,
        wildcardValue: '',
      },
    ]);
    this.saved.set(false);
  }

  protected remove(index: number): void {
    this.selected.update((list) => list.filter((_, i) => i !== index));
    this.saved.set(false);
  }

  protected move(index: number, delta: -1 | 1): void {
    const target = index + delta;
    this.selected.update((list) => {
      if (target < 0 || target >= list.length) {
        return list;
      }
      const next = [...list];
      [next[index], next[target]] = [next[target], next[index]];
      return next;
    });
    this.saved.set(false);
  }

  protected setWildcard(index: number, value: string): void {
    this.selected.update((list) =>
      list.map((item, i) => (i === index ? { ...item, wildcardValue: value } : item)),
    );
    this.saved.set(false);
  }

  protected save(): void {
    if (this.busy()) {
      return;
    }
    const missing = this.selected().findIndex((s) => s.hasWildcard && !s.wildcardValue.trim());
    if (missing >= 0) {
      this.error.set(`Fyll inn spillernavn for spørsmål ${missing + 1}`);
      return;
    }

    this.error.set(null);
    this.busy.set(true);
    const entries = this.selected().map((s, index) => ({
      questionId: s.questionId,
      orderIndex: index + 1,
      wildcardValue: s.hasWildcard ? s.wildcardValue.trim() : null,
    }));
    this.api.setMatchQuestions(this.matchId, entries).subscribe({
      next: () => {
        this.busy.set(false);
        this.saved.set(true);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(err.error?.message ?? 'Kunne ikke lagre spørsmålene');
      },
    });
  }
}
