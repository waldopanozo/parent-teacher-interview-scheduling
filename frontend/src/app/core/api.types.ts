/** API AppRole enum: Parent = 0, Teacher = 1, Director = 2 */
export type AppRoleNumber = 0 | 1 | 2;

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: AppRoleNumber;
  /** Present for Parent after API update; must be true before booking. */
  meetingProfileComplete?: boolean;
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
