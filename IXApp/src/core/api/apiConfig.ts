import { environment } from '@core/configuration/environment';

export interface ApiConfig {
  baseUrl: string;
  enableMock: boolean;
  appName: string;
  timeoutMs: number;
}

export const apiConfig: ApiConfig = {
  baseUrl: environment.apiBaseUrl || 'https://localhost:7001/api',
  enableMock: environment.enableMockApi,
  appName: import.meta.env.VITE_APP_NAME || 'IXApp',
  timeoutMs: 30000,
};
