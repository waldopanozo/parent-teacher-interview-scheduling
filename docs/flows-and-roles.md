# Flows and roles

## Role assignment (first sign-in)

1. User signs in with Google; the API validates the ID token.
2. If the email appears in `DIRECTOR_BOOTSTRAP_EMAILS` → **Director** (highest precedence).
3. Else if in `TEACHER_BOOTSTRAP_EMAILS` → **Teacher**.
4. Else → **Parent**.

Existing users keep their stored role; bootstrap lists only apply when the account is **created**.

## Parent → Teacher via access request

1. User signs in as **Parent**.
2. Opens **Request teacher access** (`/app/parent/request-teacher`), optionally adds a message, submits.
3. A **Director** opens the Director workspace, sees the request, **Approve** or **Reject**.
4. If **approved**, the database role becomes **Teacher**, but the current JWT still says Parent until the user **signs out and signs in again**.

## Director operations

- **Teacher access requests**: list (optional `?status=Pending|Approved|Rejected`), approve, reject.
- **Subjects**: create, update, delete (delete blocked if any offering still references the subject).
- **Offerings**: create an offering for a chosen **teacher** (subject, course title, grade, optional section).
- **Weekly availability**: same payload as teachers—**replaces** all windows for the selected offering (director can edit any offering).

## Section labels (`sectionLabel`)

One teacher can teach the same subject/grade in **multiple parallel groups**. Model each group as its own **TeacherOffering** with the same subject/course/grade but different **`sectionLabel`** (examples: `A`, `C`, `3ro-A`). Parents then pick the correct row in the catalog.

## Time zones and slots

- Weekly templates use **local** wall-clock times in the configured **IANA** time zone (`Scheduling:SchoolTimeZoneId` in appsettings / Compose).
- Booked slots are stored in **UTC**; the UI may show UTC labels depending on configuration—verify before production use.

## Security notes (demo scope)

- JWT is stateless; revocation is not modeled—short `AccessTokenMinutes` helps.
- Director bootstrap emails are configuration secrets in real deployments—treat `.env` as sensitive.
