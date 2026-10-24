import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UnitConversionEditor } from '@modules/finance/inventory/components/UnitConversionEditor';
import { unitOfMeasureApi } from '@modules/finance/inventory/api/unitOfMeasureApi';
import { ecoResProductApi } from '@modules/finance/inventory/api/ecoResProductApi';

vi.mock('@core/localization/useAppTranslation', () => ({ useAppTranslation: () => ({ t: (_key: string, fallback: string) => fallback }) }));
vi.mock('@modules/finance/inventory/api/unitOfMeasureApi', () => ({ unitOfMeasureApi: { list: vi.fn(), createConversion: vi.fn(), updateConversion: vi.fn(), deleteConversion: vi.fn() } }));
vi.mock('@modules/finance/inventory/api/ecoResProductApi', () => ({ ecoResProductApi: { list: vi.fn() } }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

it('saves a fraction conversion and excludes units from another class', async () => {
  const ea = { id: '1', recId: 1, symbol: 'EA', unitOfMeasureClass: 0, systemOfUnits: 0, decimalPrecision: 0, conversions: [] };
  vi.mocked(unitOfMeasureApi.list).mockResolvedValue([ea,
    { ...ea, id: '2', recId: 2, symbol: 'BOX' },
    { ...ea, id: '3', recId: 3, symbol: 'KG', unitOfMeasureClass: 4 }]);
  vi.mocked(ecoResProductApi.list).mockResolvedValue([]);
  vi.mocked(unitOfMeasureApi.createConversion).mockResolvedValue({ id: '4', recId: 4,
    fromUnitOfMeasure: 1, fromSymbol: 'EA', toUnitOfMeasure: 2, toSymbol: 'BOX', product: 0,
    productNumber: '', factor: 0, numerator: 1, denominator: 10, innerOffset: 0, outerOffset: 0, rounding: 0 });
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(<QueryClientProvider client={client}><UnitConversionEditor unit={ea} /></QueryClientProvider>);
  fireEvent.click(screen.getByRole('button', { name: 'New' }));
  const dialog = screen.getByRole('dialog');
  await waitFor(() => expect(unitOfMeasureApi.list).toHaveBeenCalled());
  fireEvent.mouseDown(within(dialog).getByLabelText('To unit'));
  expect(await screen.findByRole('option', { name: 'BOX' })).toBeDefined();
  expect(screen.queryByRole('option', { name: 'KG' })).toBeNull();
  fireEvent.click(screen.getByRole('option', { name: 'BOX' }));
  fireEvent.change(within(dialog).getByLabelText('Numerator'), { target: { value: '1' } });
  fireEvent.change(within(dialog).getByLabelText('Denominator'), { target: { value: '10' } });
  expect(within(dialog).queryByLabelText('Inner offset')).toBeNull();
  fireEvent.click(within(dialog).getByRole('button', { name: 'Save' }));
  await waitFor(() => expect(unitOfMeasureApi.createConversion).toHaveBeenCalledWith(
    expect.objectContaining({ fromUnitOfMeasure: 1, toUnitOfMeasure: 2, factor: 0, numerator: 1, denominator: 10 })));
});
