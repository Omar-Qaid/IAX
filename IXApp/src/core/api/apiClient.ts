import axios, { type AxiosInstance } from 'axios';
import { apiConfig } from './apiConfig';
import { setupInterceptors } from './interceptors';

export const apiClient: AxiosInstance = axios.create({
  baseURL: apiConfig.baseUrl,
  timeout: apiConfig.timeoutMs,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

setupInterceptors(apiClient);
