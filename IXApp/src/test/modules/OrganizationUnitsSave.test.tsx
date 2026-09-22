import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, screen, waitFor, within } from '@testing-library/react';
import { render } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { OrganizationUnitsPage } from '@modules/organization/pages/OrganizationUnitsPage';
import { organizationStructureApi as api } from '@modules/organization/api/organizationStructureApi';

afterEach(() => { cleanup(); queryClient.clear(); vi.restoreAllMocks(); });
it('sends the chosen parent when saving an existing unit', async () => {
  const records = [
    { id: 1, code: 'CHILD', name: 'Child unit', type: 1, parentOrganizationUnitId: null, validFrom: '2026-01-01', validTo: null },
    { id: 2, code: 'PARENT', name: 'Parent unit', type: 1, parentOrganizationUnitId: null, validFrom: '2026-01-01', validTo: null },
  ];
  vi.spyOn(api, 'units').mockResolvedValue(records);
  const update = vi.spyOn(api, 'update').mockResolvedValue(1);
  render(<OrganizationUnitsPage />);
  await screen.findAllByText('Child unit');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  fireEvent.click(screen.getByRole('button', { name: 'Open' }));
  const list = await screen.findByRole('listbox');
  fireEvent.click(within(list).getByText('Parent unit'));
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() => expect(update).toHaveBeenCalledWith(1, expect.objectContaining({ parentOrganizationUnitId: 2 })));
});
