import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ScheduleApiService } from '../../core/schedule-api.service';
import { listIanaTimeZones } from '../../core/iana-timezones';
import {
  SchoolSettingsResponse,
  SubjectSummary,
  TeacherAccessRequestListItem,
  TeacherListItem,
  TeacherOfferingSummary,
  VisitAuditPage,
  VisitAuditRow
} from '../../core/api.types';

@Component({
  selector: 'app-director-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './director-dashboard.component.html',
  styleUrl: './director-dashboard.component.scss'
})
export class DirectorDashboardComponent implements OnInit {
  accessRequests: TeacherAccessRequestListItem[] = [];
  visitAudit: VisitAuditPage | null = null;
  studentEmailFilter = '';
  schoolSettings: SchoolSettingsResponse | null = null;
  editTimeZoneId = '';
  editUiLanguage: 'en' | 'es' = 'en';
  tzFilter = '';
  readonly allTimeZoneIds = listIanaTimeZones();
  timeZonesForDatalist: string[] = [];
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

  constructor(
    private readonly api: ScheduleApiService,
    private readonly translate: TranslateService
  ) {}

  get displaySchoolTimeZoneId(): string {
    return (
      this.schoolSettings?.schoolTimeZoneId?.trim() ||
      this.editTimeZoneId?.trim() ||
      'UTC'
    );
  }

  ngOnInit(): void {
    this.rebuildTimeZoneDatalist();
    this.reloadSchoolSettings();
    this.reloadSubjects();
    this.reloadTeachers();
    this.reloadRequests();
    this.reloadVisitAudit();
  }

  reloadSchoolSettings(): void {
    this.api.directorSchoolSettings().subscribe({
      next: (s) => {
        this.schoolSettings = s;
        this.editTimeZoneId = s.schoolTimeZoneId ?? '';
        this.editUiLanguage = s.uiLanguage?.toLowerCase().startsWith('es') ? 'es' : 'en';
        this.rebuildTimeZoneDatalist();
      },
      error: () => (this.status = 'Unable to load school settings.')
    });
  }

  rebuildTimeZoneDatalist(): void {
    const q = this.tzFilter.trim().toLowerCase();
    const ids = this.allTimeZoneIds;
    let list = q ? ids.filter((z) => z.toLowerCase().includes(q)) : ids;
    const cap = 120;
    const head = list.slice(0, cap);
    const cur = this.editTimeZoneId.trim();
    if (cur && !head.includes(cur)) {
      this.timeZonesForDatalist = [cur, ...head.filter((z) => z !== cur)].slice(0, cap + 1);
    } else {
      this.timeZonesForDatalist = head;
    }
  }

  saveSchoolSettings(): void {
    this.status = null;
    const id = this.editTimeZoneId.trim();
    if (!id) {
      this.status = this.translate.instant('director.tzRequired');
      return;
    }
    this.api
      .directorUpdateSchoolSettings({ schoolTimeZoneId: id, uiLanguage: this.editUiLanguage })
      .subscribe({
        next: (s) => {
          this.schoolSettings = s;
          this.editTimeZoneId = s.schoolTimeZoneId;
          this.editUiLanguage = s.uiLanguage?.toLowerCase().startsWith('es') ? 'es' : 'en';
          this.rebuildTimeZoneDatalist();
          void this.translate.use(this.editUiLanguage);
          this.status = null;
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Update failed.')
      });
  }

  reloadVisitAudit(): void {
    this.api.directorVisitAudit(this.studentEmailFilter || undefined).subscribe({
      next: (page) => (this.visitAudit = page),
      error: () => (this.status = 'Unable to load visit audit.')
    });
  }

  visitRows(): VisitAuditRow[] {
    return this.visitAudit?.items ?? [];
  }

  visitSummary() {
    return this.visitAudit?.summary ?? null;
  }

  requestStatusLabel(status: number): string {
    switch (status) {
      case 0:
        return this.translate.instant('director.reqPending');
      case 1:
        return this.translate.instant('director.reqApproved');
      case 2:
        return this.translate.instant('director.reqRejected');
      default:
        return String(status);
    }
  }

  attendanceLabel(s: number): string {
    switch (s) {
      case 1:
        return this.translate.instant('teacher.attAttended');
      case 2:
        return this.translate.instant('teacher.attNoShow');
      default:
        return this.translate.instant('teacher.attUnspecified');
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
        this.reloadRequests();
        this.reloadTeachers();
      },
      error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Approve failed.')
    });
  }

  reject(id: string): void {
    this.api.directorRejectTeacherAccess(id).subscribe({
      next: () => this.reloadRequests(),
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
    if (!this.newSubjectCode.trim() || !this.newSubjectName.trim()) return;
    this.api
      .directorCreateSubject({ code: this.newSubjectCode.trim(), name: this.newSubjectName.trim() })
      .subscribe({
        next: () => {
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
          this.editingSubject = null;
          this.reloadSubjects();
        },
        error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Update failed.')
      });
  }

  deleteSubject(s: SubjectSummary): void {
    this.status = null;
    if (!confirm(this.translate.instant('director.confirmDeleteSubject', { code: s.code }))) return;
    this.api.directorDeleteSubject(s.id).subscribe({
      next: () => this.reloadSubjects(),
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
    if (!this.availabilityOfferingId) return;
    const body = {
      windows: [{ dayOfWeek: this.dayOfWeek, startLocal: this.startLocal, endLocal: this.endLocal }]
    };
    this.api.directorReplaceWeeklyAvailability(this.availabilityOfferingId, body).subscribe({
      next: () => {},
      error: (err) => (this.status = err?.error ?? err?.error?.message ?? 'Save failed.')
    });
  }
}
