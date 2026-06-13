import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  LeaderboardEntry,
  MatchListItem,
  Quiz,
  SubmitAnswerEntry,
  SubmitAnswersResult,
} from './api.models';

@Injectable({ providedIn: 'root' })
export class QuizApiService {
  private readonly http = inject(HttpClient);

  getMatches(): Observable<MatchListItem[]> {
    return this.http.get<MatchListItem[]>('/api/matches');
  }

  getQuiz(matchId: number): Observable<Quiz> {
    return this.http.get<Quiz>(`/api/matches/${matchId}/quiz`);
  }

  submitAnswers(matchId: number, answers: SubmitAnswerEntry[]): Observable<SubmitAnswersResult> {
    return this.http.put<SubmitAnswersResult>(`/api/matches/${matchId}/answers`, answers);
  }

  getMatchLeaderboard(matchId: number): Observable<LeaderboardEntry[]> {
    return this.http.get<LeaderboardEntry[]>(`/api/matches/${matchId}/leaderboard`);
  }

  getOverallLeaderboard(): Observable<LeaderboardEntry[]> {
    return this.http.get<LeaderboardEntry[]>('/api/leaderboard');
  }
}
