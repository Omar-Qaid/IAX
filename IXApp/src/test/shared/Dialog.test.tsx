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

    expect(screen.getByText('Delete Record')).toBeDefined();
    expect(screen.getByText('Are you sure you want to delete this customer?')).toBeDefined();

    const confirmButton = screen.getByText('Confirm');
    fireEvent.click(confirmButton);
    expect(handleConfirm).toHaveBeenCalledTimes(1);
  });
});
