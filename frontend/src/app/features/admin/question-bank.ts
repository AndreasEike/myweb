import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QuestionBankItem, QuestionType } from '../../core/api.models';
import { AdminApiService } from '../../core/admin-api.service';

@Component({
  selector: 'app-question-bank',
  imports: [FormsModule],
  templateUrl: './question-bank.html',
})
export class QuestionBank {
  private readonly api = inject(AdminApiService);

  protected readonly questions = signal<QuestionBankItem[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<number | null>(null);

  protected text = '';
  protected type: QuestionType = 'yesNo';
  protected hasWildcard = false;

  protected readonly typeLabels: Record<QuestionType, string> = {
    yesNo: 'Ja/Nei',
    scoreGuess: 'Resultat',
    teamPick: 'Lagvalg',
    number: 'Antall',
  };

  constructor() {
    this.reload();
  }

  protected startEdit(question: QuestionBankItem): void {
    this.editingId.set(question.id);
    this.text = question.text;
    this.type = question.type;
    this.hasWildcard = question.hasWildcard;
    this.error.set(null);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
    this.text = '';
    this.type = 'yesNo';
    this.hasWildcard = false;
    this.error.set(null);
  }

  protected submit(): void {
    const request = { text: this.text.trim(), type: this.type, hasWildcard: this.hasWildcard };
    const id = this.editingId();
    const call = id === null ? this.api.createQuestion(request) : this.api.updateQuestion(id, request);
    call.subscribe({
      next: () => {
        this.cancelEdit();
        this.reload();
      },
      error: (err) => this.error.set(err.error?.message ?? 'Kunne ikke lagre spørsmålet'),
    });
  }

  protected remove(question: QuestionBankItem): void {
    if (!confirm(`Slette «${question.text}»?`)) {
      return;
    }
    this.api.deleteQuestion(question.id).subscribe({
      next: () => this.reload(),
      error: (err) => this.error.set(err.error?.message ?? 'Kunne ikke slette spørsmålet'),
    });
  }

  private reload(): void {
    this.api.getQuestions().subscribe({
      next: (questions) => this.questions.set(questions),
      error: () => this.error.set('Kunne ikke laste spørsmålsbanken'),
    });
  }
}
