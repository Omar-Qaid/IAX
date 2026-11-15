import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';
import type {
  BalancePartyKind,
  PartyOpenBalanceCurrency,
} from '@patterns/open-balance-currency/types';

export const partyBalanceApi = {
  async load(
    kind: BalancePartyKind,
    accountNum: string,
    signal?: AbortSignal
  ): Promise<PartyOpenBalanceCurrency> {
    if (!accountNum.trim()) throw new ApiError('An account number is required.', 400);
    const resource = kind === 'customer' ? 'CustTrans' : 'VendTrans';
    const { data } = await apiClient.get<ApiResponse<PartyOpenBalanceCurrency>>(
      `/v1/${resource}/balance-currency`,
      {
        params: { accountNum },
        signal,
      }
    );
    if (!data.success || !data.data)
      throw new ApiError(data.message || 'Unable to load the account balance.', 500);
    return data.data;
  },
};
