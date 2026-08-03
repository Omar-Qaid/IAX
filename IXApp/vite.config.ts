import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@app': path.resolve(__dirname, './src/app'),
      '@core': path.resolve(__dirname, './src/core'),
      '@shared': path.resolve(__dirname, './src/shared'),
      '@patterns': path.resolve(__dirname, './src/patterns'),
      '@modules': path.resolve(__dirname, './src/modules'),
      '@assets': path.resolve(__dirname, './src/assets'),
      '@mocks': path.resolve(__dirname, './src/mocks'),
      '@test': path.resolve(__dirname, './src/test'),
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setupTests.ts'],
    css: false,
    testTimeout: 30_000,
  },
});
