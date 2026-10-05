import { createEntityApi } from '@core/api/createEntityApi';

export interface CustomerGroupRecord {
  id: string;
  recId: number;
  custGroupId: string;
  name: string;
  paymTermId: string;
  taxGroupId: string;
  clearingPeriod: string;
  priceIncludeSalesTax: number;
  taxPeriodPaymentCode: string;
  custWriteOffRefRecId: number;
  custAccountNumSeq: number;
  isPublicSector: number;
  accountingCurrencyExchangeRateType: number;
  reportingCurrencyExchangeRateType: number;
  bankCustPaymIdTable: number;
  defaultDimension: number;
}

type CustomerGroupDto = Omit<CustomerGroupRecord, 'id'>;

export const customerGroupApi = createEntityApi<CustomerGroupDto, CustomerGroupRecord>({
  endpoint: '/v1/CustGroup',
  resourceName: 'customer groups',
  toRecord: (dto) => ({ ...dto, id: String(dto.recId) }),
  toDto: ({ id: _id, ...dto }) => dto,
});

export const newCustomerGroup = (): CustomerGroupRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  custGroupId: '',
  name: '',
  paymTermId: '',
  taxGroupId: '',
  clearingPeriod: '',
  priceIncludeSalesTax: 0,
  taxPeriodPaymentCode: '',
  custWriteOffRefRecId: 0,
  custAccountNumSeq: 0,
  isPublicSector: 0,
  accountingCurrencyExchangeRateType: 0,
  reportingCurrencyExchangeRateType: 0,
  bankCustPaymIdTable: 0,
  defaultDimension: 0,
});
