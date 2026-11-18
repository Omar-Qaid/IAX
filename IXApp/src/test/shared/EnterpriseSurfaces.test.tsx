import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { RightUtilityRail } from '@shared/components/page/RightUtilityRail';
import { createAppTheme } from '@app/theme/createAppTheme';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { render as renderBare } from '@testing-library/react';
import { createEnterprisePalette } from '@shared/constants/enterpriseSurfaces';

/**
 * The enterprise surfaces are resolved by the theme, not by literal colors, so
 * they have to arrive as real CSS values. A palette path handed to a plain CSS
 * property (a logical border, for instance) would silently render nothing.
 */
describe('enterprise surfaces render resolved colors', () => {
  it('paints the utility rail from the theme', () => {
    const theme = createAppTheme('light', 'ltr');
    render(<RightUtilityRail filterLabel="Filter" informationLabel="Information" />);

    const rail = screen.getByRole('complementary', { name: 'Information' });

    expect(rail).toHaveStyle({ backgroundColor: theme.palette.enterprise.surface });
    expect(rail).toHaveStyle({
      borderInlineStart: `1px solid ${theme.palette.divider}`,
    });
  });
});

describe('enterprise surfaces resolve without the application theme', () => {
  it('still paints the utility rail under a plain Material UI theme', () => {
    // Shared components are rendered in isolation by many tests, where the theme
    // does not carry the enterprise surfaces. The resolver has to fall back to the
    // same definition rather than leaving the surface unpainted.
    const plainTheme = createTheme();
    const expected = createEnterprisePalette(plainTheme.palette);

    renderBare(
      <ThemeProvider theme={plainTheme}>
        <RightUtilityRail filterLabel="Filter" informationLabel="Information" />
      </ThemeProvider>
    );

    expect(screen.getByRole('complementary', { name: 'Information' })).toHaveStyle({
      backgroundColor: expected.surface,
    });
  });
});
