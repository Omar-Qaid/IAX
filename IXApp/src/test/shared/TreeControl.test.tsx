import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { TreeControl, type TreeControlConfig } from '@shared/components/tree-control';

interface TestNode {
  id: string;
  label: string;
  branch: boolean;
  children: TestNode[];
}

const nodes: TestNode[] = [
  {
    id: 'site-riy',
    label: 'RIY, Riyadh',
    branch: true,
    children: [
      { id: 'warehouse-riy', label: 'RIY-1, Main Warehouse', branch: false, children: [] },
    ],
  },
  {
    id: 'site-jed',
    label: 'JED, Jeddah',
    branch: true,
    children: [
      { id: 'warehouse-jed', label: 'JED-1, Main Warehouse', branch: false, children: [] },
    ],
  },
];

const config: TreeControlConfig<TestNode> = {
  getId: (node) => node.id,
  getLabel: (node) => node.label,
  getChildren: (node) => node.children,
  isBranch: (node) => node.branch,
  selectedId: 'site-riy',
  expandAllLabel: 'Expand',
  collapseAllLabel: 'Collapse',
  ariaLabel: 'Site hierarchy',
};

describe('TreeControl', () => {
  it('expands one branch and all branches from configured hierarchy data', () => {
    render(<TreeControl nodes={nodes} config={config} />);

    expect(screen.queryByText('RIY-1, Main Warehouse')).toBeNull();
    fireEvent.click(screen.getByText('RIY, Riyadh'));
    expect(screen.getByText('RIY-1, Main Warehouse')).toBeDefined();
    expect(screen.queryByText('JED-1, Main Warehouse')).toBeNull();

    fireEvent.click(screen.getByRole('button', { name: 'Expand' }));
    expect(screen.getByText('JED-1, Main Warehouse')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Collapse' })).toBeDefined();
  });
});
