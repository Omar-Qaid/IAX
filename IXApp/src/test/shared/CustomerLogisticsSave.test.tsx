import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import { LogisticsElectronicAddressDrawer } from '@shared/components/logistics/LogisticsElectronicAddressDrawer';
import { LogisticsPostalAddressDrawer } from '@shared/components/logistics/LogisticsPostalAddressDrawer';
vi.mock('@shared/hooks/useLogisticsAddress', () => ({
  useCountryRegions: () => ({ data: [{ countryRegionId: 'SAU', name: 'Saudi Arabia' }] }),
  useStates: () => ({ data: [] }),
  useCities: () => ({ data: [] }),
  useCounties: () => ({ data: [] }),
}));
const contact = { description: 'Office', type: 'Phone' as const, number: '0500000000' };
const address = {
  description: 'Office',
  countryRegionId: 'SAU',
  validFrom: '2026-01-01',
  validTo: '2154-12-31',
};
describe.each(['contact', 'address'] as const)('%s save workflow', (kind) => {
  it('keeps the editor and draft open after a rejected save, then allows retry', async () => {
    const close = vi.fn();
    const save = vi
      .fn()
      .mockRejectedValueOnce(new Error('Save refused by server'))
      .mockResolvedValueOnce(undefined);
    render(
      kind === 'contact' ? (
        <LogisticsElectronicAddressDrawer
          open
          onClose={close}
          onSave={save}
          initialData={contact}
        />
      ) : (
        <LogisticsPostalAddressDrawer open onClose={close} onSave={save} initialData={address} />
      )
    );
    fireEvent.click(screen.getByRole('button', { name: 'OK' }));
    await screen.findByText('Save refused by server');
    expect(close).not.toHaveBeenCalled();
    expect(screen.getByDisplayValue('Office')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'OK' }));
    await waitFor(() => expect(close).toHaveBeenCalledOnce());
    expect(save).toHaveBeenCalledTimes(2);
  });
  it('waits for persistence and prevents duplicate submissions', async () => {
    let finish: () => void = () => undefined;
    const close = vi.fn();
    const save = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          finish = resolve;
        })
    );
    render(
      kind === 'contact' ? (
        <LogisticsElectronicAddressDrawer
          open
          onClose={close}
          onSave={save}
          initialData={contact}
        />
      ) : (
        <LogisticsPostalAddressDrawer open onClose={close} onSave={save} initialData={address} />
      )
    );
    const ok = screen.getByRole('button', { name: 'OK' });
    fireEvent.click(ok);
    expect(ok).toBeDisabled();
    fireEvent.click(ok);
    expect(save).toHaveBeenCalledOnce();
    expect(close).not.toHaveBeenCalled();
    finish();
    await waitFor(() => expect(close).toHaveBeenCalledOnce());
  });
});
