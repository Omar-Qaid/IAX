import { apiClient } from '@core/api/apiClient';
import { createEntityApi } from '@core/api/createEntityApi';
import { requireApiData } from '@core/api/apiResponseData';
import type { ApiResponse } from '@core/api/apiResponse';
import type { LookupOption } from '@shared/components/lookups/types';
import { customerQuickCreateApi } from './customerQuickCreateApi';

interface EntityRecord {
  id: string;
  recId: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
export interface CustomerPostingProfile extends EntityRecord {
  postingProfile: string;
  name: string;
  postingProfileName: string;
  settlement: number;
  interest: number;
  collectionLetter: number;
}
export interface CustomerPostingAccount extends EntityRecord {
  postingProfile: string;
  accountCode: number;
  num: string;
  collectionLetterCourse: string;
  clearingLedgerDimension: number;
  depositLedgerDimension: number;
  endorseLedgerDimension: number;
  exportSalesLedgerDimension: number;
  liabilitiesForDiscountLedgerDimension: number;
  summaryLedgerDimension: number;
  vatPrepaymentsLedgerDimension: number;
  writeOffLedgerDimension: number;
  custInterest: number;
}
const record = <T extends EntityRecord>(dto: Omit<T, 'id'>): T =>
  ({ ...dto, id: String(dto.recId) }) as T;
const payload = <T extends EntityRecord>({ id: _id, ...dto }: T): Omit<T, 'id'> => dto;
export const customerPostingProfileApi = createEntityApi<
  Omit<CustomerPostingProfile, 'id'>,
  CustomerPostingProfile
>({
  endpoint: '/v1/CustLedger',
  resourceName: 'customer posting profiles',
  toRecord: record<CustomerPostingProfile>,
  toDto: payload,
});
export const customerPostingAccountApi = {
  ...createEntityApi<Omit<CustomerPostingAccount, 'id'>, CustomerPostingAccount>({
    endpoint: '/v1/CustLedgerAccounts',
    resourceName: 'customer posting accounts',
    toRecord: record<CustomerPostingAccount>,
    toDto: payload,
  }),
  async forProfile(profile: string, signal?: AbortSignal): Promise<CustomerPostingAccount[]> {
    return requireApiData(
      (
        await apiClient.get<ApiResponse<Omit<CustomerPostingAccount, 'id'>[]>>(
          `/v1/CustLedgerAccounts/profile/${encodeURIComponent(profile)}`,
          { signal }
        )
      ).data,
      'posting accounts'
    ).map(record<CustomerPostingAccount>);
  },
  async references(accountCode: number, signal?: AbortSignal): Promise<LookupOption[]> {
    if (accountCode === 0)
      return (await customerQuickCreateApi.list(signal)).map((customer) => ({
        id: customer.accountNumber,
        code: customer.accountNumber,
        name: customer.name,
      }));
    if (accountCode === 1) {
      const response = await apiClient.get<ApiResponse<{ custGroupId: string; name: string }[]>>(
        '/v1/CustGroup',
        { signal }
      );
      return requireApiData(response.data, 'customer groups')
        .filter((group) => group.custGroupId)
        .map((group) => ({
          id: group.custGroupId,
          code: group.custGroupId,
          name: group.name ?? group.custGroupId,
        }));
    }
    return [];
  },
};
const base = (dataAreaId: string) => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId,
});
export const newPostingProfile = (company: string): CustomerPostingProfile => ({
  ...base(company),
  postingProfile: '',
  name: '',
  postingProfileName: '',
  settlement: 0,
  interest: 0,
  collectionLetter: 0,
});
export const newPostingAccount = (
  postingProfile: string,
  company: string
): CustomerPostingAccount => ({
  ...base(company),
  postingProfile,
  accountCode: 1,
  num: '',
  collectionLetterCourse: '',
  clearingLedgerDimension: 0,
  depositLedgerDimension: 0,
  endorseLedgerDimension: 0,
  exportSalesLedgerDimension: 0,
  liabilitiesForDiscountLedgerDimension: 0,
  summaryLedgerDimension: 0,
  vatPrepaymentsLedgerDimension: 0,
  writeOffLedgerDimension: 0,
  custInterest: 0,
});
