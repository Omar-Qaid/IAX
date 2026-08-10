import { QueryClient } from '@tanstack/react-query';
import { ApiError } from './apiError';

const shouldRetry = (failureCount: number, error: Error): boolean => {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) return false;
  return failureCount < 1;
};

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: shouldRetry,
      staleTime: 1000 * 60 * 5,
    },
    mutations: { retry: false },
  },
});
