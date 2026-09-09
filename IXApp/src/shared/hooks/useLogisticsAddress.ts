import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@core/api/apiClient';
import type { CountryRegion, State, City, County } from '@shared/types/logistics';
import type { ApiResponse } from '@core/api/apiResponse';
import { ApiError } from '@core/api/apiError';

const unwrap = <T,>(response: ApiResponse<T[]>, lookup: string): T[] => {
  if (!response.success || !Array.isArray(response.data))
    throw new ApiError(response.message || `The ${lookup} lookup response did not contain data.`, 500);
  return response.data;
};

export const useCountryRegions = () => {
  return useQuery({
    queryKey: ['CountryRegions'],
    queryFn: async (): Promise<CountryRegion[]> => {
      const { data } = await apiClient.get<ApiResponse<CountryRegion[]>>(
        '/v1/LogisticsPostalAddress/CountryRegions'
      );
      return unwrap(data, 'country/region');
    },
    staleTime: 1000 * 60 * 10,
  });
};

export const useStates = (countryRegionId?: string) => {
  return useQuery({
    queryKey: ['States', countryRegionId],
    queryFn: async (): Promise<State[]> => {
      if (!countryRegionId) return [];
      const { data } = await apiClient.get<ApiResponse<State[]>>(
        `/v1/LogisticsPostalAddress/States/${encodeURIComponent(countryRegionId)}`
      );
      return unwrap(data, 'state');
    },
    enabled: !!countryRegionId,
    staleTime: 1000 * 60 * 10,
  });
};

export const useCities = (countryRegionId?: string, stateId?: string) => {
  return useQuery({
    queryKey: ['Cities', countryRegionId, stateId],
    queryFn: async (): Promise<City[]> => {
      if (!stateId) return [];
      if (!countryRegionId) return [];
      const { data } = await apiClient.get<ApiResponse<City[]>>(
        `/v1/LogisticsPostalAddress/Cities/${encodeURIComponent(countryRegionId)}/${encodeURIComponent(stateId)}`
      );
      return unwrap(data, 'city');
    },
    enabled: !!countryRegionId && !!stateId,
    staleTime: 1000 * 60 * 10,
  });
};

export const useCounties = (countryRegionId?: string, stateId?: string) => {
  return useQuery({
    queryKey: ['Counties', countryRegionId, stateId],
    queryFn: async (): Promise<County[]> => {
      if (!stateId) return [];
      if (!countryRegionId) return [];
      const { data } = await apiClient.get<ApiResponse<County[]>>(
        `/v1/LogisticsPostalAddress/Counties/${encodeURIComponent(countryRegionId)}/${encodeURIComponent(stateId)}`
      );
      return unwrap(data, 'county');
    },
    enabled: !!countryRegionId && !!stateId,
    staleTime: 1000 * 60 * 10,
  });
};
