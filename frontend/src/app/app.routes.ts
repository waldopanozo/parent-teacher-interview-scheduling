import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { parentRoleGuard, teacherRoleGuard } from './core/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: 'parent',
        canActivate: [parentRoleGuard],
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
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./pages/role-redirect.component').then((m) => m.RoleRedirectComponent)
      }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
