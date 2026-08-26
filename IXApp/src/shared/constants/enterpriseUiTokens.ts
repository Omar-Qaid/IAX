import { APP_FONT_FAMILY } from './fontFamilies';

/**
 * Shared enterprise UI measurements and colors used by route-agnostic primitives
 * and page patterns. Business modules should consume these through shared UI
 * components rather than depending on the token object directly.
 */
export const enterpriseUiTokens = {
  fontFamily: APP_FONT_FAMILY,
  fontSize: 12,
  labelFontSize: 10,
  titleFontSize: 20,
  primary: '#315efb',
  text: '#1b1a19',
  mutedText: '#605e5c',
  canvas: '#faf9f8',
  surface: '#ffffff',
  border: '#c8c6c4',
  darkBorder: '#8a8886',
  selectedRow: '#d4e0f7',
  selectedBar: '#315efb',
  disabledFill: '#f3f2f1',
  toolbarHeight: 36,
  controlHeight: 28,
  sectionHeaderHeight: 38,
  gridHeaderHeight: 30,
  gridRowHeight: 28,
  listWidth: 260,
  utilityRailWidth: 42,
  radius: 2,
  sectionRadius: 7,
  sectionGap: 4,
  fieldGap: 8,
} as const;

/** Compatibility alias used throughout existing enterprise page patterns. */
export const d365 = enterpriseUiTokens;
