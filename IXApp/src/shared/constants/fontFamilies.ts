export const DEFAULT_UI_FONT_FAMILY =
  '"Segoe UI", "Segoe UI Web (West European)", Arial, sans-serif';

export const SAUDI_FONT_FAMILY = '"Saudi"';
export const TAJAWAL_FONT_FAMILY = '"Tajawal"';
export const CAIRO_FONT_FAMILY = '"Cairo"';
export const FS_ALBERT_ARABIC_FONT_FAMILY = '"FS Albert Arabic Web"';
export const IBM_PLEX_SANS_ARABIC_FONT_FAMILY = '"IBM Plex Sans Arabic"';
export const NOTO_KUFI_ARABIC_FONT_FAMILY = '"Noto Kufi Arabic"';

export const ARABIC_UI_FONT_FAMILIES = {
  saudi: `${SAUDI_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
  tajawal: `${TAJAWAL_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
  cairo: `${CAIRO_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
  ibmPlexSansArabic: `${IBM_PLEX_SANS_ARABIC_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
  notoKufiArabic: `${NOTO_KUFI_ARABIC_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
  fsAlbertArabic: `${FS_ALBERT_ARABIC_FONT_FAMILY}, ${DEFAULT_UI_FONT_FAMILY}`,
} as const;

export const DEFAULT_ARABIC_UI_FONT_FAMILY = ARABIC_UI_FONT_FAMILIES.saudi;

const ARABIC_FONT_NAMES = [
  SAUDI_FONT_FAMILY,
  TAJAWAL_FONT_FAMILY,
  CAIRO_FONT_FAMILY,
  FS_ALBERT_ARABIC_FONT_FAMILY,
  IBM_PLEX_SANS_ARABIC_FONT_FAMILY,
  NOTO_KUFI_ARABIC_FONT_FAMILY,
];

export function isArabicUiFontFamily(fontFamily: string): boolean {
  return ARABIC_FONT_NAMES.some((fontName) => fontFamily.includes(fontName));
}

export const APP_FONT_FAMILY_CSS_VARIABLE = '--app-font-family';

export const APP_FONT_FAMILY =
  `var(${APP_FONT_FAMILY_CSS_VARIABLE}, ${DEFAULT_UI_FONT_FAMILY})`;

export function resolveUiFontFamily(
  direction: 'ltr' | 'rtl',
  preferredFontFamily = DEFAULT_UI_FONT_FAMILY,
): string {
  if (direction === 'ltr') {
    return isArabicUiFontFamily(preferredFontFamily)
      ? DEFAULT_UI_FONT_FAMILY
      : preferredFontFamily;
  }

  if (isArabicUiFontFamily(preferredFontFamily)) {
    return preferredFontFamily;
  }

  return `${SAUDI_FONT_FAMILY}, ${preferredFontFamily}`;
}
