import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../core/auth.service';
import { ScheduleApiService } from '../../core/schedule-api.service';

@Component({
  selector: 'app-account-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './account-password.component.html',
  styleUrl: './account-password.component.scss'
})
export class AccountPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ScheduleApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  readonly form = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  });

  status: string | null = null;
  success = false;

  ngOnInit(): void {
    this.auth.refreshProfileFromServer().subscribe({
      next: (p) => {
        if (!p.hasPasswordLogin) void this.router.navigateByUrl('/app');
      },
      error: () => {
        if (!this.auth.profile()?.hasPasswordLogin) void this.router.navigateByUrl('/app');
      }
    });
  }

  submit(): void {
    this.status = null;
    this.success = false;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { currentPassword, newPassword, confirmPassword } = this.form.getRawValue();
    if (newPassword !== confirmPassword) {
      this.status = this.translate.instant('accountPassword.mismatch');
      return;
    }
    this.api.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.success = true;
        this.status = null;
        this.form.reset();
      },
      error: (err) => (this.status = err?.error?.message ?? err?.error ?? 'Error.')
    });
  }
}
