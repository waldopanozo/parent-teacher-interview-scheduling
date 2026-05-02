import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, ViewChild } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements AfterViewInit {
  @ViewChild('googleButton', { static: true }) googleButton!: ElementRef<HTMLDivElement>;

  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly registerForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  /** When true, registration form is on the left and promo panel on the right (desktop). */
  isRegisterMode = false;

  errorMessage: string | null = null;
  infoMessage: string | null = null;

  ngAfterViewInit(): void {
    this.initGoogleButton();
  }

  setMode(register: boolean): void {
    this.isRegisterMode = register;
    this.errorMessage = null;
    this.infoMessage = null;
  }

  submitLogin(): void {
    this.errorMessage = null;
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    const { email, password } = this.loginForm.getRawValue();
    this.auth.signInWithEmailPassword(email, password).subscribe({
      next: () => this.navigateAfterAuth(),
      error: (err) => this.setHttpError(err)
    });
  }

  submitRegister(): void {
    this.errorMessage = null;
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    const { username, email, password } = this.registerForm.getRawValue();
    this.auth.registerWithEmailPassword(email, password, username).subscribe({
      next: () => this.navigateAfterAuth(),
      error: (err) => this.setHttpError(err)
    });
  }

  forgotPasswordClick(event: Event): void {
    event.preventDefault();
    this.infoMessage =
      'Password recovery is not available yet. Use Google sign-in or contact your school administrator.';
    this.errorMessage = null;
  }

  private initGoogleButton(): void {
    const clientId = environment.googleClientId?.trim();
    if (!clientId || clientId.startsWith('replace-with')) {
      this.errorMessage =
        'Configure googleClientId in environment so Google sign-in works.';
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
        callback: (resp: { credential: string }) => this.handleGoogleCredential(resp.credential),
        auto_select: false,
        itp_support: true
      });

      google.accounts.id.renderButton(this.googleButton.nativeElement, {
        type: 'standard',
        theme: 'filled_blue',
        size: 'large',
        text: 'signin_with',
        shape: 'pill',
        width: 300
      });
    };

    render();
  }

  private handleGoogleCredential(idToken: string): void {
    this.errorMessage = null;
    this.infoMessage = null;
    this.auth.signInWithGoogleIdToken(idToken).subscribe({
      next: () => this.navigateAfterAuth(),
      error: (err) => this.setHttpError(err)
    });
  }

  private navigateAfterAuth(): void {
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
  }

  private setHttpError(err: { error?: { message?: string }; message?: string }): void {
    const msg = err?.error?.message ?? err?.message ?? 'Something went wrong.';
    this.errorMessage = typeof msg === 'string' ? msg : 'Something went wrong.';
    this.infoMessage = null;
  }
}
