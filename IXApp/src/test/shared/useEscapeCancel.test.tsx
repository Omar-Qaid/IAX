import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { useEscapeCancel } from '@shared/hooks/useEscapeCancel';

function Page({ enabled = true, onCancel }: { enabled?: boolean; onCancel: () => void }) {
  useEscapeCancel(enabled, onCancel);
  return <input aria-label="Description" />;
}

describe('generic Escape cancellation', () => {
  it('ignores overlays that are kept mounted but hidden', async () => {
    const onCancel = vi.fn();
    render(
      <>
        <Page onCancel={onCancel} />
        <div aria-hidden="true">
          <div role="dialog" />
        </div>
      </>
    );
    fireEvent.keyDown(window, { key: 'Escape' });
    await waitFor(() => expect(onCancel).toHaveBeenCalledOnce());
  });

  it('cancels from a focused field', async () => {
    const onCancel = vi.fn();
    render(<Page onCancel={onCancel} />);
    fireEvent.keyDown(screen.getByRole('textbox'), { key: 'Escape' });
    await waitFor(() => expect(onCancel).toHaveBeenCalledOnce());
  });

  it('does not cancel when disabled or after unmount', async () => {
    const onCancel = vi.fn();
    const view = render(<Page enabled={false} onCancel={onCancel} />);
    fireEvent.keyDown(window, { key: 'Escape' });
    view.rerender(<Page onCancel={onCancel} />);
    fireEvent.keyDown(window, { key: 'Escape' });
    view.unmount();
    await new Promise((resolve) => setTimeout(resolve, 10));
    expect(onCancel).not.toHaveBeenCalled();
  });

  it('lets a nested grid consume Escape even when its listener runs later', async () => {
    const onCancel = vi.fn();
    render(<Page onCancel={onCancel} />);
    const cancelRow = (event: KeyboardEvent) => event.preventDefault();
    window.addEventListener('keydown', cancelRow);
    try {
      fireEvent.keyDown(window, { key: 'Escape' });
      await new Promise((resolve) => setTimeout(resolve, 10));
      expect(onCancel).not.toHaveBeenCalled();
    } finally {
      window.removeEventListener('keydown', cancelRow);
    }
  });

  it.each(['dialog', 'alertdialog', 'menu', 'listbox'])(
    'leaves Escape to an open %s',
    async (role) => {
      const onCancel = vi.fn();
      render(
        <>
          <Page onCancel={onCancel} />
          <div role={role} />
        </>
      );
      fireEvent.keyDown(window, { key: 'Escape' });
      await new Promise((resolve) => setTimeout(resolve, 10));
      expect(onCancel).not.toHaveBeenCalled();
    }
  );
});
