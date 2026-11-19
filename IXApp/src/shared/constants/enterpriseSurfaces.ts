import { alpha, type Palette, type Theme } from '@mui/material/styles';

/**
 * Enterprise surface colors used by the page patterns and shared page chrome
 * (page canvas, side rails, record lists, field underlines, lookup accents).
 *
 * These were literal light-mode hex values in `enterpriseUiTokens`, which made
 * the affected surfaces ignore the theme mode and the primary color preset.
 * They now live on the theme so every consumer resolves them for the active
 * mode and preset through `theme.palette.enterprise`.
 */
export interface EnterprisePalette {
  /** Page canvas behind the sections of a details page. */
  canvas: string;
  /** Panels, side rails and section surfaces that sit on the canvas. */
  surface: string;
  /** Stronger separator than `divider`: field underlines, record-list rows. */
  strongBorder: string;
  /** Background of the selected record in a navigation list. */
  selectedRow: string;
  /** Leading indicator bar of the selected record. */
  selectedBar: string;
  /** Links, lookup values and other interactive text accents. */
  accent: string;
  /** Text of the selected record, and other emphasized accents. */
  accentStrong: string;
  /** Column and cell lines of data grids (visible, lighter than strongBorder). */
  gridLine: string;
  /** Scrollbar thumb of every scroll area (global default and data grids). */
  scrollbarThumb: string;
  /** Scrollbar thumb under the pointer. */
  scrollbarThumbHover: string;
}

/**
 * Light-mode values preserve the previous `enterpriseUiTokens` appearance,
 * except under high contrast, where the canvas follows the palette so it does
 * not stay tinted while the rest of the application turns white.
 */
export const createEnterprisePalette = (
  palette: Palette,
  highContrast = false
): EnterprisePalette =>
  palette.mode === 'light'
    ? {
        canvas: highContrast ? palette.background.default : '#faf9f8',
        surface: palette.background.paper,
        strongBorder: '#8a8886',
        selectedRow: alpha(palette.primary.main, 0.16),
        selectedBar: palette.primary.main,
        accent: palette.primary.main,
        accentStrong: palette.primary.dark,
        gridLine: '#d2d0ce',
        scrollbarThumb: '#a8a8a8',
        scrollbarThumbHover: '#8f8f8f',
      }
    : {
        canvas: palette.background.default,
        surface: palette.background.paper,
        strongBorder: '#6b6a69',
        selectedRow: alpha(palette.primary.main, 0.22),
        selectedBar: palette.primary.main,
        accent: palette.primary.light,
        accentStrong: palette.primary.light,
        gridLine: '#484644',
        scrollbarThumb: '#667085',
        scrollbarThumbHover: '#8290a3',
      };

/**
 * Reads the surfaces from a theme. The application theme carries them on its
 * palette; a component rendered under a plain Material UI theme (an isolated
 * test, for instance) falls back to the same definition instead of failing.
 */
export const enterpriseSurfaces = (theme: Theme): EnterprisePalette => {
  // The palette type declares the surfaces, but a theme built outside the
  // application does not carry them, so this reads them defensively.
  const { enterprise } = theme.palette as Partial<Palette>;
  return enterprise ?? createEnterprisePalette(theme.palette);
};

/** Augmentation so `theme.palette.enterprise` is typed for every consumer. */
declare module '@mui/material/styles' {
  interface Palette {
    enterprise: EnterprisePalette;
  }
  interface PaletteOptions {
    enterprise?: EnterprisePalette;
  }
}
