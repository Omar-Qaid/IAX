import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, render } from '@testing-library/react';
import type { SimpleListPageProps } from '@patterns/simple-list/SimpleListPage';
import { InventOnHandPage } from '@modules/finance/inventory/pages/InventOnHandPage';
import type { InventOnHandRecord } from '@modules/finance/inventory/api/inventOnHandApi';

let renderedProps: SimpleListPageProps<InventOnHandRecord>;
vi.mock('@patterns/simple-list/SimpleListPage', () => ({
  SimpleListPage: (props: typeof renderedProps) => { renderedProps = props; return null; },
}));
vi.mock('@tanstack/react-query', () => ({
  useQuery: () => ({ data: { rows: [], totalCount: 0 }, isLoading: false, isFetching: false, error: null, refetch: vi.fn() }),
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key, currentLanguage: { code: 'en' } }),
}));
afterEach(() => cleanup());

it('shows inventory status and blocking alongside available quantities', () => {
  render(<InventOnHandPage />);
  const fields = renderedProps.columns.map((column) => column.field);
  expect(fields).toContain('inventoryStatusId');
  expect(fields).toContain('inventoryStatusBlocked');
  expect(fields).toContain('availablePhysical');
});
