import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'logg-inn',
    loadComponent: () => import('./features/auth/login').then((m) => m.Login),
  },
  {
    path: 'registrer',
    loadComponent: () => import('./features/auth/register').then((m) => m.Register),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shell/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'kamper' },
      {
        path: 'kamper',
        loadComponent: () => import('./features/matches/match-list').then((m) => m.MatchList),
      },
      {
        path: 'kamper/:id/quiz',
        loadComponent: () => import('./features/matches/quiz').then((m) => m.QuizPage),
      },
      {
        path: 'kamper/:id/resultater',
        loadComponent: () =>
          import('./features/leaderboard/match-leaderboard').then((m) => m.MatchLeaderboard),
      },
      {
        path: 'toppliste',
        loadComponent: () =>
          import('./features/leaderboard/overall-leaderboard').then((m) => m.OverallLeaderboard),
      },
      {
        path: 'admin',
        canActivate: [adminGuard],
        children: [
          {
            path: 'sporsmalsbank',
            loadComponent: () =>
              import('./features/admin/question-bank').then((m) => m.QuestionBank),
          },
          {
            path: 'kamper',
            loadComponent: () => import('./features/admin/match-admin').then((m) => m.MatchAdmin),
          },
          {
            path: 'kamper/:id',
            loadComponent: () => import('./features/admin/match-setup').then((m) => m.MatchSetup),
          },
          {
            path: 'kamper/:id/fasit',
            loadComponent: () => import('./features/admin/answer-key').then((m) => m.AnswerKey),
          },
        ],
      },
    ],
  },
  { path: '**', redirectTo: 'kamper' },
];
