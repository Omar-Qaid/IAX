# App Shell Components Documentation (`src/shared/components/app-shell`)

## 1. Purpose and Responsibilities
The `app-shell` sub-system provides the main enterprise application frame for **IXApp**. Inspired by Microsoft Dynamics 365 Finance & Operations, it wraps pages with a top navigation bar, dynamic sidebar navigation, quick command palette (`Ctrl+K`), notification drawer, application settings panel, and breadcrumb navigation.

---

## 2. Folder Structure
```text
src/shared/components/app-shell/
├── AppShell.tsx               # Main layout frame container
├── AppTopBar.tsx              # Top bar (Logo, search, company selector, theme toggle, profile)
├── AppSidebar.tsx             # Collapsible module navigation sidebar
├── AppCommandPalette.tsx      # Quick command palette modal (Ctrl+K)
├── AppNotificationDrawer.tsx  # Slide-out notification center drawer
├── AppSettingsDrawer.tsx      # App visual preferences drawer (Density, direction, mode)
├── CompanySelector.tsx        # Legal entity / company selection dropdown
├── UserMenu.tsx               # User profile & logout menu
└── types.ts                   # Shell types & navigation item contracts
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` prefixed with `App` (e.g., `AppShell.tsx`, `AppTopBar.tsx`, `AppSidebar.tsx`).

---

## 4. Components
- **`AppShell`:** Top-level container managing layout spacing, main scroll container, and persistent drawers.
- **`AppTopBar`:** Top app bar featuring global search trigger, company selector, notification bell badge, settings button, and user profile menu.
- **`AppSidebar`:** Multi-level navigation tree supporting mini-collapsed state and full-expanded module navigation.
- **`AppCommandPalette`:** Shortcut search dialog (`Ctrl+K` or search click) allowing instant jump to any page or module command.

---

## 5. Hooks & Integrations
- Reads global UI preferences from `useNavigationStore` (`sidebarOpen`, `navLayout`) and `useThemeStore` (`themeMode`, `direction`).

---

## 6. Services & APIs
Contains zero direct API calls. Interacts with `useNavigationStore` for menu navigation states.

---

## 7. State Management
- Sidebar open/collapsed state: Zustand `useNavigationStore`.
- Search query and modal states: Local component state.

---

## 8. Design Patterns
- **Container / Frame Pattern:** Wraps active page views inside consistent top bar and side navigation bounds.
- **Responsive Drawer Pattern:** Permanent drawer on desktop, auto-collapsing overlay drawer on mobile.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@app/store/useNavigationStore`, `@app/store/useThemeStore`, `@core/localization`.
- **Forbidden:** Must not depend on domain business modules (`@modules/*`).

---

## 10. Data Flow
`AppShell` mounts `AppTopBar` and `AppSidebar` $\rightarrow$ reads `useNavigationStore` for sidebar width calculation $\rightarrow$ renders `<PageBreadcrumbs />` and page `children` inside `<main>` scroll container.

---

## 11. Best Practices
- Keep top bar height fixed (`LAYOUT.TOPBARHEIGHT = 48px`).
- Ensure sidebar transitions smoothly between collapsed ($64\text{px}$) and expanded ($240\text{px}$) widths.

---

## 12. Do's and Don'ts
- **DO:** Support keyboard shortcut `Ctrl+K` for command palette.
- **DON'T:** Place page-level action buttons inside `AppTopBar`. Action buttons belong in `ActionPane`.

---

## 13. Common Mistakes
- **Mistake:** Hardcoding sidebar width in page components.
- **Correction:** Let `AppShell` calculate content bounds dynamically using `finalSidebarWidth`.

---

## 14. Code Example
```tsx
export function CustomLayout({ children }: { children: React.ReactNode }) {
  return <AppShell>{children}</AppShell>;
}
```

---

## 15. Decision Rules & Checklist
- [ ] Does topbar render correctly in RTL mode?
- [ ] Is `AppCommandPalette` triggered by `Ctrl+K`?
- [ ] Is mobile breakpoint handled cleanly?
