import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import { OpenBalanceCurrencyPage } from '@patterns/open-balance-currency/OpenBalanceCurrencyPage';
import type { PartyOpenBalanceCurrency } from '@patterns/open-balance-currency/types';
import { buildCustomerRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerRibbon';
import { partyBalanceApi } from '@modules/finance/party-balances/api/partyBalanceApi';
import { CustOpenBalanceCurrency } from '@modules/finance/party-balances/pages/PartyBalancePage';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render as plainRender } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import i18n from '@core/localization/i18n';

const permissions = vi.hoisted(() => ({ related: false, primary: true }));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({
    hasPermission: (permission: string) =>
      permission.startsWith('AccountsPayable.') ? permissions.related : permissions.primary,
  }),
}));
vi.mock('@modules/finance/party-balances/api/partyBalanceApi', () => ({
  partyBalanceApi: { load: vi.fn() },
}));

const balance: PartyOpenBalanceCurrency = {
  recId: 1,
  accountNum: 'C-001',
  name: 'Example customer',
  nameAlias: 'عميل الاختبار',
  currencyCode: 'SAR',
  accountingCurrency: 'SAR',
  reportingCurrency: 'EUR',
  balanceMst: 775,
  balanceReporting: 190,
  creditMax: 1000,
  reconciled: true,
  relatedAccounts: ['V-001'],
  currencies: [
    { currencyCode: 'USD', amountCur: 100, amountMst: 375, amountReporting: 90 },
    { currencyCode: 'EUR', amountCur: 100, amountMst: 400, amountReporting: 100 },
  ],
};

beforeEach(async () => {
  permissions.primary = true;
  permissions.related = false;
  vi.mocked(partyBalanceApi.load).mockReset();
  await i18n.changeLanguage('en');
});

describe('confirmed basic customer actions', () => {
  it('contains only implemented basic destinations and disables unsaved-record actions', () => {
    const groups = buildCustomerRibbonGroups((_, fallback) => fallback, {
      saved: false,
      hasParty: false,
      canReadBalances: true,
      onContacts: vi.fn(),
      onBalance: vi.fn(),
      onAttachments: vi.fn(),
    });
    expect(groups.flatMap((group) => group.actions.map((action) => action.id))).toEqual([
      'contacts',
      'balance',
      'attachments',
    ]);
    expect(groups.flatMap((group) => group.actions).every((action) => action.disabled)).toBe(true);
  });
  it('hides balance without transaction permission and calls the selected-record callback', () => {
    const onAttachments = vi.fn();
    const groups = buildCustomerRibbonGroups((_, fallback) => fallback, {
      saved: true,
      hasParty: true,
      canReadBalances: false,
      onContacts: vi.fn(),
      onBalance: vi.fn(),
      onAttachments,
    });
    expect(groups.some((group) => group.id === 'balance')).toBe(false);
    groups.find((group) => group.id === 'attachments')!.actions[0].onClick!();
    expect(onAttachments).toHaveBeenCalledOnce();
  });
});

describe('shared customer/vendor balance layout', () => {
  it('selects the first currency row and opens the implemented inquiry', () => {
    const open = vi.fn();
    render(
      <OpenBalanceCurrencyPage
        kind="customer"
        balance={balance}
        relatedBalance={0}
        onBack={vi.fn()}
        onRefresh={vi.fn()}
        onOpenTransactions={open}
      />
    );
    expect(document.querySelector('[data-row-id="currency-USD"]')).toHaveAttribute(
      'aria-selected',
      'true'
    );
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'New' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeDisabled();
    fireEvent.click(screen.getByRole('button', { name: 'Customer' }));
    fireEvent.click(screen.getByRole('button', { name: 'Open transactions' }));
    expect(open).toHaveBeenCalledOnce();
  });
  it('shows separate currency rows, accounting totals and the basic posted-balance credit remainder', () => {
    const refresh = vi.fn();
    render(
      <OpenBalanceCurrencyPage
        kind="customer"
        balance={balance}
        relatedBalance={null}
        relatedUnavailable
        onBack={vi.fn()}
        onRefresh={refresh}
      />
    );
    expect(screen.getByRole('heading', { name: 'Customer balance' })).toBeInTheDocument();
    expect(screen.getByText('USD', { exact: true })).toBeInTheDocument();
    expect(screen.getByText('EUR', { exact: true })).toBeInTheDocument();
    expect(screen.getByText('225.00', { exact: true })).toBeInTheDocument();
    expect(
      screen.getByText(
        'The linked account balance is unavailable or you do not have permission to view it.'
      )
    ).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Refresh' }));
    expect(refresh).toHaveBeenCalledOnce();
  });
  it('reuses the design for vendor balances without inventing a reporting amount or credit-limit form', () => {
    render(
      <OpenBalanceCurrencyPage
        kind="vendor"
        balance={{
          ...balance,
          accountNum: 'V-001',
          balanceMst: -250,
          balanceReporting: null,
          currencies: [
            { currencyCode: 'SAR', amountCur: -250, amountMst: -250, amountReporting: null },
          ],
        }}
        relatedBalance={0}
        onBack={vi.fn()}
        onRefresh={vi.fn()}
      />
    );
    expect(screen.getByRole('heading', { name: 'Vendor balance' })).toBeInTheDocument();
    expect(screen.queryByText('Credit remaining')).toBeNull();
    expect(screen.getAllByText('-250.00').length).toBeGreaterThan(0);
  });
  it('preserves Arabic names when the balance is displayed in RTL', async () => {
    await i18n.changeLanguage('ar');
    render(
      <OpenBalanceCurrencyPage
        kind="customer"
        balance={balance}
        relatedBalance={0}
        onBack={vi.fn()}
        onRefresh={vi.fn()}
      />
    );
    expect(screen.getByLabelText('الاسم')).toHaveTextContent('عميل الاختبار');
    expect(screen.getByRole('heading', { name: 'رصيد العميل' })).toBeInTheDocument();
  });
  it('does not request linked vendor transactions without their permission', async () => {
    vi.mocked(partyBalanceApi.load).mockResolvedValue(balance);
    plainRender(
      <MemoryRouter initialEntries={['/balance/C-001']}>
        <AppProviders>
          <Routes>
            <Route path="/balance/:accountNum" element={<CustOpenBalanceCurrency />} />
          </Routes>
        </AppProviders>
      </MemoryRouter>
    );
    await screen.findByRole('heading', { name: 'Customer balance' });
    await waitFor(() => expect(partyBalanceApi.load).toHaveBeenCalled());
    expect(vi.mocked(partyBalanceApi.load).mock.calls.every(([kind]) => kind === 'customer')).toBe(
      true
    );
  });
});
