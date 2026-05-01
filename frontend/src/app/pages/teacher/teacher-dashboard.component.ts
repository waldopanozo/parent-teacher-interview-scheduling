import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ScheduleApiService } from '../../core/schedule-api.service';
import { Booking, SubjectSummary, TeacherOfferingSummary } from '../../core/api.types';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrl: './teacher-dashboard.component.scss'
})
export class TeacherDashboardComponent implements OnInit {
  subjects: SubjectSummary[] = [];
  offerings: TeacherOfferingSummary[] = [];
  bookings: Booking[] = [];

  subjectId: string | null = null;
  courseTitle = '';
  gradeLevel = '';

  availabilityOfferingId: string | null = null;
  dayOfWeek = 'Monday';
  startLocal = '14:00';
  endLocal = '17:00';

  status: string | null = null;

  readonly days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  constructor(private readonly api: ScheduleApiService) {}

  ngOnInit(): void {
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
      next: (rows) => (this.bookings = rows),
      error: () => (this.status = 'Unable to load bookings.')
    });
  }

  createOffering(): void {
    this.status = null;
    if (!this.subjectId || !this.courseTitle.trim() || !this.gradeLevel.trim()) {
      this.status = 'Subject, course title, and grade are required.';
      return;
    }

    this.api
      .createOffering({
        subjectId: this.subjectId,
        courseTitle: this.courseTitle.trim(),
        gradeLevel: this.gradeLevel.trim()
      })
      .subscribe({
        next: () => {
          this.status = 'Offering created.';
          this.courseTitle = '';
          this.gradeLevel = '';
          this.reload();
        },
        error: (err) => (this.status = err?.error ?? 'Create failed.')
      });
  }

  publishAvailability(): void {
    this.status = null;
    if (!this.availabilityOfferingId) {
      this.status = 'Select an offering to publish weekly availability.';
      return;
    }

    const body = {
      windows: [{ dayOfWeek: this.dayOfWeek, startLocal: this.startLocal, endLocal: this.endLocal }]
    };

    this.api.replaceWeeklyAvailability(this.availabilityOfferingId, body).subscribe({
      next: () => {
        this.status = 'Weekly availability saved (replaces previous windows for that offering).';
      },
      error: (err) => (this.status = err?.error ?? 'Save failed.')
    });
  }
}
