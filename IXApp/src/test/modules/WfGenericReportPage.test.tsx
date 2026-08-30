import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { render } from '@test/testUtils';
import { WfGenericReportPage } from '@modules/workflow/pages/WfGenericReportPage';

vi.mock('@modules/workflow/api/wfProcessApi', () => ({
  wfProcessApi: {
    list: vi
      .fn()
      .mockResolvedValue([
        { id: '7', recId: 7, code: 'DAILY', name: 'Daily Seller Deposit', isActive: true },
      ]),
    getById: vi.fn().mockResolvedValue({
      id: '7',
      recId: 7,
      code: 'DAILY',
      name: 'Daily Seller Deposit',
      isActive: true,
    }),
  },
}));

describe('WfGenericReportPage', () => {
  it('opens parameters, shows processing, and renders a read-only mock report', async () => {
    const user = userEvent.setup();
    render(<WfGenericReportPage />);

    expect(screen.getByRole('dialog', { name: 'WfGenericReport' })).toBeInTheDocument();
    expect(screen.getByText('Report Builder')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Open' }));
    expect(await screen.findByText('DAILY')).toBeInTheDocument();
    expect(screen.getByText('Daily Seller Deposit')).toBeInTheDocument();
    await user.click(screen.getByText('Daily Seller Deposit'));

    await user.click(screen.getByRole('button', { name: 'OK' }));
    expect(screen.getByText('Processing operation - WfGenericReport')).toBeInTheDocument();

    await waitFor(
      () =>
        expect(
          screen.queryByText('Processing operation - WfGenericReport')
        ).not.toBeInTheDocument(),
      { timeout: 2_500 }
    );
    expect(screen.getByText('Omar Ali')).toBeInTheDocument();
    expect(screen.getByText('Bank transfer')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'New' })).not.toBeInTheDocument();
  });
});
