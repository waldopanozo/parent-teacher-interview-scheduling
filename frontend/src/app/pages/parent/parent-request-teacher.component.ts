import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ScheduleApiService } from '../../core/schedule-api.service';

@Component({
  selector: 'app-parent-request-teacher',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './parent-request-teacher.component.html',
  styleUrl: './parent-request-teacher.component.scss'
})
export class ParentRequestTeacherComponent {
  message = '';
  status: string | null = null;

  constructor(private readonly api: ScheduleApiService) {}

  submit(): void {
    this.status = null;
    this.api.submitTeacherAccessRequest({ message: this.message.trim() || undefined }).subscribe({
      next: () => {
        this.status = 'Request submitted. A director will review it.';
        this.message = '';
      },
      error: (err) => {
        this.status = err?.error?.message ?? err?.error ?? 'Submit failed.';
      }
    });
  }
}
