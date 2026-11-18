import { ThemeProvider } from '@mui/material/styles';
import { createAppTheme } from '@app/theme/createAppTheme';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@test/testUtils';
import { FastTabs } from '@shared/components/fast-tabs/FastTabs';
import { FastTab } from '@shared/components/fast-tabs/FastTab';

describe('FastTabs Component Suite', () => {
  it('renders title and toggles accordion expanded content', () => {
    render(
      <FastTabs>
        <FastTab id="tab-1" title="General Information" summary="Basic Details">
          <div>FastTab Inner Content</div>
        </FastTab>
      </FastTabs>
    );

    expect(screen.getByText('General Information')).toBeInTheDocument();
    expect(screen.getByText('FastTab Inner Content')).toBeInTheDocument();

    const summaryHeader = screen.getByText('General Information');
    fireEvent.click(summaryHeader);

    expect(screen.getByText('General Information')).toBeInTheDocument();
  });
  it('keeps controlled expansion and rich summaries independent between sections', () => {
    const changed = vi.fn();
    render(
      <>
        <FastTab
          title="Controlled"
          appearance="enterprise"
          expanded={false}
          onExpandedChange={changed}
          summary={<span>Account 123</span>}
        >
          <div>Controlled content</div>
        </FastTab>
        <FastTab title="Independent" defaultExpanded>
          <div>Other content</div>
        </FastTab>
      </>
    );
    const controlled = screen.getByRole('button', { name: /Controlled/ });
    const independent = screen.getByRole('button', { name: /Independent/ });
    expect(controlled).toHaveAttribute('aria-expanded', 'false');
    fireEvent.click(controlled);
    expect(changed).toHaveBeenCalledWith(true);
    expect(controlled).toHaveAttribute('aria-expanded', 'false');
    expect(independent).toHaveAttribute('aria-expanded', 'true');
    fireEvent.click(independent);
    expect(independent).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Account 123')).toBeVisible();
    expect(controlled.getAttribute('aria-controls')).not.toBe(
      independent.getAttribute('aria-controls')
    );
    expect(document.getElementById(controlled.getAttribute('aria-controls')!)).toHaveAttribute(
      'aria-labelledby',
      controlled.id
    );
  });
  it.each(['light', 'dark'] as const)(
    'keeps %s section styles and local padding isolated',
    (mode) => {
      const theme = createAppTheme(mode);
      const { container } = render(
        <ThemeProvider theme={theme}>
          <FastTab title="Customized" appearance="enterprise" detailsSx={{ padding: '27px' }}>
            <FastTab title="Nested standard">
              <span>Nested content</span>
            </FastTab>
          </FastTab>
          <FastTab title="Default enterprise" appearance="enterprise">
            <span>Default content</span>
          </FastTab>
        </ThemeProvider>
      );
      const details = container.querySelectorAll('.MuiAccordionDetails-root');
      expect(details[0]).toHaveStyle({ padding: '27px' });
      expect(details[2]).toHaveStyle({ padding: '11px 10px 10px' });
      expect(details[1]).not.toHaveStyle({ padding: '11px 10px 10px' });
      const section = screen
        .getByRole('button', { name: 'Default enterprise' })
        .closest('.MuiAccordion-root')!;
      expect(section).toHaveStyle({ backgroundColor: theme.palette.background.paper });
    }
  );
});
