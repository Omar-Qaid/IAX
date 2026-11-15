export type BalancePartyKind = 'customer' | 'vendor';

export interface OpenBalanceCurrency {
  currencyCode: string;
  amountCur: number;
  amountMst: number;
  amountReporting: number | null;
}

export interface PartyOpenBalanceCurrency {
  recId: number;
  accountNum: string;
  name: string;
  nameAlias: string;
  currencyCode: string;
  accountingCurrency: string;
  reportingCurrency: string;
  balanceMst: number;
  balanceReporting: number | null;
  creditMax: number;
  reconciled: boolean;
  relatedAccounts: string[];
  currencies: OpenBalanceCurrency[];
}
