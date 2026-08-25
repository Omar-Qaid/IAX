import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { queryClient } from '@core/api/queryClient';
import { settingsMockRepository } from '@modules/administration/adapters/settingsMockRepository';
import { ApplicationSettingsPage } from '@modules/administration/pages/ApplicationSettingsPage';
import {
  globalSettingsSchema,
  userSettingsSchema,
} from '@modules/administration/validation/settingsSchemas';

const renderPage = () =>
  render(
    <MemoryRouter>
      <AppProviders>
        <ApplicationSettingsPage />
      </AppProviders>
    </MemoryRouter>
  );

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
});

describe('ApplicationSettingsPage', () => {
  it('loads global settings and persists an edited application name', async () => {
    const user = userEvent.setup();
    const update = vi.spyOn(settingsMockRepository, 'updateGlobal');
    renderPage();

    const appName = await screen.findByRole('textbox', {
      name: /Application name/,
    });
    await user.clear(appName);
    await user.type(appName, 'IAX Enterprise');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => expect(update).toHaveBeenCalledTimes(1));
    expect(update.mock.calls[0]?.[0].appName).toBe('IAX Enterprise');
  });

  it('loads and saves authenticated user preferences separately', async () => {
    const user = userEvent.setup();
    const update = vi.spyOn(settingsMockRepository, 'updateUser');
    renderPage();

    const theme = await screen.findByRole('textbox', { name: /Theme/ });
    await user.clear(theme);
    await user.type(theme, 'dark');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => expect(update).toHaveBeenCalledTimes(1));
    expect(update.mock.calls[0]?.[0].theme).toBe('dark');
  });
});

describe('settings validation contracts', () => {
  it('matches the verified global numeric limits', () => {
    expect(
      globalSettingsSchema.safeParse({
        recId: 1,
        appName: 'IXApp',
        defaultLanguage: 'en',
        timeZone: 'Asia/Riyadh',
        currency: 'SAR',
        dateFormat: 'yyyy-MM-dd',
        enableAuditLog: true,
        maxUploadSize: 1024,
        paginationSize: 101,
        decimalPlaces: 2,
      }).success
    ).toBe(false);
  });

  it('matches the verified user page-size limits', () => {
    expect(
      userSettingsSchema.safeParse({
        recId: 1,
        userId: 'usr-001',
        theme: 'light',
        language: 'en',
        pageSize: 0,
        notificationEnabled: true,
        dashboardLayout: 'default',
      }).success
    ).toBe(false);
  });
});
