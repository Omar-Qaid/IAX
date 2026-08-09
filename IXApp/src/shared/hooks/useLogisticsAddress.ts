import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@core/api/apiClient';
import { environment } from '@core/configuration/environment';
import {
  MOCK_COUNTRY_REGIONS,
  MOCK_STATES,
  MOCK_CITIES,
  MOCK_COUNTIES,
} from '@shared/services/logisticsAddressMockData';
import type { CountryRegion, State, City, County } from '@shared/types/logistics';

export const useCountryRegions = () => {
  return useQuery({
    queryKey: ['CountryRegions'],
    queryFn: async (): Promise<CountryRegion[]> => {
      if (environment.enableMockApi) {
        return MOCK_COUNTRY_REGIONS;
      }
      try {
        const { data } = await apiClient.get<CountryRegion[]>(
          '/LogisticsPostalAddress/CountryRegions'
        );
        return data;
      } catch {
        return MOCK_COUNTRY_REGIONS;
      }
    },
    staleTime: 1000 * 60 * 10,
  });
};

export const useStates = (countryRegionId?: string) => {
  return useQuery({
    queryKey: ['States', countryRegionId],
    queryFn: async (): Promise<State[]> => {
      if (!countryRegionId) return [];
      if (environment.enableMockApi) {
        return MOCK_STATES.filter((s) => s.countryRegionId === countryRegionId);
      }
      try {
        const { data } = await apiClient.get<State[]>(
          `/LogisticsPostalAddress/States/${countryRegionId}`
        );
        return data;
      } catch {
        return MOCK_STATES.filter((s) => s.countryRegionId === countryRegionId);
      }
    },
    enabled: !!countryRegionId,
    staleTime: 1000 * 60 * 10,
  });
};

export const useCities = (stateId?: string) => {
  return useQuery({
    queryKey: ['Cities', stateId],
    queryFn: async (): Promise<City[]> => {
      if (!stateId) return [];
      if (environment.enableMockApi) {
        return MOCK_CITIES.filter((c) => c.stateId === stateId);
      }
      try {
        const { data } = await apiClient.get<City[]>(`/LogisticsPostalAddress/Cities/${stateId}`);
        return data;
      } catch {
        return MOCK_CITIES.filter((c) => c.stateId === stateId);
      }
    },
    enabled: !!stateId,
    staleTime: 1000 * 60 * 10,
  });
};

export const useCounties = (stateId?: string) => {
  return useQuery({
    queryKey: ['Counties', stateId],
    queryFn: async (): Promise<County[]> => {
      if (!stateId) return [];
      if (environment.enableMockApi) {
        return MOCK_COUNTIES.filter((c) => c.stateId === stateId);
      }
      try {
        const { data } = await apiClient.get<County[]>(
          `/LogisticsPostalAddress/Counties/${stateId}`
        );
        return data;
      } catch {
        return MOCK_COUNTIES.filter((c) => c.stateId === stateId);
      }
    },
    enabled: !!stateId,
    staleTime: 1000 * 60 * 10,
  });
};
