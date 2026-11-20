import React from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { queryClient } from '@core/api/queryClient';
import { CustomerPostingAccountsPanel } from '@modules/finance/accounts-receivable/setup/components/CustomerPostingAccountsPanel';
import { CustomerPostingProfilesPage } from '@modules/finance/accounts-receivable/setup/pages/CustomerPostingProfilesPage';
import {
  customerPostingAccountApi as api,
  customerPostingProfileApi as profiles,
  newPostingAccount,
  newPostingProfile,
} from '@modules/finance/accounts-receivable/setup/api/customerPostingProfileApi';

vi.mock('@core/permissions/usePermission', () => ({
  usePermission: () => ({ hasPermission: true }),
}));
vi.mock(
  '@modules/finance/accounts-receivable/setup/api/customerPostingProfileApi',
  async (original) => ({
    ...(await original<
      typeof import('@modules/finance/accounts-receivable/setup/api/customerPostingProfileApi')
    >()),
    customerPostingAccountApi: {
      forProfile: vi.fn(),
      ledgerAccounts: vi.fn(),
      references: vi.fn(),
      update: vi.fn(),
      create: vi.fn(),
      delete: vi.fn(),
    },
    customerPostingProfileApi: { list: vi.fn(), update: vi.fn(), create: vi.fn(), delete: vi.fn() },
  })
);
const profile = {
  ...newPostingProfile('dat'),
  id: '1',
  recId: 1,
  postingProfile: 'GEN',
  name: 'General',
};
const rule = {
  ...newPostingAccount('GEN', 'dat'),
  id: '10',
  recId: 10,
  accountCode: 1,
  num: 'LOCAL',
  summaryLedgerDimension: 100,
};
beforeEach(() => {
  queryClient.clear();
  vi.clearAllMocks();
  vi.mocked(api.forProfile).mockResolvedValue([rule]);
  vi.mocked(api.references).mockResolvedValue([
    { id: 'LOCAL', code: 'LOCAL', name: 'Local customers' },
  ]);
  vi.mocked(api.ledgerAccounts).mockResolvedValue([
    { id: 100, code: '11020302', name: 'Customer balances' },
    { id: 200, code: '11020303', name: 'Customer advances' },
  ]);
  vi.mocked(api.update).mockImplementation(async (value) => value);
  vi.mocked(profiles.list).mockResolvedValue([profile]);
  vi.mocked(profiles.update).mockImplementation(async (value) => value);
});

it.each(['pencil', 'toolbar'])(
  'coordinates %s editing with action-pane Save on the real posting-profile page',
  async (entry) => {
    render(
      <MemoryRouter>
        <AppProviders>
          <CustomerPostingProfilesPage />
        </AppProviders>
      </MemoryRouter>
    );
    await screen.findByText('11020302');
    fireEvent.click(
      screen.getByRole('button', { name: entry === 'pencil' ? 'Edit Summary account' : /^Edit$/ })
    );
    const lookup = await screen.findByRole('combobox', { name: 'Summary account' });
    expect(lookup).toBeEnabled();
    expect(screen.getByRole('button', { name: /^Save$/ })).toBeEnabled();
    expect(screen.getByRole('button', { name: /^Cancel$/ })).toBeEnabled();
    fireEvent.mouseDown(lookup);
    fireEvent.click(await screen.findByRole('option', { name: /Customer advances/ }));
    fireEvent.click(screen.getByRole('button', { name: /^Save$/ }));
    await waitFor(() =>
      expect(api.update).toHaveBeenCalledWith(
        expect.objectContaining({ summaryLedgerDimension: 200 })
      )
    );
    await waitFor(() => expect(profiles.update).toHaveBeenCalledTimes(1));
    expect(await screen.findByText('11020303')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /^Save$/ })).not.toBeInTheDocument();
  }
);
function open(masterEditing = false) {
  return render(
    <MemoryRouter>
      <AppProviders>
        <CustomerPostingAccountsPanel
          profile={profile}
          masterEditing={masterEditing}
          onEditingChange={vi.fn()}
        />
      </AppProviders>
    </MemoryRouter>
  );
}
it('shows account codes and saves the selected ledger record ID', async () => {
  open();
  expect(await screen.findByText('11020302')).toBeInTheDocument();
  fireEvent.click(screen.getByRole('button', { name: 'Edit account' }));
  const lookup = await screen.findByRole('combobox', { name: 'Summary account' });
  fireEvent.mouseDown(lookup);
  fireEvent.click(await screen.findByRole('option', { name: /Customer advances/ }));
  expect(api.update).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Save account' }));
  await waitFor(() =>
    expect(api.update).toHaveBeenCalledWith(
      expect.objectContaining({ summaryLedgerDimension: 200, num: 'LOCAL' })
    )
  );
  expect(await screen.findByText('11020303')).toBeInTheDocument();
});
it('preserves the draft after a failed save and lets Cancel restore the saved rule', async () => {
  vi.mocked(api.update).mockRejectedValue(new Error('A rule already exists.'));
  open();
  await screen.findByText('11020302');
  fireEvent.click(screen.getByRole('button', { name: 'Edit account' }));
  fireEvent.click(screen.getByRole('button', { name: 'Save account' }));
  expect(await screen.findByText('A rule already exists.')).toBeInTheDocument();
  expect(screen.getByRole('combobox', { name: 'Summary account' })).toBeInTheDocument();
  fireEvent.click(screen.getByRole('button', { name: 'Cancel account changes' }));
  expect(await screen.findByText('11020302')).toBeInTheDocument();
  expect(screen.queryByText('A rule already exists.')).not.toBeInTheDocument();
});
it('enables the selected rule fields when Edit is requested from the page', async () => {
  open(true);
  expect(await screen.findByRole('combobox', { name: 'Summary account' })).toBeEnabled();
});

it('clears the customer/group reference for All and prevents repeated submissions', async () => {
  let finish!: (value: typeof rule) => void;
  vi.mocked(api.update).mockReturnValue(
    new Promise((resolve) => {
      finish = resolve;
    })
  );
  open();
  await screen.findByText('11020302');
  fireEvent.click(screen.getByRole('button', { name: 'Edit account' }));
  fireEvent.mouseDown(screen.getByRole('combobox', { name: 'Account code' }));
  fireEvent.click(await screen.findByRole('option', { name: 'All' }));
  expect(screen.queryByRole('combobox', { name: /Account\/Group number/ })).not.toBeInTheDocument();
  const save = screen.getByRole('button', { name: 'Save account' });
  fireEvent.click(save);
  fireEvent.click(save);
  await waitFor(() => expect(api.update).toHaveBeenCalledTimes(1));
  expect(api.update).toHaveBeenCalledWith(expect.objectContaining({ accountCode: 2, num: '' }));
  finish({ ...rule, accountCode: 2, num: '' });
  await waitFor(() => expect(screen.getByRole('button', { name: 'Edit account' })).toBeEnabled());
});
