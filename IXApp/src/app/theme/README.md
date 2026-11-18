# Application theme

Builds the Material UI theme from palette, typography, spacing, shadows, component overrides, and theme type extensions. `createAppTheme.ts` combines preference mode/density with localization direction; `componentOverrides.ts` applies global component styling.

`createAppTheme` attaches `theme.palette.enterprise` from
[`shared/constants/enterpriseSurfaces`](../../shared/constants/enterpriseSurfaces.ts): the
page-pattern surfaces (canvas, surface, strong border, selected record, accent) resolved for the
active mode, primary color preset and high-contrast preference. Page patterns and shared chrome
read those surfaces through the theme instead of literal colors, so `enterpriseUiTokens` holds
measurements only. Components read them with `enterpriseSurfaces(theme)`, which falls back to the
same definition when a component renders under a plain Material UI theme.

Use semantic theme tokens in components. Add a token here only when it is truly application-wide.

[UI/UX and responsive standards](../../../docs/ui-ux-and-responsive.md) · [Preferences](../store/README.md)
