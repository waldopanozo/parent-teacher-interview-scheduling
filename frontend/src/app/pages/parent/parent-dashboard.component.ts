import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ScheduleApiService } from '../../core/schedule-api.service';
import { Booking, Slot, TeacherOfferingSummary } from '../../core/api.types';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
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

  constructor(private readonly api: ScheduleApiService) {}

  ngOnInit(): void {
    this.reloadCatalog();
    this.reloadBookings();
    const today = new Date();
    this.interviewDate = today.toISOString().slice(0, 10);
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
          this.status = 'Booking confirmed.';
          this.loadSlots();
          this.reloadBookings();
        },
        error: (err) => {
          this.status = err?.error?.message ?? 'Booking failed.';
        }
      });
  }
}
