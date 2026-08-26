import { describe, expect, it } from 'vitest';
import {
  ARABIC_UI_FONT_FAMILIES,
  DEFAULT_UI_FONT_FAMILY,
  SAUDI_FONT_FAMILY,
  resolveUiFontFamily,
} from '@shared/constants/fontFamilies';
import { createAppTheme } from '@app/theme/createAppTheme';

describe('direction-aware typography', () => {
  it('preserves the configured font family in LTR', () => {
    expect(resolveUiFontFamily('ltr', DEFAULT_UI_FONT_FAMILY)).toBe(DEFAULT_UI_FONT_FAMILY);
  });

  it('prepends Saudi Font in RTL without duplicating it', () => {
    const resolved = resolveUiFontFamily('rtl', DEFAULT_UI_FONT_FAMILY);

    expect(resolved.startsWith(SAUDI_FONT_FAMILY)).toBe(true);
    expect(resolveUiFontFamily('rtl', resolved)).toBe(resolved);
  });

  it('preserves an explicitly selected Arabic font in RTL', () => {
    expect(resolveUiFontFamily('rtl', ARABIC_UI_FONT_FAMILIES.tajawal)).toBe(
      ARABIC_UI_FONT_FAMILIES.tajawal,
    );
  });

  it('supports the user-supplied FS Albert Arabic font', () => {
    expect(resolveUiFontFamily('rtl', ARABIC_UI_FONT_FAMILIES.fsAlbertArabic)).toBe(
      ARABIC_UI_FONT_FAMILIES.fsAlbertArabic,
    );
  });

  it('does not carry an Arabic-only selection into LTR', () => {
    expect(resolveUiFontFamily('ltr', ARABIC_UI_FONT_FAMILIES.cairo)).toBe(
      DEFAULT_UI_FONT_FAMILY,
    );
  });

  it('uses Saudi Font in the default RTL MUI theme only', () => {
    const rtlTheme = createAppTheme('light', 'rtl');
    const ltrTheme = createAppTheme('light', 'ltr');

    expect(rtlTheme.typography.fontFamily).toContain(SAUDI_FONT_FAMILY);
    expect(ltrTheme.typography.fontFamily).not.toContain(SAUDI_FONT_FAMILY);
  });
});
