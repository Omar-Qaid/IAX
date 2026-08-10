import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { settingsQueryKeys } from './settingsQueryKeys';
import { settingsService } from '../services/settingsService';
import type { GlobalSettings, UserSettings } from '../types/settingsTypes';

export const useGlobalSettings = () =>
  useQuery({
    queryKey: settingsQueryKeys.global(),
    queryFn: ({ signal }) => settingsService.getGlobal(signal),
  });

export const useUserSettings = () =>
  useQuery({
    queryKey: settingsQueryKeys.user(),
    queryFn: ({ signal }) => settingsService.getUser(signal),
  });

export const useUpdateGlobalSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (settings: GlobalSettings) => settingsService.updateGlobal(settings),
    onSuccess: (settings) => queryClient.setQueryData(settingsQueryKeys.global(), settings),
  });
};

export const useUpdateUserSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (settings: UserSettings) => settingsService.updateUser(settings),
    onSuccess: (settings) => queryClient.setQueryData(settingsQueryKeys.user(), settings),
  });
};
