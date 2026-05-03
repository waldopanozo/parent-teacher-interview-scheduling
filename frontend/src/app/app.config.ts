import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTranslateService, TranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { firstValueFrom } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';
import { SchoolPublicConfig } from './core/api.types';
import { SchoolBrandingService } from './core/school-branding.service';
import { environment } from '../environments/environment';

export function schoolLocaleInitializer(
  http: HttpClient,
  translate: TranslateService,
  branding: SchoolBrandingService
) {
  return () =>
    firstValueFrom(
      http.get<SchoolPublicConfig>(`${environment.apiBaseUrl}/v1/catalog/school-config`).pipe(
        switchMap((cfg) => {
          branding.applyPublicConfig(cfg);
          const lang = cfg.uiLanguage?.toLowerCase().startsWith('es') ? 'es' : 'en';
          translate.setFallbackLang('en');
          return translate.use(lang);
        }),
        catchError(() => {
          branding.applyPublicConfig({
            schoolTimeZoneId: 'UTC',
            uiLanguage: 'en',
            themePreset: 'default',
            hasCustomLogo: false,
            brandingVersion: 0
          });
          translate.setFallbackLang('en');
          return translate.use('en');
        })
      )
    );
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    ...provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: '/assets/i18n/',
        suffix: '.json'
      }),
      fallbackLang: 'en'
    }),
    {
      provide: APP_INITIALIZER,
      useFactory: schoolLocaleInitializer,
      deps: [HttpClient, TranslateService, SchoolBrandingService],
      multi: true
    }
  ]
};
