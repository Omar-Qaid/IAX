import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ReleasedProductBarcodesDialog } from '@modules/finance/inventory/components/ReleasedProductBarcodesDialog';

vi.mock('@tanstack/react-query', () => ({
  useQuery: () => ({ data: [], isError: false, refetch: vi.fn() }),
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key }),
}));
afterEach(() => cleanup());

it('clears unsaved barcode details when the selected released product changes', async () => {
  const units = [{ value: 'EA', label: 'EA' }];
  const { rerender } = render(<ReleasedProductBarcodesDialog product={{ id: '1', itemNumber: 'PAPER' }} units={units} onClose={vi.fn()} />);
  fireEvent.change(screen.getByRole('textbox', { name: /Barcode/ }), { target: { value: '001234' } });
  fireEvent.mouseDown(screen.getByRole('combobox', { name: /Unit/ }));
  fireEvent.click(await screen.findByRole('option', { name: 'EA' }));
  expect(screen.getByRole('button', { name: 'Save' })).toHaveProperty('disabled', false);
  rerender(<ReleasedProductBarcodesDialog product={{ id: '2', itemNumber: 'PEN' }} units={units} onClose={vi.fn()} />);
  await waitFor(() => expect((screen.getByRole('textbox', { name: /Barcode/ }) as HTMLInputElement).value).toBe(''));
  expect(screen.getByRole('button', { name: 'Save' })).toHaveProperty('disabled', true);
});
