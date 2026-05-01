import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Booking, Slot, SubjectSummary, TeacherOfferingSummary } from './api.types';

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

  teacherOfferings() {
    return this.http.get<TeacherOfferingSummary[]>(`${this.base}/teacher/offerings`);
  }

  createOffering(body: { subjectId: string; courseTitle: string; gradeLevel: string }) {
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
}
