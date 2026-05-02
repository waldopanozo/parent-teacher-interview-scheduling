import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ScheduleApiService } from '../../core/schedule-api.service';
import { Booking, Slot, TeacherOfferingSummary } from '../../core/api.types';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe],
  templateUrl: './parent-dashboard.component.html',
  styleUrl: './parent-dashboard.component.scss'
})
export class ParentDashboardComponent implements OnInit {
  offerings: TeacherOfferingSummary[] = [];
  bookings: Booking[] = [];
  selectedOfferingId: string | null = null;
  interviewDate = '';
  slots: Slot[] = [];
  status: string | null = null;
  schoolTimeZoneId = 'UTC';

  constructor(
    private readonly api: ScheduleApiService,
    private readonly translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.api.catalogSchoolConfig().subscribe({
      next: (c) => {
        if (c.schoolTimeZoneId?.trim()) this.schoolTimeZoneId = c.schoolTimeZoneId.trim();
      },
      error: () => {}
    });
    this.reloadCatalog();
    this.reloadBookings();
    const today = new Date();
    this.interviewDate = today.toISOString().slice(0, 10);
  }

  attendanceLabel(s: number): string {
    switch (s) {
      case 1:
        return this.translate.instant('parent.attAttended');
      case 2:
        return this.translate.instant('parent.attNoShow');
      default:
        return this.translate.instant('parent.attUnspecified');
    }
  }

  reloadCatalog(): void {
    this.api.catalogOfferings().subscribe({
      next: (rows) => (this.offerings = rows),
      error: () => (this.status = 'Unable to load catalog.')
    });
  }

  reloadBookings(): void {
    this.api.parentBookings().subscribe({
      next: (rows) => (this.bookings = rows),
      error: () => (this.status = 'Unable to load bookings.')
    });
  }

  loadSlots(): void {
    this.status = null;
    this.slots = [];
    if (!this.selectedOfferingId || !this.interviewDate) return;
    this.api.slots(this.selectedOfferingId, this.interviewDate).subscribe({
      next: (rows) => (this.slots = rows),
      error: () => (this.status = 'Unable to load available slots.')
    });
  }

  book(slot: Slot): void {
    if (!this.selectedOfferingId) return;
    this.status = null;
    this.api
      .book({ teacherOfferingId: this.selectedOfferingId, startUtc: slot.startUtc })
      .subscribe({
        next: () => {
          this.loadSlots();
          this.reloadBookings();
        },
        error: (err) => {
          this.status = err?.error?.message ?? 'Booking failed.';
        }
      });
  }

  cancelBooking(booking: Booking): void {
    if (!booking.canCancel) return;
    if (!confirm(this.translate.instant('parent.cancelConfirm'))) return;
    this.status = null;
    this.api.cancelParentBooking(booking.id).subscribe({
      next: () => {
        this.reloadBookings();
        this.loadSlots();
      },
      error: (err) => {
        this.status = err?.error?.message ?? 'Could not cancel booking.';
      }
    });
  }
}
