import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ScheduleApiService } from '../../core/schedule-api.service';
import {
  SubjectSummary,
  TeacherAccessRequestListItem,
  TeacherListItem,
  TeacherOfferingSummary
} from '../../core/api.types';

@Component({
  selector: 'app-director-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './director-dashboard.component.html',
  styleUrl: './director-dashboard.component.scss'
})
export class DirectorDashboardComponent implements OnInit {
  accessRequests: TeacherAccessRequestListItem[] = [];
  subjects: SubjectSummary[] = [];
  teachers: TeacherListItem[] = [];
  directorOfferings: TeacherOfferingSummary[] = [];

  newSubjectCode = '';
  newSubjectName = '';
  editingSubject: SubjectSummary | null = null;
  editSubjectCode = '';
  editSubjectName = '';

  offeringTeacherId: string | null = null;
  offeringSubjectId: string | null = null;
  offeringCourseTitle = '';
  offeringGradeLevel = '';
  offeringSectionLabel = '';

  availabilityTeacherId: string | null = null;
  availabilityOfferingId: string | null = null;
  dayOfWeek = 'Monday';
  startLocal = '14:00';
  endLocal = '17:00';

  status: string | null = null;

  readonly days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  constructor(private readonly api: ScheduleApiService) {}

  ngOnInit(): void {
    this.reloadSubjects();
    this.reloadTeachers();
    this.reloadRequests();
  }

  requestStatusLabel(status: number): string {
    switch (status) {
      case 0:
        return 'Pending';
      case 1:
        return 'Approved';
      case 2:
        return 'Rejected';
      default:
        return String(status);
    }
  }

  reloadRequests(): void {
    this.status = null;
    this.api.directorTeacherAccessRequests().subscribe({
      next: (rows) => (this.accessRequests = rows),
      error: () => (this.status = 'Unable to load teacher access requests.')
    });
  }

  approve(id: string): void {
    this.api.directorApproveTeacherAccess(id).subscribe({
      next: () => {
        this.status = 'Request approved. Applicant must sign in again to use teacher features.';
        this.reloadRequests();
        this.reloadTeachers();
      },
      error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Approve failed.')
    });
  }

  reject(id: string): void {
    this.api.directorRejectTeacherAccess(id).subscribe({
      next: () => {
        this.status = 'Request rejected.';
        this.reloadRequests();
      },
      error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Reject failed.')
    });
  }

  reloadSubjects(): void {
    this.api.subjects().subscribe({
      next: (rows) => (this.subjects = rows),
      error: () => (this.status = 'Unable to load subjects.')
    });
  }

  reloadTeachers(): void {
    this.api.directorTeachers().subscribe({
      next: (rows) => (this.teachers = rows),
      error: () => (this.status = 'Unable to load teachers.')
    });
  }

  createSubject(): void {
    this.status = null;
    if (!this.newSubjectCode.trim() || !this.newSubjectName.trim()) {
      this.status = 'Subject code and name are required.';
      return;
    }
    this.api
      .directorCreateSubject({ code: this.newSubjectCode.trim(), name: this.newSubjectName.trim() })
      .subscribe({
        next: () => {
          this.status = 'Subject created.';
          this.newSubjectCode = '';
          this.newSubjectName = '';
          this.reloadSubjects();
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Create subject failed.')
      });
  }

  startEditSubject(s: SubjectSummary): void {
    this.editingSubject = s;
    this.editSubjectCode = s.code;
    this.editSubjectName = s.name;
  }

  saveSubjectEdit(): void {
    if (!this.editingSubject) return;
    this.status = null;
    this.api
      .directorUpdateSubject(this.editingSubject.id, {
        code: this.editSubjectCode.trim(),
        name: this.editSubjectName.trim()
      })
      .subscribe({
        next: () => {
          this.status = 'Subject updated.';
          this.editingSubject = null;
          this.reloadSubjects();
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Update failed.')
      });
  }

  deleteSubject(s: SubjectSummary): void {
    this.status = null;
    if (!confirm(`Delete subject ${s.code}?`)) return;
    this.api.directorDeleteSubject(s.id).subscribe({
      next: () => {
        this.status = 'Subject deleted.';
        this.reloadSubjects();
      },
      error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Delete failed.')
    });
  }

  onOfferingTeacherChange(): void {
    this.directorOfferings = [];
  }

  onAvailabilityTeacherChange(): void {
    this.availabilityOfferingId = null;
    this.directorOfferings = [];
    if (!this.availabilityTeacherId) return;
    this.api.directorTeacherOfferings(this.availabilityTeacherId).subscribe({
      next: (rows) => (this.directorOfferings = rows),
      error: () => (this.status = 'Unable to load offerings for teacher.')
    });
  }

  createOfferingForTeacher(): void {
    this.status = null;
    if (
      !this.offeringTeacherId ||
      !this.offeringSubjectId ||
      !this.offeringCourseTitle.trim() ||
      !this.offeringGradeLevel.trim()
    ) {
      this.status = 'Teacher, subject, course title, and grade are required.';
      return;
    }
    this.api
      .directorCreateOffering({
        teacherUserId: this.offeringTeacherId,
        subjectId: this.offeringSubjectId,
        courseTitle: this.offeringCourseTitle.trim(),
        gradeLevel: this.offeringGradeLevel.trim(),
        sectionLabel: this.offeringSectionLabel.trim()
      })
      .subscribe({
        next: () => {
          this.status = 'Offering created for teacher.';
          this.offeringCourseTitle = '';
          this.offeringGradeLevel = '';
          this.offeringSectionLabel = '';
          if (this.availabilityTeacherId === this.offeringTeacherId) {
            this.onAvailabilityTeacherChange();
          }
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Create offering failed.')
      });
  }

  publishDirectorAvailability(): void {
    this.status = null;
    if (!this.availabilityOfferingId) {
      this.status = 'Select teacher and offering first.';
      return;
    }
    const body = {
      windows: [{ dayOfWeek: this.dayOfWeek, startLocal: this.startLocal, endLocal: this.endLocal }]
    };
    this.api.directorReplaceWeeklyAvailability(this.availabilityOfferingId, body).subscribe({
      next: () => {
        this.status = 'Weekly availability saved for that offering.';
      },
      error: (err) => (this.status = err?.error ?? err?.error?.message ?? 'Save failed.')
    });
  }
}
