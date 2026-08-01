# Dialog Components Documentation (`src/shared/components/dialogs`)

## 1. Purpose and Responsibilities
The `dialogs` sub-system provides standardized modal overlay dialogs for **IXApp**. It includes general-purpose containers (`AppDialog`), action confirmation overlays (`ConfirmationDialog`), destructive warning prompts (`DeleteConfirmationDialog`), and specialized process dialogs.

All dialogs support keyboard accessibility (`Escape` key close, focus trapping), full responsiveness, LTR/RTL layout orientation, and compact header styling.

---

## 2. Folder Structure
```text
src/shared/components/dialogs/
├── AppDialog.tsx              # Base modal container (Title, Content, Actions)
├── ConfirmationDialog.tsx     # Generic confirmation dialog (Confirm / Cancel)
├── DeleteConfirmationDialog.tsx# Destructive delete confirmation warning modal
└── types.ts                   # Dialog interface contracts
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `AppDialog.tsx`, `ConfirmationDialog.tsx`, `DeleteConfirmationDialog.tsx`).
- **Props Interfaces:** `PascalCaseProps` (e.g., `AppDialogProps`, `ConfirmationDialogProps`).

---

## 4. Components
- **`AppDialog`:** Base container featuring clean divider borders, top close icon button (`CloseIcon`), customizable `maxWidth` (`'xs'` to `'xl'`), and `actions` slot.
- **`ConfirmationDialog`:** Renders title, message body, primary action button, and cancel button.
- **`DeleteConfirmationDialog`:** Specialized variant featuring error-colored warning styling (`color="error"`), item description highlight, and destructive action button (`Delete`).

---

## 5. Hooks & Integrations
Dialogs manage `open` and `onClose` state driven by parent component React state (`useState`) or page pattern hooks.

---

## 6. Services & APIs
Dialog components contain **no direct API service calls**. Submit or confirm button handlers execute callbacks provided by the parent view.

---

## 7. State Management
Dialog open/close state is kept local to the calling page or feature hook (`const [open, setOpen] = useState(false)`).

---

## 8. Design Patterns
- **Container Slot Pattern:** `AppDialog` exposes `title`, `children`, and `actions` slots.
- **Confirmation Pattern:** `DeleteConfirmationDialog` isolates warning messages and prevents accidental record deletions.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@mui/icons-material/Close`, `@core/localization`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always supply accessible labels for icon-only close buttons (`aria-label="close"`).
- Use `DeleteConfirmationDialog` for any destructive delete operation to require explicit user confirmation.

---

## 11. Do's and Don'ts
- **DO:** Disable confirm buttons during pending async saving states (`loading={isDeleting}`).
- **DON'T:** Create custom custom raw MUI `<Dialog>` primitives in pages—always wrap with `AppDialog` or `ConfirmationDialog`.

---

## 12. Code Example
```tsx
<DeleteConfirmationDialog
  open={isDeleteDialogOpen}
  title="Delete Customer"
  message="Are you sure you want to delete customer US-001? This operation cannot be undone."
  loading={isDeleting}
  onConfirm={handleConfirmDelete}
  onClose={() => setIsDeleteDialogOpen(false)}
/>
```

---

## 13. Decision Rules & Checklist
- [ ] Is `AppDialog` or `ConfirmationDialog` used instead of raw MUI Dialog?
- [ ] Is close icon button properly wired to `onClose`?
- [ ] Is loading spinner shown on submit button during async network operations?
