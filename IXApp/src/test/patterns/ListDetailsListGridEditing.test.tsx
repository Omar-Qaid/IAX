import React from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import { useListGridEditor } from '@patterns/list-details-listgrid/useListGridEditor';
import { queryClient } from '@core/api/queryClient';

const saveDetail = vi.fn();
function RuleEditor() {
  const [value, setValue] = React.useState('11020302');
  useListGridEditor({
    saveEdit: async () => {
      await saveDetail(value);
      return true;
    },
    cancelEdit: () => setValue('11020302'),
  });
  return <ListGridField label="Summary account" value={value} onChange={setValue} />;
}
function open() {
  return render(
    <MemoryRouter>
      <AppProviders>
        <ListDetailsListGridPage
          title="Test profiles"
          config={{
            dataSource: { type: 'static', records: [{ id: '1', name: 'General' }] },
            createRecord: () => ({ id: 'new', name: '' }),
            getPrimaryText: (record) => record.name,
            getValues: (record) => ({ name: record.name }),
            setValues: (record, values) => ({ ...record, name: String(values.name) }),
            headerFields: [],
            sections: [{ id: 'setup', title: 'Setup', content: <RuleEditor /> }],
          }}
        />
      </AppProviders>
    </MemoryRouter>
  );
}
beforeEach(() => {
  queryClient.clear();
  vi.clearAllMocks();
  saveDetail.mockResolvedValue(undefined);
});
it('pencil editing displays action-pane Save/Cancel and one Save commits the custom editor', async () => {
  open();
  fireEvent.click(await screen.findByRole('button', { name: 'Edit Summary account' }));
  const input = await screen.findByRole('textbox', { name: 'Summary account' });
  expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  fireEvent.change(input, { target: { value: '11020303' } });
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() => expect(saveDetail).toHaveBeenCalledExactlyOnceWith('11020303'));
  await waitFor(() =>
    expect(screen.queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
  );
});
it('action-pane Edit enables custom fields and Cancel discards their drafts', async () => {
  open();
  fireEvent.click(await screen.findByRole('button', { name: /^Edit$/ }));
  const input = await screen.findByRole('textbox', { name: 'Summary account' });
  fireEvent.change(input, { target: { value: 'unsaved' } });
  fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  expect(await screen.findByText('11020302')).toBeInTheDocument();
  expect(saveDetail).not.toHaveBeenCalled();
});
it('a failed custom-editor save retains edit mode and the entered value', async () => {
  saveDetail.mockRejectedValue(new Error('Invalid ledger account'));
  open();
  fireEvent.click(await screen.findByRole('button', { name: 'Edit Summary account' }));
  fireEvent.change(await screen.findByRole('textbox', { name: 'Summary account' }), {
    target: { value: 'invalid' },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  expect(await screen.findByText('Invalid ledger account')).toBeInTheDocument();
  expect(screen.getByRole('textbox', { name: 'Summary account' })).toHaveValue('invalid');
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
});
