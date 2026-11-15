import { expect, it, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import { PartyPostalAddressPanel } from '@shared/components/logistics/PartyLogisticsPanels';
vi.mock('@core/api/apiClient', () => ({
  apiClient: {
    get: vi
      .fn()
      .mockResolvedValue({
        data: {
          success: true,
          data: [
            { id: '1', location: 1, description: 'Office', address: 'Main Street', roles: [] },
          ],
        },
      }),
  },
}));
vi.mock('@shared/components/data-grid/DataGrid', () => ({
  DataGrid: ({
    rows,
    onSelectionChange,
  }: {
    rows: { id: string }[];
    onSelectionChange: (ids: string[]) => void;
  }) => (
    <button disabled={!rows.length} onClick={() => onSelectionChange([rows[0].id])}>
      Select address
    </button>
  ),
}));
vi.mock('@shared/hooks/useLogisticsAddress', () => ({
  useCountryRegions: () => ({ data: [] }),
  useStates: () => ({ data: [] }),
  useCities: () => ({ data: [] }),
  useCounties: () => ({ data: [] }),
}));
it('opens the selected saved address map in view mode while keeping mutations disabled', async () => {
  const open = vi.spyOn(window, 'open').mockReturnValue(null);
  render(
    <PartyPostalAddressPanel partyId={1} editing={false} storageKey="test.customer.readonly-map" />,
  );
  await waitFor(() => expect(screen.getByRole('button', { name: 'Select address' })).toBeEnabled());
  fireEvent.click(screen.getByRole('button', { name: 'Select address' }));
  fireEvent.click(screen.getByRole('button', { name: 'Map' }));
  expect(open).toHaveBeenCalledWith(
    'https://www.google.com/maps/search/?api=1&query=Main%20Street',
    '_blank',
    'noopener,noreferrer',
  );
  for (const name of ['Add', 'Remove', 'Edit'])
    expect(screen.getByRole('button', { name })).toBeDisabled();
  open.mockRestore();
});
