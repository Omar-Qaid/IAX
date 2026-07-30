import { MOCK_CURRENCIES, type Currency } from '@mocks/data/currencies';

let mockState = [...MOCK_CURRENCIES];

export const currencyService = {
  async getCurrencies(): Promise<Currency[]> {
    await new Promise((r) => setTimeout(r, 150));
    return [...mockState];
  },

  async saveCurrencies(currencies: Currency[]): Promise<Currency[]> {
    await new Promise((r) => setTimeout(r, 300));
    mockState = [...currencies];
    return mockState;
  },

  async deleteCurrency(id: string): Promise<void> {
    await new Promise((r) => setTimeout(r, 200));
    mockState = mockState.filter((c) => c.id !== id);
  },
};
