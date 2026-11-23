import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';

interface AccountRecord {
  id: string;
  code: string;
  name: string;
  suspended: boolean;
}

const records: AccountRecord[] = [
  { id: '1', code: '10000000', name: 'Assets', suspended: false },
  { id: '2', code: '11000000', name: 'Current Assets', suspended: true },
  { id: '3', code: '11010000', name: 'Cash & Bank Balances', suspended: false },
];

const baseConfig = (
  overrides: Partial<EnterpriseListDetailsConfig<AccountRecord>> = {}
): EnterpriseListDetailsConfig<AccountRecord> => ({
  filterStorageKey: 'test.accounts',
  dataSource: { type: 'static', records },
  createRecord: () => ({ id: '', code: '', name: '', suspended: false }),
  getPrimaryText: (record) => `${record.code}, ${record.name}`,
  getValues: (record) => ({ name: record.name, suspended: record.suspended }),
  setValues: (record, values) => ({
    ...record,
    name: String(values.name ?? ''),
    suspended: Boolean(values.suspended),
  }),
  headerFields: [],
  sections: [],
  presentation: { mode: 'list' },
  ...overrides,
});

const renderPage = (config: EnterpriseListDetailsConfig<AccountRecord>) =>
  render(<ListDetailsPage variant="enterprise" title="Main accounts" config={config} />);

describe('list details form surface', () => {
  it('renders a section command strip and runs the command', async () => {
    const onClick = vi.fn();
    renderPage(
      baseConfig({
        sections: [
          {
            id: 'general',
            title: 'General',
            defaultExpanded: true,
            commands: [{ id: 'totals', label: 'Totals', onClick }],
            groups: [{ id: 'identification', fields: [{ name: 'name', label: 'Name' }] }],
          },
        ],
      })
    );

    const totals = await screen.findByRole('button', { name: 'Totals' });
    fireEvent.click(totals);
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('disables a section command that has no handler, and says why', async () => {
    renderPage(
      baseConfig({
        sections: [
          {
            id: 'general',
            title: 'General',
            defaultExpanded: true,
            commands: [{ id: 'currency-code-validation', label: 'Currency code validation' }],
            groups: [{ id: 'identification', fields: [{ name: 'name', label: 'Name' }] }],
          },
        ],
      })
    );

    const command = await screen.findByRole('button', { name: 'Currency code validation' });
    expect(command).toBeDisabled();
    // The hover text sits on the wrapper, because a disabled button fires no pointer events.
    expect(command.parentElement).toHaveAttribute('title');
  });

  it('opens the menu of a command that has items', async () => {
    const onClick = vi.fn();
    renderPage(
      baseConfig({
        sections: [
          {
            id: 'general',
            title: 'General',
            defaultExpanded: true,
            commands: [
              {
                id: 'assign-template',
                label: 'Assign template',
                items: [{ id: 'assign', label: 'Assign', onClick }],
              },
            ],
            groups: [{ id: 'identification', fields: [{ name: 'name', label: 'Name' }] }],
          },
        ],
      })
    );

    fireEvent.click(await screen.findByRole('button', { name: /assign template/i }));
    fireEvent.click(await screen.findByRole('menuitem', { name: 'Assign' }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('steps the selection with the list pager and stops at both ends', async () => {
    const onSelectionChange = vi.fn();
    renderPage(baseConfig({ listPager: {}, onSelectionChange }));
    fireEvent.click(await screen.findByRole('button', { name: 'Toggle record list' }));

    // The list pane is laid out responsively, so jsdom computes it as hidden; the
    // pager is queried accordingly rather than being skipped by the role query.
    const pager = (name: RegExp) => screen.getByRole('button', { name, hidden: true });
    await waitFor(() => expect(pager(/previous/i)).toBeInTheDocument());

    // The first record is selected, so there is nothing before it.
    expect(pager(/previous/i)).toBeDisabled();
    expect(pager(/^next$/i)).toBeEnabled();

    fireEvent.click(pager(/^next$/i));
    await waitFor(() =>
      expect(onSelectionChange).toHaveBeenCalledWith(expect.objectContaining({ id: '2' }))
    );
    await waitFor(() => expect(pager(/previous/i)).toBeEnabled());

    fireEvent.click(pager(/^next$/i));
    await waitFor(() =>
      expect(onSelectionChange).toHaveBeenCalledWith(expect.objectContaining({ id: '3' }))
    );
    // The last record is selected, so there is nothing after it.
    await waitFor(() => expect(pager(/^next$/i)).toBeDisabled());
  });

  it('leaves the pager out when the page does not ask for one', async () => {
    const onSelectionChange = vi.fn();
    renderPage(baseConfig({ onSelectionChange }));
    await waitFor(() => expect(onSelectionChange).toHaveBeenCalled());
    expect(
      screen.queryByRole('button', { name: /previous/i, hidden: true })
    ).not.toBeInTheDocument();
  });
});
