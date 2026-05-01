import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import {
  Booking,
  Slot,
  SubjectSummary,
  TeacherAccessRequestListItem,
  TeacherListItem,
  TeacherOfferingSummary
} from './api.types';

@Injectable({ providedIn: 'root' })
export class ScheduleApiService {
  private readonly base = `${environment.apiBaseUrl}/v1`;

  constructor(private readonly http: HttpClient) {}

  subjects() {
    return this.http.get<SubjectSummary[]>(`${this.base}/subjects`);
  }

  catalogOfferings() {
    return this.http.get<TeacherOfferingSummary[]>(`${this.base}/catalog/teacher-offerings`);
  }

  slots(offeringId: string, date: string) {
    const params = new HttpParams().set('date', date);
    return this.http.get<Slot[]>(`${this.base}/catalog/teacher-offerings/${offeringId}/slots`, { params });
  }

  parentBookings() {
    return this.http.get<Booking[]>(`${this.base}/parent/bookings`);
  }

  book(body: { teacherOfferingId: string; startUtc: string }) {
    return this.http.post<Booking>(`${this.base}/parent/bookings`, body);
  }

  submitTeacherAccessRequest(body: { message?: string }) {
    return this.http.post<void>(`${this.base}/teacher-access-requests`, body);
  }

  teacherOfferings() {
    return this.http.get<TeacherOfferingSummary[]>(`${this.base}/teacher/offerings`);
  }

  createOffering(body: { subjectId: string; courseTitle: string; gradeLevel: string; sectionLabel?: string }) {
    return this.http.post<TeacherOfferingSummary>(`${this.base}/teacher/offerings`, body);
  }

  replaceWeeklyAvailability(
    offeringId: string,
    body: {
      windows: { dayOfWeek: string; startLocal: string; endLocal: string }[];
    }
  ) {
    return this.http.put<void>(`${this.base}/teacher/offerings/${offeringId}/weekly-availability`, body);
  }

  teacherBookings() {
    return this.http.get<Booking[]>(`${this.base}/teacher/bookings`);
  }

  directorTeacherAccessRequests(status?: string) {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<TeacherAccessRequestListItem[]>(`${this.base}/director/teacher-access-requests`, {
      params
    });
  }

  directorApproveTeacherAccess(id: string) {
    return this.http.post<void>(`${this.base}/director/teacher-access-requests/${id}/approve`, {});
  }

  directorRejectTeacherAccess(id: string) {
    return this.http.post<void>(`${this.base}/director/teacher-access-requests/${id}/reject`, {});
  }

  directorTeachers() {
    return this.http.get<TeacherListItem[]>(`${this.base}/director/teachers`);
  }

  directorTeacherOfferings(teacherUserId: string) {
    const params = new HttpParams().set('teacherUserId', teacherUserId);
    return this.http.get<TeacherOfferingSummary[]>(`${this.base}/director/teacher-offerings`, { params });
  }

  directorCreateOffering(body: {
    teacherUserId: string;
    subjectId: string;
    courseTitle: string;
    gradeLevel: string;
    sectionLabel?: string;
  }) {
    return this.http.post<TeacherOfferingSummary>(`${this.base}/director/teacher-offerings`, body);
  }

  directorReplaceWeeklyAvailability(
    offeringId: string,
    body: {
      windows: { dayOfWeek: string; startLocal: string; endLocal: string }[];
    }
  ) {
    return this.http.put<void>(
      `${this.base}/director/teacher-offerings/${offeringId}/weekly-availability`,
      body
    );
  }

  directorCreateSubject(body: { code: string; name: string }) {
    return this.http.post<SubjectSummary>(`${this.base}/director/subjects`, body);
  }

  directorUpdateSubject(id: string, body: { code: string; name: string }) {
    return this.http.put<void>(`${this.base}/director/subjects/${id}`, body);
  }

  directorDeleteSubject(id: string) {
    return this.http.delete<void>(`${this.base}/director/subjects/${id}`);
  }
}
