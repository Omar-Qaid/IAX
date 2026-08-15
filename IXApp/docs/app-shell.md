# Application shell (`src/app/shell`)

`AppShell` is the protected application frame. It renders the fixed `AppTopBar`, optional `AppSidebar`, scrollable main content, and the singleton command palette, notification drawer, and settings drawer.

## Components

- `AppTopBar`: product/module navigation, breadcrumbs in vertical mode, company display, command search, notifications, settings, help/feedback affordances, and account/logout menu.
- `AppSidebar`: vertical/mini module navigation and mobile drawer behavior.
- `ModuleNavPanel`: permission-filtered module sections and links.
- `AppCommandPalette`: keyboard/page search dialog; the store controls its visibility.
- `AppNotificationDrawer`: notification history surface backed by the shared notification store.
- `AppSettingsDrawer`: edits preference-store theme, density, direction, navigation, typography, and zoom values.
- `CompanySelector`, `GlobalSearch`, `NotificationMenu`, and `UserMenu`: smaller shell controls; not all are mounted directly by the current top bar.

## State and permissions

The shell reads `useNavigationStore`, `usePreferenceStore`, `useAppStore`, `useNotificationStore`, and `useAuth`. Navigation links are filtered by page registration and permission. `SystemAdmin`, wildcard permission, or the exact permission grants access through the auth service.

## Layout behavior

The top bar height is `LAYOUT.TOPBARHEIGHT` (58 px). Main content uses a flex layout with controlled overflow. Vertical navigation uses expanded or collapsed sidebar widths; horizontal navigation removes the sidebar. Below `md`, content takes full width and the menu button opens mobile navigation. A skip link targets `#main-content`.

Do not place feature commands in the top bar; use a page `ActionPane`. Do not calculate page widths from sidebar constants; the shell already provides the available content area.

See [Routing and layouts](routing-and-layouts.md) and [UI/UX and responsive design](ui-ux-and-responsive.md).
