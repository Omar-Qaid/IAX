import React from 'react';
import { describe, it, expect } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { AppShell } from '@app/shell/AppShell';
import i18n from '@core/localization/i18n';
import { usePreferenceStore } from '@app/store/usePreferenceStore';

describe('AppShell', () => {
  it('renders top bar and content area', () => {
    render(
      <MemoryRouter>
        <AppProviders>
          <AppShell>
            <div data-testid="page-content">Dashboard Content</div>
          </AppShell>
        </AppProviders>
      </MemoryRouter>
    );

    expect(screen.getByTestId('page-content')).toBeDefined();
    expect(screen.getByTestId('page-content').textContent).toBe('Dashboard Content');
  });

  it('switches Arabic and English together with the page direction', async () => {
    await i18n.changeLanguage('en');
    usePreferenceStore.getState().setRtl(false);

    render(
      <MemoryRouter>
        <AppProviders>
          <AppShell>
            <div>Dashboard Content</div>
          </AppShell>
        </AppProviders>
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Account' }));
    fireEvent.click(await screen.findByText('العربية'));

    await waitFor(() => {
      expect(i18n.resolvedLanguage).toBe('ar');
      expect(document.documentElement.lang).toBe('ar');
      expect(document.documentElement.dir).toBe('rtl');
      expect(usePreferenceStore.getState().rtl).toBe(true);
    });

    fireEvent.click(screen.getByRole('button', { name: 'الحساب' }));
    fireEvent.click(await screen.findByText('English'));

    await waitFor(() => {
      expect(i18n.resolvedLanguage).toBe('en');
      expect(document.documentElement.lang).toBe('en');
      expect(document.documentElement.dir).toBe('ltr');
      expect(usePreferenceStore.getState().rtl).toBe(false);
    });
  });

  it('changes the language when the RTL setting is toggled', async () => {
    await i18n.changeLanguage('en');
    usePreferenceStore.getState().setRtl(false);

    render(
      <MemoryRouter>
        <AppProviders>
          <AppShell>
            <div>Dashboard Content</div>
          </AppShell>
        </AppProviders>
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Settings' }));
    const directionSwitch = await screen.findByRole('switch', { name: 'RTL direction' });
    fireEvent.click(directionSwitch);

    await waitFor(() => {
      expect(i18n.resolvedLanguage).toBe('ar');
      expect(document.documentElement.lang).toBe('ar');
      expect(document.documentElement.dir).toBe('rtl');
    });

    expect(directionSwitch.closest('.MuiDrawer-paper')).toHaveAttribute(
      'data-drawer-anchor',
      'right'
    );

    fireEvent.click(screen.getByRole('switch', { name: 'اتجاه من اليمين لليسار' }));

    await waitFor(() => {
      expect(i18n.resolvedLanguage).toBe('en');
      expect(document.documentElement.lang).toBe('en');
      expect(document.documentElement.dir).toBe('ltr');
    });
  });
});
