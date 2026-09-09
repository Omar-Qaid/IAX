import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { Customer } from '@mocks/data/customers';

export interface CustomerLookupOption {
  value: string;
  label: string;
}

export interface CustomerQuickCreateLookups {
  customerGroups: CustomerLookupOption[];
  currencies: CustomerLookupOption[];
  paymentTerms: CustomerLookupOption[];
  paymentMethods: CustomerLookupOption[];
  deliveryTerms: CustomerLookupOption[];
  deliveryModes: CustomerLookupOption[];
}

type LookupRow = Record<string, unknown>;

export interface CustomerQuickCreateInput {
  name: string;
  nameAlias?: string;
  custGroupId: string;
  currencyCode: string;
  custCategory?: string;
  paymTermId?: string;
  paymModeId?: string;
  dlvModeId?: string;
  taxGroupId?: string;
  vatNum?: string;
  countryRegionId?: string;
  memo?: string;
}

export interface CustomerRecord extends Customer {
  recId: number;
  party: number;
  custCategory: string;
  paymTermId: string;
  paymModeId: string;
  dlvModeId: string;
  taxGroupId: string;
  vatNum: string;
  countryRegionId: string;
  memo?: string;
  invoiceAccount: string;
  inventSiteId: string;
  inventLocationId: string;
}

interface CustomerListDto {
  recId: number;
  party: number;
  accountNumber: string;
  name: string;
  nameAr?: string;
  customerGroupId: string;
  currencyCode: string;
  custCategory: string;
  paymTermId: string;
  paymModeId: string;
  dlvModeId: string;
  taxGroupId: string;
  vatNum: string;
  countryRegionId: string;
  memo?: string;
  phone?: string;
  invoiceAccount: string;
  inventSiteId: string;
  inventLocationId: string;
  status: string;
  createdAt: string;
}

const data = (response: ApiResponse<LookupRow[]>, name: string): LookupRow[] => {
  if (!response.success || !Array.isArray(response.data))
    throw new ApiError(response.message || `The ${name} lookup response did not contain data.`, 500);
  return response.data;
};

const text = (row: LookupRow, key: string): string => String(row[key] ?? '').trim();
const options = (rows: LookupRow[], valueKey: string, labelKey: string): CustomerLookupOption[] =>
  rows.map((row) => {
    const value = text(row, valueKey);
    return { value, label: text(row, labelKey) || value };
  }).filter((option) => option.value);

const load = async (endpoint: string, name: string, signal?: AbortSignal): Promise<LookupRow[]> => {
  const response = await apiClient.get<ApiResponse<LookupRow[]>>(endpoint, { signal });
  return data(response.data, name);
};

const requireCustomerData = (response: ApiResponse<CustomerListDto[]>, name: string): CustomerListDto[] => {
  if (!response.success || !Array.isArray(response.data))
    throw new ApiError(response.message || `The ${name} response did not contain data.`, 500);
  return response.data;
};

const toCustomer = (row: CustomerListDto): CustomerRecord => ({
  recId: row.recId,
  party: row.party,
  id: String(row.recId),
  accountNumber: row.accountNumber,
  name: row.name,
  nameAr: row.nameAr,
  customerGroupId: row.customerGroupId,
  currencyCode: row.currencyCode,
  custCategory: row.custCategory,
  paymTermId: row.paymTermId,
  paymModeId: row.paymModeId,
  dlvModeId: row.dlvModeId,
  taxGroupId: row.taxGroupId,
  vatNum: row.vatNum,
  countryRegionId: row.countryRegionId,
  memo: row.memo,
  phone: row.phone,
  invoiceAccount: row.invoiceAccount,
  inventSiteId: row.inventSiteId,
  inventLocationId: row.inventLocationId,
  status: row.status === 'blocked' ? 'blocked' : row.status === 'onHold' ? 'onHold' : 'active',
  createdAt: row.createdAt,
});

export const customerQuickCreateApi = {
  async list(signal?: AbortSignal): Promise<CustomerRecord[]> {
    const response = await apiClient.get<ApiResponse<CustomerListDto[]>>('/v1/CustTable/list', { signal });
    return requireCustomerData(response.data, 'customer list').map(toCustomer);
  },

  async create(input: CustomerQuickCreateInput): Promise<CustomerRecord> {
    const response = await apiClient.post<ApiResponse<CustomerListDto>>('/v1/CustTable/quick-create', input);
    if (!response.data.success || !response.data.data)
      throw new ApiError(response.data.message || 'The customer could not be created.', 500);
    return toCustomer(response.data.data);
  },

  async update(record: CustomerRecord): Promise<CustomerRecord> {
    const input: CustomerQuickCreateInput = {
      name: record.name,
      nameAlias: record.nameAr,
      custGroupId: record.customerGroupId,
      currencyCode: record.currencyCode,
      custCategory: record.custCategory,
      paymTermId: record.paymTermId,
      paymModeId: record.paymModeId,
      dlvModeId: record.dlvModeId,
      taxGroupId: record.taxGroupId,
      vatNum: record.vatNum,
      countryRegionId: record.countryRegionId,
      memo: record.memo,
    };
    const response = await apiClient.put<ApiResponse<CustomerListDto>>(`/v1/CustTable/quick-update/${record.recId}`, input);
    if (!response.data.success || !response.data.data)
      throw new ApiError(response.data.message || 'The customer could not be updated.', 500);
    return toCustomer(response.data.data);
  },

  async remove(record: CustomerRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`/v1/CustTable/${record.recId}`);
    if (!response.data.success) throw new ApiError(response.data.message || 'The customer could not be deleted.', 500);
  },

  async lookups(signal?: AbortSignal): Promise<CustomerQuickCreateLookups> {
    const [groups, currencies, terms, methods, deliveryTerms, deliveryModes] = await Promise.all([
      load('/v1/CustGroup', 'customer groups', signal),
      load('/v1/Currency', 'currencies', signal),
      load('/v1/PaymTerm', 'payment terms', signal),
      load('/v1/CustPaymMode', 'payment methods', signal),
      load('/v1/DlvTerm', 'delivery terms', signal),
      load('/v1/DlvMode', 'delivery modes', signal),
    ]);
    return {
      customerGroups: options(groups, 'custGroupId', 'name'),
      currencies: options(currencies, 'currencyCode', 'txt'),
      paymentTerms: options(terms, 'paymTermId', 'description'),
      paymentMethods: options(methods, 'paymMode', 'name'),
      deliveryTerms: options(deliveryTerms, 'code', 'txt'),
      deliveryModes: options(deliveryModes, 'code', 'txt'),
    };
  },
};
