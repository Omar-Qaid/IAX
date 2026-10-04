export interface DiscountableSalesLine {
  quantity: number;
  unitPrice: number;
  lineDiscount?: number;
  lineDiscountPercent?: number;
  multiLineDiscount?: number;
}

const round = (value: number, decimals: number) => {
  const factor = 10 ** decimals;
  return Math.round((value + Number.EPSILON) * factor) / factor;
};

export const salesLineGrossAmount = (line: DiscountableSalesLine) =>
  Math.max(0, Number(line.quantity || 0) * Number(line.unitPrice || 0));

export const synchronizeSalesLineDiscount = <T extends DiscountableSalesLine>(
  line: T,
  field: 'quantity' | 'unitPrice' | 'lineDiscount' | 'lineDiscountPercent',
  value: number
): T => {
  const next = { ...line, [field]: Number.isFinite(value) ? value : 0 };
  const gross = salesLineGrossAmount(next);

  if (field === 'lineDiscount') {
    const amount = Math.max(0, next.lineDiscount ?? 0);
    return {
      ...next,
      lineDiscount: round(amount, 2),
      lineDiscountPercent: gross > 0 ? round((amount / gross) * 100, 4) : 0,
    };
  }

  const percent = Math.max(0, next.lineDiscountPercent ?? 0);
  return {
    ...next,
    lineDiscountPercent: round(percent, 4),
    lineDiscount: round((gross * percent) / 100, 2),
  };
};

export const salesLineNetAmount = (line: DiscountableSalesLine) =>
  Math.max(
    0,
    salesLineGrossAmount(line) -
      Number(line.lineDiscount || 0) -
      Number(line.multiLineDiscount || 0)
  );
