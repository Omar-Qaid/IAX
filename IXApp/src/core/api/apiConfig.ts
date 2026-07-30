export interface ApiConfig {
  baseUrl: string;
  enableMock: boolean;
  appName: string;
  timeoutMs: number;
}

export const apiConfig: ApiConfig = {
  baseUrl: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001/api',
  enableMock: import.meta.env.VITE_ENABLE_MOCK_API === 'true',
  appName: import.meta.env.VITE_APP_NAME || 'IXApp',
  timeoutMs: 30000,
};
