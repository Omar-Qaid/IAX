import React from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { DataGridEmptyState } from '@shared/components/data-grid/DataGridEmptyState';

describe('DataGridEmptyState', () => {
  it('shows a transparent watermark for an empty dataset', () => {
    render(<DataGridEmptyState />);

    expect(screen.getByText('No records found')).toBeDefined();
    expect(screen.getByText('There are no records to display.')).toBeDefined();
  });

  it('keeps feedback when active filters return no results', () => {
    render(<DataGridEmptyState hasActiveFilters />);

    expect(screen.getByText('No matching records')).toBeDefined();
  });
});
