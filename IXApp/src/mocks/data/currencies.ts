export interface Currency {
  id: string;
  currencyCode: string;
  name: string;
  symbol: string;
  numberOfDecimals: number;
  active: boolean;
}

export const MOCK_CURRENCIES: Currency[] = [
  { id: 'curr-1', currencyCode: 'USD', name: 'US Dollar', symbol: '$', numberOfDecimals: 2, active: true },
  { id: 'curr-2', currencyCode: 'EUR', name: 'Euro', symbol: '€', numberOfDecimals: 2, active: true },
  { id: 'curr-3', currencyCode: 'SAR', name: 'Saudi Riyal', symbol: 'ر.س', numberOfDecimals: 2, active: true },
  { id: 'curr-4', currencyCode: 'AED', name: 'UAE Dirham', symbol: 'د.إ', numberOfDecimals: 2, active: true },
  { id: 'curr-5', currencyCode: 'GBP', name: 'Pound Sterling', symbol: '£', numberOfDecimals: 2, active: true },
];
