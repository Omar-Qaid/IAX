# Feedback components

The feedback family provides explicit async and result states:

- `LoadingState`: spinner and optional message.
- `ErrorState`: title/message and optional retry callback.
- `EmptyState`: title/message and optional action.
- `NoResultsState`: filtered-empty state with optional clear action.
- `EmptyDataWatermark`: lightweight grid/form watermark.
- `AccessDeniedState`: route-agnostic authorization message with optional action.
- `AppAlert` and `AppNotification`: reusable alert/snackbar wrappers.

The app-owned `AppAccessDeniedState` adds navigation behavior for routes. Global operation notifications are queued by `useNotificationStore` and rendered one at a time by `NotificationProvider`.

Handle initial loading before empty data, preserve retry for recoverable fetch errors, and keep validation errors close to the form. Do not render a blank page while a request is pending or failed.
