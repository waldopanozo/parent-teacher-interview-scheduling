import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

function homeForRole(role: ReturnType<AuthService['roleName']>): string {
  if (role === 'Teacher') return '/app/teacher';
  if (role === 'Director') return '/app/director';
  return '/app/parent';
}

export const parentRoleGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.roleName();
  if (role === 'Parent') return true;
  if (role === 'Teacher' || role === 'Director') {
    void router.navigateByUrl(homeForRole(role));
    return false;
  }
  void router.navigateByUrl('/login');
  return false;
};

export const teacherRoleGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.roleName();
  if (role === 'Teacher') return true;
  if (role === 'Parent' || role === 'Director') {
    void router.navigateByUrl(homeForRole(role));
    return false;
  }
  void router.navigateByUrl('/login');
  return false;
};

/** Parents must save student + attendee + relationship before other parent routes (except the form itself). */
export const parentMeetingProfileGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.roleName() !== 'Parent') return true;
  if (auth.isParentMeetingProfileComplete()) return true;
  if (router.url.includes('/parent/meeting-profile')) return true;
  void router.navigateByUrl('/app/parent/meeting-profile');
  return false;
};

export const directorRoleGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.roleName();
  if (role === 'Director') return true;
  if (role === 'Parent' || role === 'Teacher') {
    void router.navigateByUrl(homeForRole(role));
    return false;
  }
  void router.navigateByUrl('/login');
  return false;
};
