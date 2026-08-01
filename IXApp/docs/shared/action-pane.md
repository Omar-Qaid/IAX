# Action Pane Component Documentation (`src/shared/components/action-pane`)

## 1. Purpose and Responsibilities
The `action-pane` component sub-system provides a Microsoft Dynamics 365 Finance & Operations-style command bar header. It organizes commands into functional action groups (`New`, `Maintain`, `Process`, `Inquiries`, `Print`, `Options`), enforces RBAC permission visibility, tracks loading/disabled states, and provides horizontal overflow scrolling on smaller screens.

---

## 2. Folder Structure
```text
src/shared/components/action-pane/
├── ActionPane.tsx             # Main command bar container
├── ActionPaneGroup.tsx        # Command grouping container with divider
├── ActionPaneButton.tsx       # Action button with icon, loading & RBAC guard
├── ActionPaneMenu.tsx         # Dropdown menu button for grouped sub-actions
├── ActionPaneDivider.tsx      # Vertical separator between action groups
└── types.ts                   # PageAction & ActionPaneProps interface contracts
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `ActionPane.tsx`, `ActionPaneButton.tsx`).
- **Interfaces & Types:** `PascalCase` (e.g., `PageAction`, `ActionPaneProps`).

---

## 4. Components
- **`ActionPane`:** Main paper container with horizontal scroll overflow.
- **`ActionPaneGroup`:** Renders visual action button clusters separated by dividers.
- **`ActionPaneButton`:** Button executing an action. Automatically evaluates permissions (`usePermissions`) and loading state.

---

## 5. Hooks & Integrations
- Integrates with `usePermissions` (`@core/auth/usePermissions`) to hide or disable buttons when user lacks required permission.

---

## 6. Services & APIs
Contains zero direct API calls. Emits `onClick` events back to the parent page pattern or custom hook.

---

## 7. State Management
Actions are configuration-driven via props. State (such as button loading or disabled flags) is passed down from page hooks.

---

## 8. Design Patterns
- **Command Pattern:** Encapsulates actions into `PageAction` configuration objects.
- **Guard Pattern:** Hides or disables buttons automatically if `permission` check fails.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@core/auth/usePermissions`.
- **Forbidden:** No business module imports.

---

## 10. Data Flow
Page defines `PageAction[]` $\rightarrow$ passes to `ActionPane` $\rightarrow$ `ActionPaneButton` evaluates permission & renders icon/label $\rightarrow$ user clicks $\rightarrow$ `action.onClick()` executes.

---

## 11. Best Practices & Reusability Rules
- Icon imports in button definitions **must** use specific path imports (`@mui/icons-material/Add`).
- Group actions logically: `New` (create), `Maintain` (edit/delete/save/cancel), `Process` (post/confirm), `Print`, `Options`.

---

## 12. Generic Implementation Guidelines
Always wrap individual buttons inside `ActionPaneGroup` to ensure proper spacing and enterprise visual divider grouping.

---

## 13. Do's and Don'ts
- **DO:** Set `loading={isSaving}` on Save buttons to prevent double submission.
- **DON'T:** Place business API requests inside `ActionPaneButton`.

---

## 14. Common Mistakes
- **Mistake:** Using barrel icon imports in action objects (`import { Add } from '@mui/icons-material'`).
- **Correction:** Use `import AddIcon from '@mui/icons-material/Add'`.

---

## 15. Code Example
```tsx
const actions: PageAction[] = [
  {
    id: 'new-customer',
    label: 'New',
    icon: <AddIcon fontSize="small" />,
    group: 'New',
    permission: 'AccountsReceivable.Customers.Create',
    onClick: handleNew,
  },
  {
    id: 'save-customer',
    label: 'Save',
    icon: <SaveIcon fontSize="small" />,
    group: 'Maintain',
    loading: isSaving,
    onClick: handleSave,
  },
];

<ActionPane>
  <ActionPaneGroup>
    {actions.map((action) => (
      <ActionPaneButton key={action.id} action={action} />
    ))}
  </ActionPaneGroup>
</ActionPane>
```

---

## 16. Decision Rules & Checklist
- [ ] Are action icons imported via specific path imports?
- [ ] Is permission string assigned if command requires RBAC check?
- [ ] Is loading state bound for async operations?

---

## 17. Extension Guidelines
To add a dropdown action menu, create an action with sub-items and use `ActionPaneMenu.tsx`.

---

## 18. Performance Considerations
Actions array should be memoized using `useMemo` in parent page components.
