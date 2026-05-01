import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements AfterViewInit {
  @ViewChild('googleButton', { static: true }) googleButton!: ElementRef<HTMLDivElement>;

  errorMessage: string | null = null;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngAfterViewInit(): void {
    const clientId = environment.googleClientId?.trim();
    if (!clientId || clientId.startsWith('replace-with')) {
      this.errorMessage =
        'Configure googleClientId in src/environments/environment.development.ts to match your Google OAuth Web client.';
      return;
    }

    const render = () => {
      const google = window.google;
      if (!google?.accounts?.id) {
        window.setTimeout(render, 50);
        return;
      }

      google.accounts.id.initialize({
        client_id: clientId,
        callback: (resp: { credential: string }) => this.handleCredential(resp.credential),
        auto_select: false,
        itp_support: true
      });

      google.accounts.id.renderButton(this.googleButton.nativeElement, {
        type: 'standard',
        theme: 'outline',
        size: 'large',
        text: 'continue_with',
        shape: 'rectangular'
      });
    };

    render();
  }

  private handleCredential(idToken: string): void {
    this.errorMessage = null;
    this.auth.signInWithGoogleIdToken(idToken).subscribe({
      next: () => {
        const role = this.auth.roleName();
        if (role === 'Teacher') {
          void this.router.navigateByUrl('/app/teacher');
          return;
        }
        if (role === 'Director') {
          void this.router.navigateByUrl('/app/director');
          return;
        }
        if (role === 'Parent' && !this.auth.isParentMeetingProfileComplete()) {
          void this.router.navigateByUrl('/app/parent/meeting-profile');
          return;
        }
        void this.router.navigateByUrl('/app/parent');
      },
      error: (err) => {
        const msg = err?.error?.message ?? err?.message ?? 'Sign-in failed.';
        this.errorMessage = typeof msg === 'string' ? msg : 'Sign-in failed.';
      }
    });
  }
}
