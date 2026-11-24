import React from 'react';
import { beforeEach, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { CustParametersPage } from '@modules/finance/accounts-receivable/setup/pages/CustParametersPage';
import { customerParametersApi } from '@modules/finance/accounts-receivable/setup/api/customerParametersApi';
vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => false }) }));
vi.mock(
  '@modules/finance/accounts-receivable/setup/api/customerParametersApi',
  async (importOriginal) => ({
    ...(await importOriginal<
      typeof import('@modules/finance/accounts-receivable/setup/api/customerParametersApi')
    >()),
    customerParametersApi: { get: vi.fn(), save: vi.fn() },
  })
);
beforeEach(() => {
  queryClient.clear();
  vi.clearAllMocks();
});
it('allows viewing persisted settings without granting edit permission', async () => {
  vi.mocked(customerParametersApi.get).mockResolvedValue({
    recId: 4,
    key: 0,
    dataAreaId: 'USMF',
    rowVersion: 'AQ==',
    recVersion: 1,
    paymTermId: 'NET30',
  });
  render(<CustParametersPage />);
  expect(await screen.findByRole('textbox', { name: 'Terms of payment' })).toHaveValue('NET30');
  expect(screen.getByRole('textbox', { name: 'Terms of payment' })).toBeDisabled();
  expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
  expect(screen.getByRole('alert')).toHaveTextContent('do not have permission');
  expect(customerParametersApi.save).not.toHaveBeenCalled();
});
