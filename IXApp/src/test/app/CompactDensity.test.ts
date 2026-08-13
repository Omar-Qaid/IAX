import { describe, expect, it } from 'vitest';
import { createAppTheme } from '@app/theme/createAppTheme';
import { uiDensity } from '@shared/constants/uiDensity';

describe('application compact density', () => {
  it('centralizes compact measurements used by shared UI primitives', () => {
    expect(uiDensity.controlHeight).toBe(30);
    expect(uiDensity.gridRowHeight).toBe(32);
    expect(uiDensity.gridHeaderHeight).toBe(64);
    expect(uiDensity.dialogPadding).toBe(12);
  });

  it('applies compact defaults to common MUI components', () => {
    const theme = createAppTheme('light', 'ltr', { density: 'compact' });

    expect(theme.spacing(1)).toBe('4px');
    expect(theme.components?.MuiToolbar?.styleOverrides).toBeDefined();
    expect(theme.components?.MuiDialogContent?.styleOverrides).toBeDefined();
    expect(theme.components?.MuiTableCell?.styleOverrides).toBeDefined();
  });
});
