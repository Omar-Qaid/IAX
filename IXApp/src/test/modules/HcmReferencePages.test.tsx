import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { HcmNationalityPage } from '@modules/organization/pages/HcmNationalityPage';
import { HcmOccupationPage } from '@modules/organization/pages/HcmOccupationPage';
import { HcmDepartmentPage } from '@modules/organization/pages/HcmDepartmentPage';
import { hcmNationalityApi, hcmOccupationApi, hcmDepartmentApi } from '@modules/organization/api/hcmReferenceApi';

vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({
    t: (key: string) => key,
    currentLanguage: { code: 'en' },
  }),
}));

vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({
    user: { id: '1', name: 'Test User' },
    isAuthenticated: true,
  }),
}));

vi.mock('@core/permissions/usePermission', () => ({
  usePermission: () => true,
  useHasPermission: () => true,
}));

vi.mock('@shared/hooks/useNotifications', () => ({
  useNotifications: () => ({
    notifyError: vi.fn(),
    notifySuccess: vi.fn(),
  }),
}));

vi.mock('@modules/organization/api/hcmReferenceApi', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@modules/organization/api/hcmReferenceApi')>();
  return {
    ...actual,
    hcmNationalityApi: {
      list: vi.fn().mockResolvedValue([
        {
          id: '1',
          recId: 1,
          code: 'NAT1',
          name: 'Saudi',
          nameAlias: 'سعودي',
          description: 'Saudi Arabia',
          isActive: true,
          dataAreaId: 'dat',
          recVersion: 1,
          rowVersion: null,
        },
      ]),
      create: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    },
    hcmOccupationApi: {
      list: vi.fn().mockResolvedValue([
        {
          id: '1',
          recId: 1,
          code: 'OCC1',
          name: 'Software Engineer',
          nameAlias: 'مهندس برمجيات',
          description: 'IT Department',
          isActive: true,
          dataAreaId: 'dat',
          recVersion: 1,
          rowVersion: null,
        },
      ]),
      create: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    },
    hcmDepartmentApi: {
      list: vi.fn().mockResolvedValue([
        {
          id: '1',
          recId: 1,
          code: 'DEP1',
          name: 'Human Resources',
          nameAlias: 'الموارد البشرية',
          description: 'HR Department',
          isActive: true,
          dataAreaId: 'dat',
          recVersion: 1,
          rowVersion: null,
        },
      ]),
      create: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    },
  };
});

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>
  );
}

describe('HCM Reference Pages', () => {
  it('renders HcmNationalityPage and loads data', async () => {
    renderWithProviders(<HcmNationalityPage />);

    await waitFor(() => {
      expect(hcmNationalityApi.list).toHaveBeenCalled();
      expect(screen.getByText('Saudi')).toBeInTheDocument();
    });
  });

  it('renders HcmOccupationPage and loads data', async () => {
    renderWithProviders(<HcmOccupationPage />);

    await waitFor(() => {
      expect(hcmOccupationApi.list).toHaveBeenCalled();
      expect(screen.getByText('Software Engineer')).toBeInTheDocument();
    });
  });

  it('renders HcmDepartmentPage and loads data', async () => {
    renderWithProviders(<HcmDepartmentPage />);

    await waitFor(() => {
      expect(hcmDepartmentApi.list).toHaveBeenCalled();
      expect(screen.getByText('Human Resources')).toBeInTheDocument();
    });
  });
});
