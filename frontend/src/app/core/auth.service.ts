import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
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

  logout(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem('pta_user_profile');
    this.profile.set(null);
    void this.router.navigateByUrl('/login');
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
