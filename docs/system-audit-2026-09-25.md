# System audit — 25 September 2026

This pass covers the active `NHIGIA.Modern` application: navigation, procedure forms, approvals, leave accounting, work modules, asset editing and payroll data preservation. It does not certify every production workflow as error-free.

## Changes

- Navigation: move sidebar behaviour into a dedicated script with guarded local storage; preserve section collapse state, update accessibility attributes, support Escape/backdrop dismissal, and use a drawer below 1280 px. Keep logo/menu targets separate on small screens. Fix active route highlighting and the mobile approvals drawer/search overflow.
- Procedures and attachments: validate required fields, selected employees, dates, expenses and files before submission; safely escape employee/request text. Submit actual multipart attachments from the personal procedure form and link stored attachments to the authorized download endpoint. Reject forged attachment metadata. Preserve form inputs after errors.
- Approvals: provide one pending inbox for all supported request sources. Single and bulk leave approvals use the same permission and state policy, excluding self-approval and managers approving outside their department or approving peers. Apply corresponding restrictions to direct overtime/resignation/KPI transitions and check expected states before updating. AJAX authentication failures return JSON instead of login HTML.
- Leave accounting: count approved annual leave only, clip to the selected year, merge overlapping half days and use assigned working days, including half-day Saturday. Recognize legacy subtype text so sick leave and business trips do not spend annual leave. Existing records without any active schedule retain calendar-day accounting.
- Work modules: scope lists and payroll components to the current account, suppress unpublished payroll for employees/managers, remove runtime demo insertion and fabricated recurring training sessions, use current Vietnam periods, and expose a month picker for payroll. Propagate database errors and disable mutation controls while keeping modal cancellation available.
- Assets: allocate the next sequential code on the server under a transaction; keep it read-only. Editing descriptive fields preserves the assignee, department, handover date, code and lifecycle status. Validate allocation/recovery state and permissions. Prevent deletion of allocated assets or assets with handover history. Reopen failed saves with the entered values and validation messages.
- Meeting booking: repeat the room-conflict check inside the insertion transaction, including the existing 10-minute separation rule.
- Payroll: display stored tax/insurance details instead of fabricated dependent counts, deductions or contribution estimates. Missing metadata is shown as unknown, explicit zero remains zero. Recalculation retains unrelated JSON fields, refuses malformed metadata and only updates editable draft/rejected payroll.

## Validation completed

- `dotnet build NHIGIA.Modern/NHIGIA.Modern.csproj -c Release --no-restore`: zero warnings/errors.
- `dotnet run --project tests/RequestPolicy.Tests/RequestPolicy.Tests.csproj -c Release --no-restore`: 39 permission, leave-accounting and payroll metadata cases passed.
- `dotnet run --project tests/AttendanceLeave.Tests/AttendanceLeave.Tests.csproj -c Release --no-restore`: 24 attendance/approved-leave cases passed.
- `dotnet run --project tests/WorkModules.Smoke/WorkModules.Smoke.csproj -c Release -- NHIGIA.Modern --snapshots`: local rendered pages, role access, anti-forgery checks, JSON session expiry and failed asset-save retention passed.
- The smoke suite's `--attendance-only` mode passed for attendance, assigned schedules and date validation.
- `tests/WorkModules.Smoke/browser.cjs`: headless Edge checks passed at 320, 390, 768, 1024 and 1440 px. Verified section toggles, unavailable local storage, menu/header geometry, pending-inbox actions using fixtures, negative advance validation, multipart upload, failed asset-form restoration, modal cancellation and rendered module scripts without uncaught JavaScript errors.

To run browser checks, first produce snapshots using the smoke command above. Set `HRM_TEST_PLAYWRIGHT` to an installed Playwright package directory and run `node tests/WorkModules.Smoke/browser.cjs`. `HRM_TEST_BROWSER` defaults to `msedge`. Snapshots and screenshots in `.tmp-audit-ui` are local test artifacts and are not committed.

## Data preservation and remaining verification

The test host explicitly replaces the inherited SQL connection with an unavailable loopback endpoint and uses short-lived local authentication. Browser checks block external network calls and use local fixtures. No production SQL records were created, modified, deleted or reseeded for this audit.

The startup schema script widens `HrmLeaveRequest.Reason` to `NVARCHAR(MAX)` when the existing field is shorter than the supported request payload. It preserves stored values; this migration was not executed against production during the audit.

SQL writes, transaction concurrency, schema migration and real HANET/TopCV/CareerViet/Gemini round trips still require verification on a dedicated test database/environment. The role-scoped SQL changes were reviewed and compiled, not exercised against production. The tax/insurance detail display is based on persisted metadata and is not a new statutory payroll engine. Delivery stops at Git push as requested; Render deployment is not monitored in this pass.
