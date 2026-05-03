/** API AppRole enum: Parent = 0, Teacher = 1, Director = 2 */
export type AppRoleNumber = 0 | 1 | 2;

/** Booking attendance: Unspecified = 0, Attended = 1, NoShow = 2 */
export type AttendanceStatusNumber = 0 | 1 | 2;

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: AppRoleNumber;
  /** Present for Parent after API update; must be true before booking. */
  meetingProfileComplete?: boolean;
  /** True when account has email/password (not Google-only). */
  hasPasswordLogin?: boolean;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
}

export interface SubjectSummary {
  id: string;
  code: string;
  name: string;
}

/** GET /catalog/school-config (anonymous) */
export interface SchoolPublicConfig {
  schoolTimeZoneId: string;
  uiLanguage: string;
  themePreset: string;
  hasCustomLogo: boolean;
  brandingVersion: number;
}

export interface TeacherOfferingSummary {
  id: string;
  teacherDisplayName: string;
  subjectName: string;
  subjectCode: string;
  courseTitle: string;
  gradeLevel: string;
  sectionLabel: string;
}

export interface Slot {
  startUtc: string;
  endUtc: string;
}

export interface Booking {
  id: string;
  startUtc: string;
  endUtc: string;
  subjectName: string;
  courseTitle: string;
  gradeLevel: string;
  sectionLabel: string;
  studentSchoolEmail: string;
  interviewAttendeeName: string;
  relationshipToStudent: string;
  teacherDisplayName: string;
  parentDisplayName: string;
  parentEmail: string;
  /** Parent: true if cancellation is still allowed (before interview day, school calendar). */
  canCancel: boolean;
  attendanceStatus: AttendanceStatusNumber;
  visitNotes: string | null;
}

export interface ParentMeetingProfile {
  studentSchoolEmail: string | null;
  interviewAttendeeName: string | null;
  relationshipToStudent: string | null;
}

export interface TeacherListItem {
  id: string;
  email: string;
  displayName: string;
}

/** TeacherAccessRequestStatus: Pending = 0, Approved = 1, Rejected = 2 */
export interface TeacherAccessRequestListItem {
  id: string;
  applicantUserId: string;
  applicantEmail: string;
  applicantDisplayName: string;
  message: string | null;
  status: number;
  createdAt: string;
}

export interface SchoolSettingsResponse {
  schoolTimeZoneId: string;
  uiLanguage: string;
  themePreset: string;
  hasCustomLogo: boolean;
  brandingVersion: number;
  updatedAt: string | null;
  updatedByDirectorId: string | null;
}

export interface VisitAuditSummary {
  totalBookings: number;
  cancelledCount: number;
  activeCount: number;
}

export interface VisitAuditRow {
  id: string;
  createdAt: string;
  isCancelled: boolean;
  cancelledAt: string | null;
  cancelledByUserId: string | null;
  startUtc: string;
  endUtc: string;
  studentSchoolEmail: string;
  parentEmail: string;
  parentDisplayName: string;
  teacherDisplayName: string;
  subjectName: string;
  courseTitle: string;
  gradeLevel: string;
  sectionLabel: string;
  attendanceStatus: AttendanceStatusNumber;
  visitNotes: string | null;
}

export interface VisitAuditPage {
  items: VisitAuditRow[];
  summary: VisitAuditSummary | null;
}
