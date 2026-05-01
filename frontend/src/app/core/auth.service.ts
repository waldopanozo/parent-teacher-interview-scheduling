import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { Observable, tap } from 'rxjs';
import { AuthResponse, UserProfile } from './api.types';
import { tokenKey } from './auth.interceptor';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly profile = signal<UserProfile | null>(this.readProfile());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  isAuthenticated(): boolean {
    return !!localStorage.getItem(tokenKey);
  }

  roleName(): AppRoleName | null {
    const p = this.profile();
    if (!p) return null;
    switch (p.role) {
      case 1:
        return 'Teacher';
      case 2:
        return 'Director';
      default:
        return 'Parent';
    }
  }

  signInWithGoogleIdToken(idToken: string) {
    const url = `${environment.apiBaseUrl}/v1/auth/google`;
    return this.http.post<AuthResponse>(url, { idToken }).pipe(
      tap((r) => {
        localStorage.setItem(tokenKey, r.accessToken);
        localStorage.setItem('pta_user_profile', JSON.stringify(r.user));
        this.profile.set(r.user);
      })
    );
  }

  registerWithEmailPassword(email: string, password: string, displayName: string) {
    const url = `${environment.apiBaseUrl}/v1/auth/register`;
    return this.http.post<AuthResponse>(url, { email, password, displayName }).pipe(
      tap((r) => {
        localStorage.setItem(tokenKey, r.accessToken);
        localStorage.setItem('pta_user_profile', JSON.stringify(r.user));
        this.profile.set(r.user);
      })
    );
  }

  signInWithEmailPassword(email: string, password: string) {
    const url = `${environment.apiBaseUrl}/v1/auth/email-login`;
    return this.http.post<AuthResponse>(url, { email, password }).pipe(
      tap((r) => {
        localStorage.setItem(tokenKey, r.accessToken);
        localStorage.setItem('pta_user_profile', JSON.stringify(r.user));
        this.profile.set(r.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem('pta_user_profile');
    this.profile.set(null);
    void this.router.navigateByUrl('/login');
  }

  /** Reloads profile from API (e.g. after saving meeting registration). */
  refreshProfileFromServer(): Observable<UserProfile> {
    const url = `${environment.apiBaseUrl}/v1/auth/me`;
    return this.http.get<UserProfile>(url).pipe(
      tap((u) => {
        localStorage.setItem('pta_user_profile', JSON.stringify(u));
        this.profile.set(u);
      })
    );
  }

  isParentMeetingProfileComplete(): boolean {
    const p = this.profile();
    if (!p || p.role !== 0) return true;
    return p.meetingProfileComplete === true;
  }

  private readProfile(): UserProfile | null {
    const raw = localStorage.getItem('pta_user_profile');
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}

export type AppRoleName = 'Parent' | 'Teacher' | 'Director';
