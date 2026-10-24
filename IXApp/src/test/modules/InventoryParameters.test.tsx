import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, render } from '@testing-library/react';
import type { SetupPageProps } from '@patterns/setup/types';
import { InventParametersPage } from '@modules/finance/inventory/pages/InventParametersPage';
import { inventParametersApi } from '@modules/finance/inventory/api/inventParametersApi';

let renderedProps: SetupPageProps;
vi.mock('@patterns/setup/SetupPage', () => ({
  SetupPage: (props: SetupPageProps) => { renderedProps = props; return null; },
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key }),
}));
vi.mock('@core/company/useCompanyStore', () => ({ useCompanyStore: (selector: (state: { currentCompany: string }) => string) => selector({ currentCompany: 'dat' }) }));
vi.mock('@tanstack/react-query', () => ({
  useQueryClient: () => ({ setQueryData: vi.fn() }),
  useQuery: ({ queryKey }: { queryKey: string[] }) => ({ isLoading: false, error: null,
    data: queryKey.includes('lookups')
      ? { units: [], sites: [{ id: 'JED', name: 'Jeddah' }], warehouses: [{ id: 'MAIN', name: 'Main', siteId: 'JED' }], journalNames: [] }
      : { recId: 1, reserveOnOrdered: false, multiSiteDefaultInventSiteId: 'JED', multiSiteDefaultInventLocationId: 'MAIN' },
  }),
}));
vi.mock('@modules/finance/inventory/api/inventParametersApi', () => ({ inventParametersApi: { save: vi.fn() } }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

it('blocks ordered reservation and a warehouse outside the selected site before save', async () => {
  render(<InventParametersPage />);
  const reserve = renderedProps.sections.flatMap((section) => section.fields).find((field) => field.name === 'reserveOnOrdered');
  expect(reserve?.disabled).toBe(true);
  await expect(renderedProps.onSave?.({ reserveOnOrdered: true, multiSiteDefaultInventSiteId: 'JED', multiSiteDefaultInventLocationId: 'MAIN' }))
    .rejects.toThrow('ordered supply');
  await expect(renderedProps.onSave?.({ reserveOnOrdered: false, multiSiteDefaultInventSiteId: 'RUH', multiSiteDefaultInventLocationId: 'MAIN' }))
    .rejects.toThrow('must belong');
  expect(inventParametersApi.save).not.toHaveBeenCalled();
});
