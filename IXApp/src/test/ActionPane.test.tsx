import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@test/testUtils';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';

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
});
