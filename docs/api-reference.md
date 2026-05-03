# HTTP API reference (v1)

Base URL examples:

- Through the SPA/nginx: `http://localhost:3456/api/v1/...`
- Direct API: `http://localhost:5103/api/v1/...`

Unless noted, requests send header `Authorization: Bearer <jwt>`.

## Operations

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | No | Aggregate health (includes EF Core **database** check). Intended for load balancers / orchestrators. Response body is the default ASP.NET Core health report format. |

Clients may send **`X-Request-Id`** (any string); the API echoes the same value on the response and attaches it to structured logs for the request.

## Auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/v1/auth/google` | No | Body: `{ "idToken": "<google credential>" }`. Returns access token + user profile (`role`: Parent=0, Teacher=1, Director=2; `meetingProfileComplete` for parents; `hasPasswordLogin` is **false** for Google-only accounts). |
| POST | `/api/v1/auth/register` | No | Body: `{ "email", "password" (min 8), "displayName" }`. Same domain/bootstrap rules as Google. **400** with `{ "message" }` on validation/conflict. |
| POST | `/api/v1/auth/email-login` | No | Body: `{ "email", "password" }`. **401** with `{ "message" }` if invalid. |
| GET | `/api/v1/auth/me` | Yes | Current user profile (same shape as login `user`, including `meetingProfileComplete` and `hasPasswordLogin`). |
| PUT | `/api/v1/auth/password` | Yes | Body: `{ "currentPassword", "newPassword" }` (new min 8 chars). **Email/password accounts only**; **400** if Google-only or wrong current password. **204** on success. |

## Catalog (anonymous)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/catalog/school-config` | School time zone, UI language, `themePreset`, `hasCustomLogo`, `brandingVersion` (for SPA theming before login). |
| GET | `/api/v1/catalog/school-logo` | School logo bytes when configured; **404** if using default branding only. |

## Catalog (authenticated)

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/v1/subjects` | Any authenticated | List subjects. |
| GET | `/api/v1/catalog/teacher-offerings` | Any authenticated | List offerings with teacher, subject, course, grade, `sectionLabel`. |
| GET | `/api/v1/catalog/teacher-offerings/{id}/slots?date=YYYY-MM-DD` | Any authenticated | Available slot start/end (UTC) for that local calendar date. |

## Parent

| Method | Path | Role | Description |
|--------|------|------|-------------|
| GET | `/api/v1/parent/bookings` | Parent | List caller’s bookings. Each item includes `canCancel` (true before the interview’s school-calendar day). |
| POST | `/api/v1/parent/bookings` | Parent | Body: `teacherOfferingId`, `startUtc`. Creates booking (requires completed meeting profile; **400** if not). One booking per parent per school day. |
| DELETE | `/api/v1/parent/bookings/{id}` | Parent | Cancel own booking. **400** on or after the interview day (school time zone). **404** if not found or not owned. |
| GET | `/api/v1/parent/meeting-profile` | Parent | Returns saved student email, attendee name, relationship (may be null until saved). |
| PUT | `/api/v1/parent/meeting-profile` | Parent | Body: `studentSchoolEmail`, `interviewAttendeeName`, `relationshipToStudent`. Validates email shape and lengths. |
| POST | `/api/v1/teacher-access-requests` | Parent | Body: `{ "message": "optional" }`. Submits teacher access request (409 if pending duplicate). |

## Teacher

| Method | Path | Role | Description |
|--------|------|------|-------------|
| GET | `/api/v1/teacher/offerings` | Teacher | List own offerings. |
| POST | `/api/v1/teacher/offerings` | Teacher | Body: `subjectId`, `courseTitle`, `gradeLevel`, optional `sectionLabel`. |
| PUT | `/api/v1/teacher/offerings/{offeringId}/weekly-availability` | Teacher | Body: `{ "windows": [ { "dayOfWeek": "Monday", "startLocal": "14:00", "endLocal": "17:00" } ] }`. **Replaces** all windows. `dayOfWeek` is JSON string enum. |
| GET | `/api/v1/teacher/bookings` | Teacher | Bookings for caller’s offerings. |

## Director

All require role **Director**.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/director/teachers` | Users with Teacher role (id, email, displayName). |
| GET | `/api/v1/director/teacher-offerings?teacherUserId={guid}` | List offerings; filter by teacher optional. |
| GET | `/api/v1/director/teacher-access-requests?status=Pending` | List access requests (`status` optional). |
| POST | `/api/v1/director/teacher-access-requests/{id}/approve` | Approve; sets applicant role to Teacher. |
| POST | `/api/v1/director/teacher-access-requests/{id}/reject` | Reject pending request. |
| POST | `/api/v1/director/subjects` | Body: `code`, `name`. Create subject. |
| PUT | `/api/v1/director/subjects/{id}` | Body: `code`, `name`. |
| DELETE | `/api/v1/director/subjects/{id}` | Delete if not referenced. |
| POST | `/api/v1/director/teacher-offerings` | Body: `teacherUserId`, `subjectId`, `courseTitle`, `gradeLevel`, optional `sectionLabel`. |
| PUT | `/api/v1/director/teacher-offerings/{offeringId}/weekly-availability` | Same body as teacher weekly PUT; any offering. |
| GET | `/api/v1/director/school-settings` | Time zone, UI language, `themePreset`, logo flags, audit fields. |
| PUT | `/api/v1/director/school-settings` | Body: `schoolTimeZoneId`, `uiLanguage`, `themePreset`. |
| POST | `/api/v1/director/school-logo` | Multipart field **`file`** (PNG/JPEG/SVG/WebP). Max file size: **`Scheduling:MaxSchoolLogoKb`** (kilobytes, default 1024 in `appsettings.json`, 2048 in `appsettings.Testing.json`; clamped 64–8192). |
| DELETE | `/api/v1/director/school-logo` | Remove custom logo (SPA falls back to default). |

## Booking payload shapes

- **Booking** responses include `sectionLabel`, **`studentSchoolEmail`**, **`interviewAttendeeName`**, **`relationshipToStudent`** (snapshot at booking time), subject/course/grade, and parent/teacher display fields (camelCase JSON).

## Errors

Validation failures return **400** with a problem body or plain text depending on the code path; conflicts (e.g. subject code duplicate) may return **409**.
