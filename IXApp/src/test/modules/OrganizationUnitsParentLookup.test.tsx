import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import { render } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { OrganizationUnitsPage } from '@modules/organization/pages/OrganizationUnitsPage';
import { organizationStructureApi as api } from '@modules/organization/api/organizationStructureApi';

const capture = vi.hoisted(() => vi.fn());
vi.mock('@patterns/list-details-tree', () => ({ ListDetailsTreePage: (props: unknown) => { capture(props); return null; } }));
afterEach(() => { cleanup(); queryClient.clear(); vi.restoreAllMocks(); capture.mockClear(); });

it('uses cached units for parent options without a fetch and excludes self and descendants', async () => {
  const date = new Date();
  const asOf = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  const key = ['list-details', `organization-units-${useCompanyStore.getState().currentCompany}-${asOf}`];
  const make = (id: number, parent: number | null) => ({ id: String(id), recordId: id, code: `UNIT-${id}`, name: `Unit ${id}`, nameAlias: `وحدة ${id}`, type: 1, parentOrganizationUnitId: parent, validFrom: asOf, validTo: null });
  const records = [make(1, null), make(2, 1), make(3, 2), make(4, null)];
  queryClient.setQueryDefaults(key, { staleTime: Infinity });
  queryClient.setQueryData(key, records);
  const load = vi.spyOn(api, 'units');
  render(<OrganizationUnitsPage />);
  const config = capture.mock.calls.at(-1)![0].config;
  const sections = config.sections({ record: records[1] });
  const field = sections.flatMap((section: { groups: { fields: { name: string }[] }[] }) => section.groups.flatMap((group) => group.fields)).find((item: { name: string }) => item.name === 'parentOrganizationUnitId');
  const lookup = field.render({ value: 1, disabled: false, onChange: vi.fn() });
  const page = await lookup.props.fetchPage({ pageNumber: 1, pageSize: 20, search: '' });
  expect(page.data.map((option: { id: number }) => option.id)).toEqual([1, 4]);
  expect(load).not.toHaveBeenCalled();
  const searched = await lookup.props.fetchPage({ pageNumber: 1, pageSize: 20, search: 'وحدة 4' });
  expect(searched.data.map((option: { id: number }) => option.id)).toEqual([4]);
});
