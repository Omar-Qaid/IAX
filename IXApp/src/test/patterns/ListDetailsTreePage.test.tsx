import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { TreeNavigationPane } from '@patterns/list-details-tree/TreeNavigationPane';

interface NodeRecord {
  id: string;
  parentId: string | null;
  name: string;
}

const records: NodeRecord[] = [
  { id: 'root', parentId: null, name: 'Sales' },
  { id: 'branch', parentId: 'root', name: 'Purchase' },
  { id: 'leaf', parentId: 'branch', name: 'Receive' },
];

const renderTree = (visibleRecords = records, query = '') => {
  const onSelect = vi.fn();
  render(
    <TreeNavigationPane
      records={records}
      visibleRecords={visibleRecords}
      selectedId="leaf"
      editing={false}
      loading={false}
      query={query}
      filterVisible
      filterLabel="Filter"
      onQueryChange={vi.fn()}
      onSelect={onSelect}
      tree={{
        getParentId: (record) => record.parentId,
        initiallyExpanded: 'all',
        ariaLabel: 'Categories',
      }}
      getPrimaryText={(record) => record.name}
    />
  );
  return onSelect;
};

describe('TreeNavigationPane', () => {
  it('renders nested records and selects a tree item', async () => {
    const onSelect = renderTree();

    expect(screen.getByRole('tree', { name: 'Categories' })).toBeInTheDocument();
    expect(screen.getByRole('treeitem', { name: /Receive/ })).toHaveAttribute('aria-level', '3');

    await userEvent.click(screen.getByRole('treeitem', { name: /Purchase/ }));
    expect(onSelect).toHaveBeenCalledWith(records[1]);
  });

  it('keeps ancestors visible while filtering to a descendant', () => {
    renderTree([records[2]], 'receive');

    expect(screen.getByRole('treeitem', { name: /Sales/ })).toBeInTheDocument();
    expect(screen.getByRole('treeitem', { name: /Purchase/ })).toBeInTheDocument();
    expect(screen.getByRole('treeitem', { name: /Receive/ })).toBeInTheDocument();
  });
});
