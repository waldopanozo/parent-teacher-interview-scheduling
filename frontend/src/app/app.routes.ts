import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import {
  directorRoleGuard,
  parentMeetingProfileGuard,
  parentRoleGuard,
  teacherRoleGuard
} from './core/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./pages/login/forgot-password.component').then((m) => m.ForgotPasswordComponent)
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: 'account/password',
        loadComponent: () =>
          import('./pages/account/account-password.component').then((m) => m.AccountPasswordComponent)
      },
      {
        path: 'parent/meeting-profile',
        canActivate: [parentRoleGuard],
        loadComponent: () =>
          import('./pages/parent/parent-meeting-profile.component').then((m) => m.ParentMeetingProfileComponent)
      },
      {
        path: 'parent/request-teacher',
        canActivate: [parentRoleGuard, parentMeetingProfileGuard],
        loadComponent: () =>
          import('./pages/parent/parent-request-teacher.component').then((m) => m.ParentRequestTeacherComponent)
      },
      {
        path: 'parent',
        canActivate: [parentRoleGuard, parentMeetingProfileGuard],
        loadComponent: () =>
          import('./pages/parent/parent-dashboard.component').then((m) => m.ParentDashboardComponent)
      },
      {
        path: 'teacher',
        canActivate: [teacherRoleGuard],
        loadComponent: () =>
          import('./pages/teacher/teacher-dashboard.component').then((m) => m.TeacherDashboardComponent)
      },
      {
        path: 'director',
        canActivate: [directorRoleGuard],
        loadComponent: () =>
          import('./pages/director/director-dashboard.component').then((m) => m.DirectorDashboardComponent)
      },
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./pages/role-redirect.component').then((m) => m.RoleRedirectComponent)
      }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
