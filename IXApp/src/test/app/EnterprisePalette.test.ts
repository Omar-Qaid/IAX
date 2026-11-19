import { describe, expect, it } from 'vitest';
import { createAppTheme } from '@app/theme/createAppTheme';
import { enterpriseUiTokens } from '@shared/constants/enterpriseUiTokens';

describe('enterprise surface palette', () => {
  it('exposes the enterprise surfaces on every theme', () => {
    const theme = createAppTheme('light', 'ltr');

    expect(Object.keys(theme.palette.enterprise).sort()).toEqual([
      'accent',
      'accentStrong',
      'canvas',
      'scrollbarThumb',
      'scrollbarThumbHover',
      'selectedBar',
      'selectedRow',
      'strongBorder',
      'surface',
    ]);
  });

  it('resolves the canvas and surface for the active mode', () => {
    const light = createAppTheme('light', 'ltr');
    const dark = createAppTheme('dark', 'ltr');

    expect(light.palette.enterprise.surface).toBe(light.palette.background.paper);
    expect(dark.palette.enterprise.surface).toBe(dark.palette.background.paper);
    expect(dark.palette.enterprise.canvas).toBe(dark.palette.background.default);
    expect(dark.palette.enterprise.canvas).not.toBe(light.palette.enterprise.canvas);
  });

  it('follows the selected primary color preset', () => {
    const base = createAppTheme('light', 'ltr');
    const emerald = createAppTheme('light', 'ltr', { colorPreset: 'emerald' });

    expect(emerald.palette.enterprise.accent).toBe(emerald.palette.primary.main);
    expect(emerald.palette.enterprise.selectedBar).toBe(emerald.palette.primary.main);
    expect(emerald.palette.enterprise.accent).not.toBe(base.palette.enterprise.accent);
  });

  it('keeps colors out of the measurement tokens', () => {
    const colorLike = Object.entries(enterpriseUiTokens).filter(
      ([, value]) => typeof value === 'string' && value.startsWith('#')
    );

    expect(colorLike).toEqual([]);
  });
});
