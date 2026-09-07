import React from 'react';
import { act, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { render } from '@test/testUtils';
import { WfGenericReportPage } from '@modules/workflow/pages/WfGenericReportPage';
import i18n from '@core/localization/i18n';

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

vi.mock('@modules/workflow/api/wfRequestControlApi', () => ({
  wfRequestControlApi: {
    list: vi.fn().mockResolvedValue([
      {
        id: '101',
        recId: 101,
        processId: 7,
        code: 'SHOWROOM',
        name: 'Showroom',
        nameAlias: 'المعرض',
        sortOrder: 1,
        isActive: true,
        canFilter: true,
        canGroup: true,
        canSort: true,
        referenceType: 'Showroom',
        fieldRole: 'Dimension',
        dataType: 'String',
        defaultAggregation: 'NONE',
      },
      {
        id: '102',
        recId: 102,
        processId: 7,
        code: 'AMOUNT',
        name: 'Amount',
        nameAlias: 'المبلغ',
        sortOrder: 2,
        isActive: true,
        canFilter: true,
        canGroup: false,
        canSort: true,
        referenceType: null,
        fieldRole: 'Measure',
        dataType: 'Decimal',
        defaultAggregation: 'SUM',
      },
    ]),
  },
}));

describe('WfGenericReportPage', () => {
  it('opens parameters, shows processing, and renders a read-only mock report', async () => {
    await act(() => i18n.changeLanguage('en'));
    const user = userEvent.setup();
    render(<WfGenericReportPage />);

    expect(screen.getByRole('dialog', { name: 'Generic workflow report' })).toBeInTheDocument();
    expect(screen.getByText('Report builder')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Open' }));
    expect(await screen.findByText('DAILY')).toBeInTheDocument();
    expect(screen.getByText('Daily Seller Deposit')).toBeInTheDocument();
    await user.click(screen.getByText('Daily Seller Deposit'));
    expect(await screen.findByText('SUM (Amount)')).toBeInTheDocument();
    expect(screen.getByText('AVG (Amount)')).toBeInTheDocument();
    expect(screen.getByText('COUNT (Requests)')).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: 'SUM (Amount)' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'AVG (Amount)' })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'COUNT (Requests)' })).not.toBeChecked();

    await user.click(screen.getByRole('button', { name: 'Run Report' }));
    expect(screen.getByText('Processing operation - Generic workflow report')).toBeInTheDocument();

    await waitFor(
      () =>
        expect(
          screen.queryByText('Processing operation - Generic workflow report')
        ).not.toBeInTheDocument(),
      { timeout: 2_500 }
    );
    expect(screen.getByText('Omar Ali')).toBeInTheDocument();
    expect(screen.getByText('Bank transfer')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'New' })).not.toBeInTheDocument();
  });

  it('uses Arabic labels and RTL direction when Arabic is active', async () => {
    await act(() => i18n.changeLanguage('ar'));

    render(<WfGenericReportPage />);

    const dialog = screen.getByRole('dialog', { name: 'تقرير سير العمل العام' });
    expect(dialog).toHaveAttribute('dir', 'rtl');
    expect(screen.getByText('منشئ التقارير')).toHaveStyle({ textAlign: 'start' });
    expect(screen.getByText('من تاريخ')).toBeInTheDocument();
    expect(screen.getAllByText('العملية').length).toBeGreaterThan(0);

    await act(() => i18n.changeLanguage('en'));
  });
});
