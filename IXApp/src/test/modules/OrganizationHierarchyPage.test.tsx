import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, screen } from '@testing-library/react';
import { queryClient } from '@core/api/queryClient';
import { render } from '@test/testUtils';
import { OrganizationHierarchyPage } from '@modules/organization/pages/OrganizationHierarchyPage';
import { organizationStructureApi as api } from '@modules/organization/api/organizationStructureApi';

const capture = vi.hoisted(() => vi.fn());
vi.mock('@patterns/simple-list/SimpleListPage', () => ({
  SimpleListPage: (props: unknown) => {
    capture(props);
    return null;
  },
}));
afterEach(() => {
  cleanup();
  vi.restoreAllMocks();
  capture.mockClear();
  queryClient.clear();
});

it('loads organization units only when the searchable lookup opens and saves the selected ID', async () => {
  const units = vi.spyOn(api, 'units').mockResolvedValue([
    { id: 5, code: 'FIN', name: 'Finance', nameAlias: 'المالية', type: 1, parentOrganizationUnitId: null, validFrom: '2026-01-01', validTo: null },
    { id: 6, code: 'OPS', name: 'Operations', type: 1, parentOrganizationUnitId: null, validFrom: '2026-01-01', validTo: null },
  ]);
  render(<OrganizationHierarchyPage />);
  act(() => { capture.mock.calls.at(-1)![0].dataGridProps.onNewRow(); });
  const column = capture.mock.calls.at(-1)![0].columns.find((item: { field: string }) => item.field === 'rootOrganizationUnitId');
  const onChange = vi.fn();
  render(column.renderEditCell({ value: 0, onChange, disabled: false }));
  expect(units).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Open' }));
  expect(await screen.findByText('Finance')).toBeInTheDocument();
  fireEvent.change(screen.getByRole('combobox'), { target: { value: 'Operations' } });
  fireEvent.click(await screen.findByText('Operations'));
  expect(onChange).toHaveBeenCalledWith(6);
});

it('creates a hierarchy with its initial root membership and keeps both names', async () => {
  vi.spyOn(api, 'units').mockResolvedValue([]);
  const create = vi.spyOn(api, 'createHierarchyWithRootNode').mockResolvedValue(12);
  render(<OrganizationHierarchyPage />);
  const props = capture.mock.calls.at(-1)![0];
  const row = {
    id: 'new',
    recordId: 0,
    code: 'FIN',
    name: 'Financial',
    nameAlias: 'الهيكل المالي',
    purpose: 'Financial',
    rootOrganizationUnitId: 5,
    validFrom: '2026-01-01',
    validTo: null,
  };
  await props.dataGridProps.onRowSave(row, true);
  expect(create).toHaveBeenCalledWith({
    code: 'FIN',
    name: 'Financial',
    nameAlias: 'الهيكل المالي',
    purpose: 'Financial',
    organizationUnitId: 5,
    validFrom: '2026-01-01',
    validTo: null,
  });
  expect(props.columns.map((column: { field: string }) => column.field)).toEqual([
    'code',
    'name',
    'nameAlias',
    'purpose',
  ]);
  expect(props.enterpriseConfig.crud.onDelete).toBeUndefined();
});

it('requires a root when creating and sends only hierarchy fields when updating', async () => {
  vi.spyOn(api, 'units').mockResolvedValue([]);
  const create = vi.spyOn(api, 'createHierarchyWithRootNode');
  const update = vi.spyOn(api, 'updateHierarchy').mockResolvedValue(12);
  render(<OrganizationHierarchyPage />);
  const save = capture.mock.calls.at(-1)![0].dataGridProps.onRowSave;
  const row = {
    id: '12',
    recordId: 12,
    code: 'FIN',
    name: 'Financial',
    nameAlias: 'الهيكل المالي',
    purpose: 'Financial',
    rootOrganizationUnitId: 0,
    validFrom: '',
    validTo: null,
  };
  await expect(save(row, true)).rejects.toThrow();
  expect(create).not.toHaveBeenCalled();
  await save(row, false);
  expect(update).toHaveBeenCalledWith(12, {
    code: 'FIN',
    name: 'Financial',
    nameAlias: 'الهيكل المالي',
    purpose: 'Financial',
  });
});
