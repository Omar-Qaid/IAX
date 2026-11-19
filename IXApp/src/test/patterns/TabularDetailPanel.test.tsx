import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';

const renderPanel = (showFilterRow: boolean) =>
  render(
    <TabularDetailPanel
      rows={[{ id: '1', name: 'Submit requests' }]}
      columns={[{ field: 'name', headerName: 'Task description' }]}
      addLabel="New"
      removeLabel="Delete"
      selectedIds={[]}
      onSelectionChange={vi.fn()}
      showFilterRow={showFilterRow}
    />
  );

describe('TabularDetailPanel filter row', () => {
  it('uses compact invoice defaults and forwards edit requests from the pencil', () => {
    const onCellEditRequest = vi.fn();
    const row = { id: '1', name: 'Consulting' };
    render(
      <TabularDetailPanel
        rows={[row]}
        columns={[{ field: 'name', headerName: 'Description', editable: true }]}
        addLabel="New"
        removeLabel="Delete"
        selectedIds={[]}
        onSelectionChange={vi.fn()}
        onCellEditRequest={onCellEditRequest}
      />
    );
    expect(screen.getByRole('gridcell')).toHaveStyle({ height: '24px' });
    expect(screen.getByRole('columnheader')).toHaveStyle({ height: '27px' });
    fireEvent.click(screen.getByRole('button', { name: 'Edit Description' }));
    expect(onCellEditRequest).toHaveBeenCalledWith(row, 'name');
  });

  it('hides column filters by default', () => {
    renderPanel(false);
    expect(screen.queryByRole('textbox', { name: /Task description/i })).toBeNull();
  });

  it('shows column filters when requested by the owning page', () => {
    renderPanel(true);
    expect(screen.getByRole('textbox', { name: /Task description/i })).toBeDefined();
  });
});
