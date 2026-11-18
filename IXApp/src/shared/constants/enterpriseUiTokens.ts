import { APP_FONT_FAMILY } from './fontFamilies';
import { uiDensity } from './uiDensity';

/**
 * Shared enterprise UI measurements used by route-agnostic primitives and page
 * patterns. Business modules should consume these through shared UI components
 * rather than depending on the token object directly.
 *
 * Colors are not tokens here: the enterprise surfaces live on the theme as
 * `theme.palette.enterprise` so they follow the active mode and primary color.
 * Measurement names that describe a different control than the same-named
 * `uiDensity` entry are prefixed, so no two tokens share a name with different
 * values.
 */
export const enterpriseUiTokens = {
  fontFamily: APP_FONT_FAMILY,
  fontSize: 12,
  labelFontSize: 10,
  titleFontSize: 20,
  /** Height of a form field control (the list grid uses `uiDensity.controlHeight`). */
  fieldControlHeight: 28,
  /** Row height of a tabular detail panel (the list grid uses `uiDensity.gridRowHeight`). */
  detailGridRowHeight: 28,
  /** Header height of a tabular detail panel (the list grid uses `uiDensity.gridHeaderHeight`). */
  detailGridHeaderHeight: 30,
  toolbarHeight: uiDensity.toolbarHeight,
  sectionHeaderHeight: 38,
  listWidth: 260,
  utilityRailWidth: 42,
  radius: 2,
  sectionRadius: 7,
  sectionGap: 4,
  fieldGap: 8,
} as const;

/** Compatibility alias used throughout existing enterprise page patterns. */
export const d365 = enterpriseUiTokens;
