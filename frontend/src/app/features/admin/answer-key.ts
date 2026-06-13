import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminMatch, AnswerKeyResult, MatchQuestion } from '../../core/api.models';
import { AdminApiService } from '../../core/admin-api.service';

const SCORE_PATTERN = /^\d{1,2}-\d{1,2}$/;
const NUMBER_PATTERN = /^\d{1,3}$/;

@Component({
  selector: 'app-answer-key',
  imports: [RouterLink],
  templateUrl: './answer-key.html',
})
export class AnswerKey {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly matchId = Number(this.route.snapshot.paramMap.get('id'));
  protected readonly match = signal<AdminMatch | null>(null);
  protected readonly questions = signal<MatchQuestion[]>([]);
  protected readonly answers = signal<Record<number, string>>({});
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<AnswerKeyResult | null>(null);
  protected readonly busy = signal(false);

  protected readonly validCount = computed(
    () => this.questions().filter((q) => this.isValid(q, this.answers()[q.matchQuestionId])).length,
  );

  constructor() {
    this.api.getMatches().subscribe({
      next: (matches) => this.match.set(matches.find((m) => m.id === this.matchId) ?? null),
    });
    this.api.getMatchQuestions(this.matchId).subscribe({
      next: (questions) => {
        this.questions.set(questions);
        const initial: Record<number, string> = {};
        for (const question of questions) {
          if (question.correctAnswer) {
            initial[question.matchQuestionId] = question.correctAnswer;
          }
        }
        this.answers.set(initial);
      },
      error: () => this.error.set('Kunne ikke laste kampens spørsmål'),
    });
  }

  protected answerOf(question: MatchQuestion): string {
    return this.answers()[question.matchQuestionId] ?? '';
  }

  protected setAnswer(question: MatchQuestion, answer: string): void {
    this.answers.update((all) => ({ ...all, [question.matchQuestionId]: answer }));
    this.result.set(null);
  }

  protected scorePart(question: MatchQuestion, part: 0 | 1): string {
    return this.answerOf(question).split('-')[part] ?? '';
  }

  protected setScorePart(question: MatchQuestion, part: 0 | 1, input: HTMLInputElement): void {
    const digits = input.value.replace(/\D/g, '').slice(0, 2);
    input.value = digits;
    const parts = this.answerOf(question).split('-');
    while (parts.length < 2) {
      parts.push('');
    }
    parts[part] = digits;
    this.setAnswer(question, `${parts[0]}-${parts[1]}`);
  }

  protected setNumber(question: MatchQuestion, input: HTMLInputElement): void {
    const digits = input.value.replace(/\D/g, '').slice(0, 3);
    input.value = digits;
    this.setAnswer(question, digits);
  }

  protected save(): void {
    if (this.busy()) {
      return;
    }
    if (this.validCount() !== this.questions().length) {
      this.error.set('Fasit må fylles ut for alle spørsmålene');
      return;
    }

    this.error.set(null);
    this.busy.set(true);
    const entries = this.questions().map((q) => ({
      matchQuestionId: q.matchQuestionId,
      correctAnswer: this.answers()[q.matchQuestionId],
    }));
    this.api.setAnswerKey(this.matchId, entries).subscribe({
      next: (result) => {
        this.busy.set(false);
        this.result.set(result);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(err.error?.message ?? 'Kunne ikke lagre fasiten');
      },
    });
  }

  private isValid(question: MatchQuestion, answer: string | undefined): boolean {
    if (!answer) {
      return false;
    }
    switch (question.type) {
      case 'yesNo':
        return answer === 'yes' || answer === 'no';
      case 'teamPick':
        return answer === 'home' || answer === 'away';
      case 'scoreGuess':
        return SCORE_PATTERN.test(answer);
      case 'number':
        return NUMBER_PATTERN.test(answer);
    }
  }
}
