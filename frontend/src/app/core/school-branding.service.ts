import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { SchoolPublicConfig, SchoolSettingsResponse } from './api.types';
import { applySchoolTheme } from './school-theme';

@Injectable({ providedIn: 'root' })
export class SchoolBrandingService {
  private hasCustomLogo = false;
  private brandingVersion = 0;

  /** Apply anonymous public config (startup + after logout). */
  applyPublicConfig(cfg: SchoolPublicConfig): void {
    applySchoolTheme(cfg.themePreset);
    this.hasCustomLogo = cfg.hasCustomLogo;
    this.brandingVersion = cfg.brandingVersion ?? 0;
  }

  /** Apply director settings response after save or logo upload. */
  applyDirectorSettings(s: SchoolSettingsResponse): void {
    applySchoolTheme(s.themePreset);
    this.hasCustomLogo = s.hasCustomLogo;
    this.brandingVersion = s.brandingVersion ?? 0;
  }

  getLogoSrc(): string {
    if (!this.hasCustomLogo) {
      return '/assets/school-logo-default.svg';
    }
    return `${environment.apiBaseUrl}/v1/catalog/school-logo?v=${this.brandingVersion}`;
  }
}
