import React from 'react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import { act, render, screen } from '@test/testUtils';
import i18n from '@core/localization/i18n';
import { queryClient } from '@core/api/queryClient';
import { OrganizationRolePage } from '@modules/organization/pages/OrganizationRolePage';
import { organizationStructureApi } from '@modules/organization/api/organizationStructureApi';

afterEach(async () => {
  cleanup();
  queryClient.clear();
  vi.restoreAllMocks();
  await i18n.changeLanguage('en');
});

describe('Organization roles localized names', () => {
  it('renders aliases in RTL, original names in LTR, and falls back for missing aliases', async () => {
    vi.spyOn(organizationStructureApi, 'roles').mockResolvedValue([
      { id: 1, code: 'AREA_MANAGER', name: 'Area Manager', nameAlias: 'مدير المنطقة' },
      { id: 2, code: 'CUSTOM', name: 'Custom role', nameAlias: null },
    ]);
    await i18n.changeLanguage('ar');
    render(<OrganizationRolePage />);
    expect((await screen.findAllByText('مدير المنطقة')).length).toBeGreaterThanOrEqual(1);
    expect(screen.getByText('Custom role')).toBeInTheDocument();
    expect(screen.queryByText('Area Manager')).not.toBeInTheDocument();
    await act(() => i18n.changeLanguage('en'));
    expect(await screen.findByText('Area Manager')).toBeInTheDocument();
    expect(screen.getByText('مدير المنطقة')).toBeInTheDocument();
  });
});
