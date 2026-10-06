import React from 'react';
import { expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ListDetailsLayout } from '@patterns/list-details/ListDetailsLayout';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import { FieldViewModeProvider } from '@shared/components/fields/FieldViewModeContext';
import { LookupField } from '@shared/components/lookups/LookupField';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

it('opens the existing editor from a lookup pencil', () => {
  const edit = vi.fn();
  render(
    <MemoryRouter>
      <ListDetailsLayout
        listPane={null}
        listPaneVisible={false}
        header={null}
        sections={[{
          id: 'general', title: 'General', groups: [{
            id: 'payment', fields: [{
              name: 'paymTermId', label: 'Terms of payment', type: 'select',
              options: [{ value: '90', label: '90 Days' }],
            }],
          }],
        }]}
        values={{ paymTermId: '90' }}
        editing={false}
        yesLabel="Yes"
        noLabel="No"
        onChange={vi.fn()}
        onFieldEdit={edit}
      />
    </MemoryRouter>
  );

  const pencil = screen.getByRole('button', { name: 'Edit Terms of payment' });
  expect(pencil).toHaveClass('field-edit-action');
  fireEvent.click(pencil);
  expect(edit).toHaveBeenCalledWith('paymTermId');
});

it('offers the same edit action for empty lookups and editable text, number, and boolean fields', () => {
  const edit = vi.fn();
  render(
    <MemoryRouter>
      <ListDetailsLayout
        listPane={null}
        listPaneVisible={false}
        header={null}
        sections={[{
          id: 'general', title: 'General', groups: [{ id: 'fields', fields: [
            { name: 'term', label: 'Payment term', type: 'select', options: [] },
            { name: 'name', label: 'Name' },
            { name: 'amount', label: 'Amount', type: 'number' },
            { name: 'active', label: 'Active', type: 'boolean' },
            { name: 'readOnly', label: 'Read only', type: 'display' },
          ] }],
        }]}
        values={{ term: '', name: '', amount: 0, active: false, readOnly: '' }}
        editing={false}
        yesLabel="Yes"
        noLabel="No"
        onChange={vi.fn()}
        onFieldEdit={edit}
      />
    </MemoryRouter>
  );

  for (const [label, name] of [
    ['Payment term', 'term'], ['Name', 'name'], ['Amount', 'amount'], ['Active', 'active'],
  ]) {
    fireEvent.click(screen.getByRole('button', { name: `Edit ${label}` }));
    expect(edit).toHaveBeenLastCalledWith(name);
  }
  expect(screen.queryByRole('button', { name: 'Edit Read only' })).toBeNull();
  expect(screen.queryByRole('switch')).toBeNull();
  expect(screen.queryByRole('textbox')).toBeNull();
});

it('shows list-grid values as display fields until edit mode starts', () => {
  const { rerender } = render(<ListGridField label="Customer group" value="10" options={[{ value: '10', label: 'Retail' }]} />);
  expect(screen.getByText('Retail')).toBeInTheDocument();
  expect(screen.queryByRole('textbox')).toBeNull();
  expect(screen.queryByRole('combobox')).toBeNull();

  rerender(<ListGridField label="Customer group" value="10" options={[{ value: '10', label: 'Retail' }]} editing />);
  expect(screen.getByRole('combobox')).toBeInTheDocument();
});

it('displays a custom lookup as text in view mode and mounts its editor only in edit mode', () => {
  const renderLookup = vi.fn(() => <input aria-label="Lookup editor" />);
  const props = {
    listPane: null,
    listPaneVisible: false,
    header: null,
    sections: [{ id: 'general', title: 'General', groups: [{ id: 'classification', fields: [{
      name: 'priorityId', label: 'Priority', type: 'select' as const, renderOwnLabel: true,
      formatValue: () => 'High', render: renderLookup,
    }] }] }],
    values: { priorityId: 1 },
    yesLabel: 'Yes', noLabel: 'No', onChange: vi.fn(),
  };
  const { rerender } = render(<ListDetailsLayout {...props} editing={false} />);
  expect(screen.getByText('High')).toBeInTheDocument();
  expect(screen.queryByRole('textbox')).toBeNull();
  expect(renderLookup).not.toHaveBeenCalled();

  rerender(<ListDetailsLayout {...props} editing />);
  expect(screen.getByRole('textbox', { name: 'Lookup editor' })).toBeInTheDocument();
});

it('renders shared lookup values without dropdown controls in view mode', () => {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const { rerender } = render(
    <QueryClientProvider client={client}>
      <FieldViewModeProvider viewMode>
        <LookupField name="priorityId" label="Priority" value="high" options={[{ id: 'high', code: 'HIGH', name: 'High' }]} />
      </FieldViewModeProvider>
    </QueryClientProvider>
  );
  expect(screen.getByText('High')).toBeInTheDocument();
  expect(screen.queryByRole('combobox')).toBeNull();
  expect(screen.queryByRole('button', { name: 'Open' })).toBeNull();

  rerender(
    <QueryClientProvider client={client}>
      <FieldViewModeProvider viewMode>
        <LookupField name="priorityId" label="Priority" value="" options={[]} />
      </FieldViewModeProvider>
    </QueryClientProvider>
  );
  expect(screen.queryByText('Select...')).toBeNull();
});
