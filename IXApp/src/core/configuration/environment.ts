export interface AppEnvironment {
  apiBaseUrl: string;
  enableMockApi: boolean;
}

export const environment: AppEnvironment = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
  enableMockApi: import.meta.env.VITE_ENABLE_MOCK_API === 'true',
};
