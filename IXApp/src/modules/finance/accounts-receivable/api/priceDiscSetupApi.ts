import { createEntityApi } from '@core/api/createEntityApi';

export interface TradeAgreementJournal {
  id: string;
  recId: number;
  journalNum: string;
  journalName: string;
  name: string;
  defaultRelation: number;
  posted: number;
  postedDate: string | null;
  exportCurrentPrice: number;
  lockedForDeletion: number;
  priceGroup: string;
  priceComponentCombination: number;
  priceApplyAdjustment: number;
  partition: number;
}

export interface PriceDiscountGroup {
  id: string;
  recId: number;
  groupId: string;
  name: string;
  type: number;
  module: number;
  mcrPriceDiscGroupType: number;
  retailCheckSalesPriceStatus: number;
  retailPricingPriorityNumber: number;
  pricingRuleRecId: number;
  priceGroupAttributeEnable: number;
  partition: number;
}

export interface TradeAgreementJournalName {
  id: string;
  recId: number;
  journalName: string;
  name: string;
  defaultRelation: number;
  priceDiscPriceAttributeEnable: number;
  partition: number;
}

type WithoutId<T> = Omit<T, 'id'>;
const recordAdapter = <T extends { recId: number }>() => ({
  toRecord: (dto: WithoutId<T>): T => ({ ...dto, id: String(dto.recId) }) as T,
  toDto: ({ id: _id, ...dto }: T): WithoutId<T> => dto as WithoutId<T>,
});

export const tradeAgreementJournalApi = createEntityApi<WithoutId<TradeAgreementJournal>, TradeAgreementJournal>({
  endpoint: '/v1/PriceDiscAdmTable', resourceName: 'trade agreement journals', ...recordAdapter<TradeAgreementJournal>(),
});

export const priceDiscountGroupApi = createEntityApi<WithoutId<PriceDiscountGroup>, PriceDiscountGroup>({
  endpoint: '/v1/PriceDiscGroup', resourceName: 'price/discount groups', ...recordAdapter<PriceDiscountGroup>(),
});

export const tradeAgreementJournalNameApi = createEntityApi<WithoutId<TradeAgreementJournalName>, TradeAgreementJournalName>({
  endpoint: '/v1/PriceDiscAdmName', resourceName: 'trade agreement journal names', ...recordAdapter<TradeAgreementJournalName>(),
});

export const newTradeAgreementJournal = (): TradeAgreementJournal => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, journalNum: '', journalName: '', name: '', defaultRelation: 0,
  posted: 0, postedDate: null, exportCurrentPrice: 0, lockedForDeletion: 0, priceGroup: '',
  priceComponentCombination: 0, priceApplyAdjustment: 0, partition: 0,
});

export const newPriceDiscountGroup = (): PriceDiscountGroup => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, groupId: '', name: '', type: 0, module: 2,
  mcrPriceDiscGroupType: 0, retailCheckSalesPriceStatus: 0, retailPricingPriorityNumber: 0,
  pricingRuleRecId: 0, priceGroupAttributeEnable: 0, partition: 0,
});

export const newTradeAgreementJournalName = (): TradeAgreementJournalName => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, journalName: '', name: '', defaultRelation: 0,
  priceDiscPriceAttributeEnable: 0, partition: 0,
});
