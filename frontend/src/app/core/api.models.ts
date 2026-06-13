export type QuestionType = 'yesNo' | 'scoreGuess' | 'teamPick' | 'number';
export type MatchStatus = 'upcoming' | 'locked' | 'finished';

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
}

export interface RegisterResponse {
  id: number;
  email: string;
  success: boolean;
  message?: string;
}

export interface CurrentUser {
  email: string;
  role: string;
}

export interface QuestionBankItem {
  id: number;
  text: string;
  type: QuestionType;
  hasWildcard: boolean;
  usageCount: number;
}

export interface QuestionRequest {
  text: string;
  type: QuestionType;
  hasWildcard: boolean;
}

export interface AdminMatch {
  id: number;
  homeTeam: string;
  awayTeam: string;
  kickoffUtc: string;
  questionCount: number;
  hasAnswerKey: boolean;
  isLocked: boolean;
}

export interface MatchRequest {
  homeTeam: string;
  awayTeam: string;
  kickoffUtc: string;
}

export interface AssignmentEntry {
  questionId: number;
  orderIndex: number;
  wildcardValue?: string | null;
}

export interface MatchQuestion {
  matchQuestionId: number;
  questionId: number;
  orderIndex: number;
  text: string;
  resolvedText: string;
  type: QuestionType;
  hasWildcard: boolean;
  wildcardValue?: string | null;
  correctAnswer?: string | null;
}

export interface AnswerKeyEntry {
  matchQuestionId: number;
  correctAnswer: string;
}

export interface AnswerKeyResult {
  questionsKeyed: number;
  scoredAnswers: number;
  participants: number;
}

export interface MatchListItem {
  id: number;
  homeTeam: string;
  awayTeam: string;
  kickoffUtc: string;
  lockAtUtc: string;
  status: MatchStatus;
  answeredCount: number;
  questionCount: number;
  myPoints?: number | null;
}

export interface QuizQuestion {
  matchQuestionId: number;
  orderIndex: number;
  text: string;
  type: QuestionType;
  myAnswer?: string | null;
  correctAnswer?: string | null;
  isCorrect?: boolean | null;
}

export interface Quiz {
  matchId: number;
  homeTeam: string;
  awayTeam: string;
  kickoffUtc: string;
  lockAtUtc: string;
  isLocked: boolean;
  hasAnswerKey: boolean;
  myPoints?: number | null;
  questions: QuizQuestion[];
}

export interface SubmitAnswerEntry {
  matchQuestionId: number;
  answer: string;
}

export interface SubmitAnswersResult {
  saved: number;
}

export interface LeaderboardEntry {
  rank: number;
  name: string;
  points: number;
  matchesPlayed: number;
}

export interface ApiError {
  message: string;
}
