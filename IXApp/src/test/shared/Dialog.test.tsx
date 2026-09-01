import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';

describe('ConfirmationDialog', () => {
  it('renders confirmation dialog and handles confirm', () => {
    const handleConfirm = vi.fn();
    const handleClose = vi.fn();

    render(
      <ConfirmationDialog
        open={true}
        title="Delete Record"
        message="Are you sure you want to delete this customer?"
        onConfirm={handleConfirm}
        onClose={handleClose}
      />
    );

    const dialog = screen.getByRole('dialog');
    expect(dialog).toHaveAccessibleName('Delete Record');
    expect(dialog).toHaveAccessibleDescription('Are you sure you want to delete this customer?');

    const confirmButton = screen.getByRole('button', { name: 'Confirm' });
    fireEvent.click(confirmButton);
    expect(handleConfirm).toHaveBeenCalledTimes(1);
  });

  it('locks the dialog and shows progress while the action is running', () => {
    render(
      <ConfirmationDialog
        open
        message="Publishing template"
        onConfirm={vi.fn()}
        onClose={vi.fn()}
        loading
      />
    );

    expect(screen.getByRole('button', { name: 'Confirm' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });
});
