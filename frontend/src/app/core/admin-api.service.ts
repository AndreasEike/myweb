import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminMatch,
  AnswerKeyEntry,
  AnswerKeyResult,
  AssignmentEntry,
  MatchParticipant,
  MatchQuestion,
  MatchRequest,
  QuestionBankItem,
  QuestionRequest,
} from './api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);

  getQuestions(): Observable<QuestionBankItem[]> {
    return this.http.get<QuestionBankItem[]>('/api/admin/questions');
  }

  createQuestion(request: QuestionRequest): Observable<QuestionBankItem> {
    return this.http.post<QuestionBankItem>('/api/admin/questions', request);
  }

  updateQuestion(id: number, request: QuestionRequest): Observable<QuestionBankItem> {
    return this.http.put<QuestionBankItem>(`/api/admin/questions/${id}`, request);
  }

  deleteQuestion(id: number): Observable<unknown> {
    return this.http.delete(`/api/admin/questions/${id}`);
  }

  getMatches(): Observable<AdminMatch[]> {
    return this.http.get<AdminMatch[]>('/api/admin/matches');
  }

  createMatch(request: MatchRequest): Observable<AdminMatch> {
    return this.http.post<AdminMatch>('/api/admin/matches', request);
  }

  updateMatch(id: number, request: MatchRequest): Observable<AdminMatch> {
    return this.http.put<AdminMatch>(`/api/admin/matches/${id}`, request);
  }

  deleteMatch(id: number): Observable<unknown> {
    return this.http.delete(`/api/admin/matches/${id}`);
  }

  getMatchQuestions(matchId: number): Observable<MatchQuestion[]> {
    return this.http.get<MatchQuestion[]>(`/api/admin/matches/${matchId}/questions`);
  }

  setMatchQuestions(matchId: number, entries: AssignmentEntry[]): Observable<MatchQuestion[]> {
    return this.http.put<MatchQuestion[]>(`/api/admin/matches/${matchId}/questions`, entries);
  }

  setAnswerKey(matchId: number, entries: AnswerKeyEntry[]): Observable<AnswerKeyResult> {
    return this.http.put<AnswerKeyResult>(`/api/admin/matches/${matchId}/answer-key`, entries);
  }

  getMatchParticipants(matchId: number): Observable<MatchParticipant[]> {
    return this.http.get<MatchParticipant[]>(`/api/admin/matches/${matchId}/participants`);
  }
}
