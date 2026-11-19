import { alpha, type Theme } from '@mui/material/styles';

/**
 * Process Builder colors are CSS variables resolved from the active theme by
 * <ProcessBuilderThemeVariables />, so the builder follows light/dark mode,
 * high contrast and the primary color preset. The fallback after each
 * variable is the previous light value, used when the variables are absent
 * (for example a component rendered on its own in a test).
 */
const v = (name: string, fallback: string) => `var(--pb-${name}, ${fallback})`;

export const processBuilderColorVariables = (theme: Theme): Record<string, string> => {
  const { palette } = theme;
  return {
    '--pb-accent': palette.primary.main,
    '--pb-accent-hover': palette.primary.dark,
    '--pb-accent-soft': alpha(palette.primary.main, 0.08),
    '--pb-accent-light': alpha(palette.primary.main, 0.45),
    '--pb-border': palette.divider,
    '--pb-border-strong': alpha(palette.text.primary, 0.22),
    '--pb-divider-strong': palette.text.primary,
    '--pb-canvas': palette.background.default,
    '--pb-surface': palette.background.paper,
    '--pb-warning': palette.warning.main,
    '--pb-warning-soft': alpha(palette.warning.main, 0.14),
    '--pb-success': palette.success.main,
    '--pb-success-soft': alpha(palette.success.main, 0.1),
    '--pb-danger': palette.error.main,
    '--pb-text': palette.text.primary,
    '--pb-text-muted': palette.text.secondary,
    '--pb-text-subtle': palette.text.disabled,
    '--pb-focus-ring': alpha(palette.primary.main, 0.24),
  };
};

export const processBuilderTokens = {
  accent: v('accent', '#8b5cf6'),
  accentHover: v('accent-hover', '#7c3aed'),
  accentSoft: v('accent-soft', '#f6f2fe'),
  accentLight: v('accent-light', '#c5adfa'),
  border: v('border', '#e1e6ed'),
  borderStrong: v('border-strong', '#cbd2dc'),
  dividerStrong: v('divider-strong', '#1f2937'),
  canvas: v('canvas', '#fafafa'),
  /** Panels and cards (was literal white in the builder components). */
  surface: v('surface', '#ffffff'),
  warning: v('warning', '#f59e0b'),
  warningSoft: v('warning-soft', '#fff3cd'),
  success: v('success', '#16a34a'),
  successSoft: v('success-soft', '#ecfdf5'),
  danger: v('danger', '#ef4444'),
  text: v('text', '#1f2937'),
  textMuted: v('text-muted', '#667085'),
  textSubtle: v('text-subtle', '#94a3b8'),
  focusRing: `0 0 0 3px ${v('focus-ring', 'rgb(139 92 246 / 24%)')}`,
  radius: 2,
  controlHeight: 28,
  headerHeight: 40,
  tabsHeight: 52,
  leftWidth: 250,
  rightWidth: 320,
  fontSize: {
    caption: 9,
    secondary: 10,
    body: 11,
    heading: 14,
  },
  spacing: {
    compact: 4,
    control: 6,
    section: 12,
  },
} as const;
