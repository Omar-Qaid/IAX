import { describe, it, expect, vi } from 'vitest';
import { useState } from 'react';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneRibbon } from '@shared/components/action-pane/ActionPaneRibbon';
import { ActionPaneRibbonTrigger } from '@shared/components/action-pane/ActionPaneRibbonTrigger';

describe('ActionPane Component Suite', () => {
  it('renders action buttons and handles click events', () => {
    const handleClick = vi.fn();
    render(
      <ActionPane>
        <ActionPaneGroup label="Maintain">
          <ActionPaneButton label="New Record" onClick={handleClick} />
        </ActionPaneGroup>
      </ActionPane>
    );

    const button = screen.getByText('New Record');
    expect(button).toBeInTheDocument();
    fireEvent.click(button);
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('disables button when disabled prop is true', () => {
    const handleClick = vi.fn();
    render(
      <ActionPane>
        <ActionPaneGroup label="Maintain">
          <ActionPaneButton label="Delete Record" disabled onClick={handleClick} />
        </ActionPaneGroup>
      </ActionPane>
    );

    const button = screen.getByText('Delete Record').closest('button');
    expect(button).toBeDisabled();
    if (button) fireEvent.click(button);
    expect(handleClick).not.toHaveBeenCalled();
  });

  it('disables actions that do not have an implementation', () => {
    render(
      <ActionPane>
        <ActionPaneGroup label="Maintain">
          <ActionPaneButton label="Future action" />
        </ActionPaneGroup>
      </ActionPane>
    );

    expect(screen.getByText('Future action').closest('button')).toBeDisabled();
  });

  it('renders grouped expanded ribbon content and supports pinning', async () => {
    const handleAction = vi.fn();
    const handleClickAway = vi.fn();

    function RibbonHarness() {
      const [pinned, setPinned] = useState(false);
      return (
        <ActionPaneRibbon
          groups={[
            {
              id: 'new',
              label: 'New',
              actions: [
                { id: 'service-order', label: 'Service order', onClick: handleAction },
                { id: 'future', label: 'Future action', disabled: true },
              ],
            },
          ]}
          pinned={pinned}
          onPinnedChange={setPinned}
        />
      );
    }

    render(
      <ActionPane expandedContent={<RibbonHarness />} onClickAway={handleClickAway}>
        <ActionPaneButton
          label="Sales order"
          selected
          controlsId="sales-order-ribbon"
          onClick={() => undefined}
        />
      </ActionPane>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Service order' }));
    expect(handleAction).toHaveBeenCalledTimes(1);
    expect(screen.getByRole('button', { name: 'Future action' })).toBeDisabled();
    fireEvent.click(screen.getByRole('button', { name: 'Pin action pane' }));
    expect(screen.getByRole('button', { name: 'Unpin action pane' })).toHaveAttribute(
      'aria-pressed',
      'true'
    );
    await new Promise((resolve) => window.setTimeout(resolve, 0));
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await waitFor(() => expect(handleClickAway).toHaveBeenCalledTimes(1));
  });

  it('opens and dismisses a generic ribbon trigger', async () => {
    render(
      <ActionPane>
        <ActionPaneRibbonTrigger
          id="options"
          label="Options"
          groups={[
            {
              id: 'record',
              label: 'Record',
              actions: [{ id: 'record-info', label: 'Record information' }],
            },
          ]}
        />
      </ActionPane>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Options' }));
    expect(screen.getByText('Record information')).toBeVisible();
    await new Promise((resolve) => window.setTimeout(resolve, 0));
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await waitFor(() => expect(screen.queryByText('Record information')).toBeNull());
  });
});
