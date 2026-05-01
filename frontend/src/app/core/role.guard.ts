import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const parentRoleGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.roleName() === 'Parent') return true;
  if (auth.roleName() === 'Teacher') {
    void router.navigateByUrl('/app/teacher');
    return false;
  }
  void router.navigateByUrl('/login');
  return false;
};

export const teacherRoleGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.roleName() === 'Teacher') return true;
  if (auth.roleName() === 'Parent') {
    void router.navigateByUrl('/app/parent');
    return false;
  }
  void router.navigateByUrl('/login');
  return false;
};
