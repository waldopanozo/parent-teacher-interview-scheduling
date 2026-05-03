import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth.service';
import { ScheduleApiService } from '../../core/schedule-api.service';

@Component({
  selector: 'app-parent-meeting-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe],
  templateUrl: './parent-meeting-profile.component.html',
  styleUrl: './parent-meeting-profile.component.scss'
})
export class ParentMeetingProfileComponent implements OnInit {
  studentSchoolEmail = '';
  interviewAttendeeName = '';
  relationshipToStudent = '';
  status: string | null = null;

  /** Link back to dashboard only after profile is already complete (editing). */
  showSkipToApp = false;

  constructor(
    private readonly api: ScheduleApiService,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.showSkipToApp = this.auth.isParentMeetingProfileComplete();
    this.api.getMeetingProfile().subscribe({
      next: (p) => {
        this.studentSchoolEmail = p.studentSchoolEmail ?? '';
        this.interviewAttendeeName = p.interviewAttendeeName ?? '';
        this.relationshipToStudent = p.relationshipToStudent ?? '';
      },
      error: () => (this.status = 'Unable to load your saved profile.')
    });
  }

  save(): void {
    this.status = null;
    this.api
      .putMeetingProfile({
        studentSchoolEmail: this.studentSchoolEmail.trim(),
        interviewAttendeeName: this.interviewAttendeeName.trim(),
        relationshipToStudent: this.relationshipToStudent.trim()
      })
      .subscribe({
        next: () => {
          this.auth.refreshProfileFromServer().subscribe({
            next: () => void this.router.navigateByUrl('/app/parent'),
            error: () => (this.status = 'Saved, but could not refresh session. Try signing out and in again.')
          });
        },
        error: (err) => {
          this.status = err?.error?.message ?? err?.error ?? 'Save failed.';
        }
      });
  }
}
