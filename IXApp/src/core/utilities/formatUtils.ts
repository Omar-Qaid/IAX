export function formatCurrency(
  amount?: number | null,
  currencyCode: string = 'USD',
  locale: string = 'en-US'
): string {
  if (amount === undefined || amount === null) return '-';
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency: currencyCode,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatNumber(
  value?: number | null,
  decimals: number = 2,
  locale: string = 'en-US'
): string {
  if (value === undefined || value === null) return '-';
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value);
}
