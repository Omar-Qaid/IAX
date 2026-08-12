import React from 'react';
import { describe, expect, it } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen } from '@test/testUtils';
import { ProcessBuilderPage } from '@patterns/process-builder';

describe('ProcessBuilderPage', () => {
  it('connects tree selection, tabs, summary, and contextual properties', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage title="Purchase approval" code="WF-001" active nodes={[{ id: 'process-1', kind: 'process', label: 'Purchase approval', children: [{ id: 'step-1', kind: 'step', label: 'Manager review' }] }]} tabs={[{ id: 'designer', label: 'Designer', content: <div>Designer canvas</div> }, { id: 'steps', label: 'Steps', content: <div>Steps content</div> }]} summary={[{ label: 'steps', value: 1 }]} properties={(node) => <div>{node?.label ?? 'None'} properties</div>} />);
    expect(screen.getByRole('heading', { name: 'Purchase approval' })).toBeDefined();
    expect(screen.getByText('1 steps')).toBeDefined();
    expect(screen.getByText('Designer canvas')).toBeDefined();
    await user.click(screen.getByRole('treeitem', { name: /Manager review/ }));
    expect(screen.getByText('Manager review properties')).toBeDefined();
    await user.click(screen.getByRole('tab', { name: 'Steps' }));
    expect(screen.getByText('Steps content')).toBeDefined();
  });
});
