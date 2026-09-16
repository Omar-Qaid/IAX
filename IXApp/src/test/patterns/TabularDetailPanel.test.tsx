import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
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
  it('hides column filters by default', () => {
    renderPanel(false);
    expect(screen.queryByRole('textbox', { name: /Task description/i })).toBeNull();
  });

  it('shows column filters when requested by the owning page', () => {
    renderPanel(true);
    expect(screen.getByRole('textbox', { name: /Task description/i })).toBeDefined();
  });
});
