export type AppRole = 'Parent' | 'Teacher';

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: number;
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
  teacherDisplayName: string;
  parentDisplayName: string;
  parentEmail: string;
}
