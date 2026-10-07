# Case Study – What changed

## New file
- `AppHelpers.vb` (added to the .vbproj): shared validation, input filters, password hashing, friendly DB errors, and consistent styling.

## Validation
- Inline red-icon errors (ErrorProvider) show ALL problems at once instead of one pop-up per field.
- Students: LRN = exactly 12 digits, PH mobile number (09XXXXXXXXX / +639XXXXXXXXX), names letters only, duplicate Student ID / LRN checked before saving (also on edit).
- Users: username 4-30 chars, password min 8 with letters + numbers, not containing the username, duplicate check is case-insensitive.
- Documents: fee must be a valid amount between 0 and 99,999.99.
- Keystroke filters: digits-only LRN, money-only fee, letters-only names.
- Database errors are translated into plain language (MySQL not running, duplicate record, record in use...).

## Security
- Passwords are now stored as salted PBKDF2 hashes. Old plain-text passwords still work and are upgraded automatically on first successful login.
- Passwords are no longer loaded into the grid, shown, or searchable in the Users screen.
- Login: same message for wrong user/password, 5 failed attempts = 30 s lockout, inactive accounts get a clear message.

## User-friendliness
- Enter key submits login / moves to the next field on forms; Show-password checkbox; tooltips on fields.
- Unsaved-changes and "clear form" confirmations; confirm before updating a record.
- Users screen: Edit button now works (it had no handler before); leave password blank to keep it.
- Consistent button colours (blue = main action, grey = secondary, red = delete/deactivate), styled read-only grids, "No records found" hints on empty lists.
- Sidebar navigation buttons were intentionally left with your original design.

## Still recommended (not changed)
- Connection string (root, empty password) is hard-coded in DatabaseConnection.vb: create a dedicated MySQL user and move it to App.config.
