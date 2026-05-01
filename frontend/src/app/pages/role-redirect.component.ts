import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-role-redirect',
  standalone: true,
  template: ''
})
export class RoleRedirectComponent implements OnInit {
  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const role = this.auth.roleName();
    if (role === 'Teacher') {
      void this.router.navigateByUrl('/app/teacher');
      return;
    }
    if (role === 'Director') {
      void this.router.navigateByUrl('/app/director');
      return;
    }
    if (role === 'Parent' && !this.auth.isParentMeetingProfileComplete()) {
      void this.router.navigateByUrl('/app/parent/meeting-profile');
      return;
    }
    void this.router.navigateByUrl('/app/parent');
  }
}
