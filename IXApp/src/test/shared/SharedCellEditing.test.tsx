import React from 'react';
import { expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@test/testUtils';
import { BufferedValueField } from '@shared/components/fields/BufferedValueField';
import { createCellRenderer } from '@shared/components/data-grid/createCellRenderer';
import { CommitAwareCrudActions } from '@shared/components/action-pane/CommitAwareCrudActions';

it('preserves partial decimal and empty input while accepting real upstream changes', () => {
  function Editor() {
    const [value, setValue] = React.useState(5);
    return (
      <>
        <BufferedValueField
          type="number"
          value={value}
          onChange={(event) => setValue(Number(event.target.value))}
          slotProps={{ htmlInput: { 'aria-label': 'Amount' } }}
        />
        <button onClick={() => setValue(37)}>Reset</button>
      </>
    );
  }
  render(<Editor />);
  const input = screen.getByRole('spinbutton', { name: 'Amount' });
  fireEvent.change(input, { target: { value: '1.' } });
  expect(input).toHaveValue('1.');
  fireEvent.change(input, { target: { value: '' } });
  expect(input).toHaveValue('');
  fireEvent.click(screen.getByText('Reset'));
  expect(input).toHaveValue('37');
});

it('updates generic cell rendering without replacing columns and isolates renderer providers', () => {
  type Row = { id: string; amount: number };
  const first = createCellRenderer<Row>();
  const second = createCellRenderer<Row>();
  const column = { field: 'amount' as const, headerName: 'Amount' };
  const params = { row: { id: '1', amount: 4 }, value: 4, rowIndex: 0 };
  const view = (label: string) => (
    <first.Provider renderCell={(_column, cell) => `${label}: ${cell.value}`}>
      <second.Provider renderCell={(_column, cell) => `Other: ${cell.value}`}>
        <span>
          <first.Cell column={column} params={params} />
        </span>
        <span>
          <second.Cell column={column} params={params} />
        </span>
      </second.Provider>
    </first.Provider>
  );
  const { rerender } = render(view('Initial'));
  expect(screen.getByText('Initial: 4')).toBeInTheDocument();
  expect(screen.getByText('Other: 4')).toBeInTheDocument();
  rerender(view('Changed'));
  expect(screen.getByText('Changed: 4')).toBeInTheDocument();
  expect(screen.getByText('Other: 4')).toBeInTheDocument();
});

const labels = {
  editLabel: 'Edit',
  newLabel: 'New',
  deleteLabel: 'Delete',
  saveLabel: 'Save',
  cancelLabel: 'Cancel',
  editing: true,
  canEdit: true,
  canDelete: false,
};
it('blocks page Save after a rejected line commit and clears drafts before Cancel', async () => {
  const onSave = vi.fn();
  const order: string[] = [];
  render(
    <CommitAwareCrudActions
      {...labels}
      beforeSave={async () => false}
      onSave={onSave}
      onCancelDraft={() => order.push('draft')}
      onCancel={() => order.push('page')}
    />
  );
  await act(async () => {
    fireEvent.click(screen.getByLabelText('Save'));
  });
  expect(onSave).not.toHaveBeenCalled();
  fireEvent.click(screen.getByLabelText('Cancel'));
  expect(order).toEqual(['draft', 'page']);
});

it('locks actions until both the line commit and asynchronous page save finish', async () => {
  let commit!: (accepted: boolean) => void;
  let finish!: () => void;
  const beforeSave = vi.fn(
    () =>
      new Promise<boolean>((resolve) => {
        commit = resolve;
      })
  );
  const onSave = vi.fn(
    () =>
      new Promise<void>((resolve) => {
        finish = resolve;
      })
  );
  const onCancel = vi.fn();
  render(
    <CommitAwareCrudActions
      {...labels}
      beforeSave={beforeSave}
      onSave={onSave}
      onCancel={onCancel}
    />
  );
  fireEvent.click(screen.getByLabelText('Save'));
  fireEvent.click(screen.getByLabelText('Save'));
  expect(beforeSave).toHaveBeenCalledOnce();
  expect(screen.getByLabelText('Cancel')).toBeDisabled();
  await act(async () => {
    commit(true);
  });
  expect(onSave).toHaveBeenCalledOnce();
  expect(screen.getByLabelText('Save')).toBeDisabled();
  fireEvent.keyDown(document, { key: 'Escape' });
  expect(onCancel).not.toHaveBeenCalled();
  await act(async () => {
    finish();
  });
  await waitFor(() => expect(screen.getByLabelText('Save')).toBeEnabled());
});
