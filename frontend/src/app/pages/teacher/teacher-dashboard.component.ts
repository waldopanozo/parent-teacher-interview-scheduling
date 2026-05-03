import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ScheduleApiService } from '../../core/schedule-api.service';
import { Booking, SubjectSummary, TeacherOfferingSummary } from '../../core/api.types';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './teacher-dashboard.component.html',
  styleUrl: './teacher-dashboard.component.scss'
})
export class TeacherDashboardComponent implements OnInit {
  subjects: SubjectSummary[] = [];
  offerings: TeacherOfferingSummary[] = [];
  bookings: Booking[] = [];
  schoolTimeZoneId = 'UTC';

  subjectId: string | null = null;
  courseTitle = '';
  gradeLevel = '';
  sectionLabel = '';

  availabilityOfferingId: string | null = null;
  dayOfWeek = 'Monday';
  startLocal = '14:00';
  endLocal = '17:00';

  status: string | null = null;

  readonly days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  constructor(private readonly api: ScheduleApiService) {}

  ngOnInit(): void {
    this.api.catalogSchoolConfig().subscribe({
      next: (c) => {
        if (c.schoolTimeZoneId?.trim()) this.schoolTimeZoneId = c.schoolTimeZoneId.trim();
      },
      error: () => {}
    });
    this.api.subjects().subscribe({
      next: (rows) => (this.subjects = rows),
      error: () => (this.status = 'Unable to load subjects.')
    });
    this.reload();
  }

  reload(): void {
    this.api.teacherOfferings().subscribe({
      next: (rows) => (this.offerings = rows),
      error: () => (this.status = 'Unable to load offerings.')
    });
    this.api.teacherBookings().subscribe({
      next: (rows) =>
        (this.bookings = rows.map((r) => ({
          ...r,
          visitNotes: r.visitNotes ?? ''
        }))),
      error: () => (this.status = 'Unable to load bookings.')
    });
  }

  createOffering(): void {
    this.status = null;
    if (!this.subjectId || !this.courseTitle.trim() || !this.gradeLevel.trim()) {
      return;
    }

    this.api
      .createOffering({
        subjectId: this.subjectId,
        courseTitle: this.courseTitle.trim(),
        gradeLevel: this.gradeLevel.trim(),
        sectionLabel: this.sectionLabel.trim()
      })
      .subscribe({
        next: () => {
          this.courseTitle = '';
          this.gradeLevel = '';
          this.sectionLabel = '';
          this.reload();
        },
        error: (err) => (this.status = err?.error ?? 'Create failed.')
      });
  }

  publishAvailability(): void {
    this.status = null;
    if (!this.availabilityOfferingId) {
      return;
    }

    const body = {
      windows: [{ dayOfWeek: this.dayOfWeek, startLocal: this.startLocal, endLocal: this.endLocal }]
    };

    this.api.replaceWeeklyAvailability(this.availabilityOfferingId, body).subscribe({
      next: () => {},
      error: (err) => (this.status = err?.error ?? 'Save failed.')
    });
  }

  saveOutcome(b: Booking): void {
    this.status = null;
    this.api
      .teacherPatchBooking(b.id, {
        attendanceStatus: b.attendanceStatus,
        visitNotes: b.visitNotes?.trim() || null
      })
      .subscribe({
        next: (updated) => {
          const i = this.bookings.findIndex((x) => x.id === updated.id);
          if (i >= 0) this.bookings[i] = updated;
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Save failed.')
      });
  }
}
