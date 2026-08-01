import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/core/api/apiClient';
import type { ApiResponse } from '@/core/types/api';

export const useCountryRegions = () => {
    return useQuery({
        queryKey: ['CountryRegions'],
        queryFn: async () => {
            const { data } = await apiClient.get<ApiResponse<{ countryRegionId: string; isoCode: string }[]>>('/LogisticsPostalAddress/CountryRegions');
            return data.data;
        },
    });
};

export const useStates = (countryRegionId: string) => {
    return useQuery({
        queryKey: ['States', countryRegionId],
        queryFn: async () => {
            const { data } = await apiClient.get<ApiResponse<{ stateId: string; name: string }[]>>(`/LogisticsPostalAddress/States/${countryRegionId}`);
            return data.data;
        },
        enabled: !!countryRegionId,
    });
};

export const useCities = (stateId: string) => {
    return useQuery({
        queryKey: ['Cities', stateId],
        queryFn: async () => {
            const { data } = await apiClient.get<ApiResponse<{ cityKey: string; name: string }[]>>(`/LogisticsPostalAddress/Cities/${stateId}`);
            return data.data;
        },
        enabled: !!stateId,
    });
};

export const useCounties = (stateId: string) => {
    return useQuery({
        queryKey: ['Counties', stateId],
        queryFn: async () => {
            const { data } = await apiClient.get<ApiResponse<{ countyId: string; name: string }[]>>(`/LogisticsPostalAddress/Counties/${stateId}`);
            return data.data;
        },
        enabled: !!stateId,
    });
};
