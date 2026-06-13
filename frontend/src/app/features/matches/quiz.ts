import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Quiz, QuizQuestion, SubmitAnswerEntry } from '../../core/api.models';
import { QuizApiService } from '../../core/quiz-api.service';

const SCORE_PATTERN = /^\d{1,2}-\d{1,2}$/;
const NUMBER_PATTERN = /^\d{1,3}$/;

@Component({
  selector: 'app-quiz',
  imports: [RouterLink, DatePipe],
  templateUrl: './quiz.html',
})
export class QuizPage {
  private readonly api = inject(QuizApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly quiz = signal<Quiz | null>(null);
  protected readonly answers = signal<Record<number, string>>({});
  protected readonly error = signal<string | null>(null);
  protected readonly saved = signal(false);
  protected readonly busy = signal(false);

  protected readonly validCount = computed(() => {
    const quiz = this.quiz();
    if (!quiz) {
      return 0;
    }
    return quiz.questions.filter((q) => this.isValid(q, this.answers()[q.matchQuestionId])).length;
  });

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getQuiz(id).subscribe({
      next: (quiz) => {
        this.quiz.set(quiz);
        const initial: Record<number, string> = {};
        for (const question of quiz.questions) {
          if (question.myAnswer) {
            initial[question.matchQuestionId] = question.myAnswer;
          }
        }
        this.answers.set(initial);
      },
      error: () => this.error.set('Kunne ikke laste quizen'),
    });
  }

  protected answerOf(question: QuizQuestion): string {
    return this.answers()[question.matchQuestionId] ?? '';
  }

  protected setAnswer(question: QuizQuestion, answer: string): void {
    if (this.quiz()?.isLocked) {
      return;
    }
    this.answers.update((all) => ({ ...all, [question.matchQuestionId]: answer }));
    this.saved.set(false);
  }

  protected scorePart(question: QuizQuestion, part: 0 | 1): string {
    return this.answerOf(question).split('-')[part] ?? '';
  }

  protected setScorePart(question: QuizQuestion, part: 0 | 1, input: HTMLInputElement): void {
    const digits = input.value.replace(/\D/g, '').slice(0, 2);
    input.value = digits;
    const parts = this.answerOf(question).split('-');
    while (parts.length < 2) {
      parts.push('');
    }
    parts[part] = digits;
    this.setAnswer(question, `${parts[0]}-${parts[1]}`);
  }

  protected setNumber(question: QuizQuestion, input: HTMLInputElement): void {
    const digits = input.value.replace(/\D/g, '').slice(0, 3);
    input.value = digits;
    this.setAnswer(question, digits);
  }

  protected save(): void {
    const quiz = this.quiz();
    if (!quiz || this.busy()) {
      return;
    }
    const entries: SubmitAnswerEntry[] = quiz.questions
      .map((q) => ({ matchQuestionId: q.matchQuestionId, answer: this.answers()[q.matchQuestionId] ?? '' }))
      .filter((entry, index) => this.isValid(quiz.questions[index], entry.answer));
    if (entries.length === 0) {
      this.error.set('Svar på minst ett spørsmål før du lagrer');
      return;
    }

    this.error.set(null);
    this.busy.set(true);
    this.api.submitAnswers(quiz.matchId, entries).subscribe({
      next: () => {
        this.busy.set(false);
        this.saved.set(true);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(err.error?.message ?? 'Kunne ikke lagre svarene');
      },
    });
  }

  protected formatAnswer(question: QuizQuestion, answer: string | null | undefined): string {
    const quiz = this.quiz();
    if (!answer || !quiz) {
      return '–';
    }
    switch (question.type) {
      case 'yesNo':
        return answer === 'yes' ? 'Ja' : 'Nei';
      case 'teamPick':
        return answer === 'home' ? quiz.homeTeam : quiz.awayTeam;
      default:
        return answer;
    }
  }

  private isValid(question: QuizQuestion, answer: string | undefined): boolean {
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
