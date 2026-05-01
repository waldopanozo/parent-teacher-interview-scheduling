# HTTP API reference (v1)

Base URL examples:

- Through the SPA/nginx: `http://localhost:3456/api/v1/...`
- Direct API: `http://localhost:5103/api/v1/...`

Unless noted, requests send header `Authorization: Bearer <jwt>`.

## Auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/v1/auth/google` | No | Body: `{ "idToken": "<google credential>" }`. Returns access token + user profile (`role`: Parent=0, Teacher=1, Director=2; `meetingProfileComplete` for parents). |
| GET | `/api/v1/auth/me` | Yes | Current user profile (same shape as login `user`, including `meetingProfileComplete`). |

## Catalog (authenticated)

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/v1/subjects` | Any authenticated | List subjects. |
| GET | `/api/v1/catalog/teacher-offerings` | Any authenticated | List offerings with teacher, subject, course, grade, `sectionLabel`. |
| GET | `/api/v1/catalog/teacher-offerings/{id}/slots?date=YYYY-MM-DD` | Any authenticated | Available slot start/end (UTC) for that local calendar date. |

## Parent

| Method | Path | Role | Description |
|--------|------|------|-------------|
| GET | `/api/v1/parent/bookings` | Parent | List caller’s bookings. |
| POST | `/api/v1/parent/bookings` | Parent | Body: `teacherOfferingId`, `startUtc`. Creates booking (requires completed meeting profile; **400** if not). |
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

## Booking payload shapes

- **Booking** responses include `sectionLabel`, **`studentSchoolEmail`**, **`interviewAttendeeName`**, **`relationshipToStudent`** (snapshot at booking time), subject/course/grade, and parent/teacher display fields (camelCase JSON).

## Errors

Validation failures return **400** with a problem body or plain text depending on the code path; conflicts (e.g. subject code duplicate) may return **409**.
