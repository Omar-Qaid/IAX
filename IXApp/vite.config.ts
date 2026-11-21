import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Every runtime dependency is pre-bundled at start-up. A dependency first reached
  // from a lazily loaded page (a grid lookup, an export, a viewer) would otherwise be
  // optimized mid-session, and Vite answers that with a full page reload -- which in a
  // half-edited form shows the browser's "Leave site?" prompt and drops the edits.
  optimizeDeps: {
    // Crawl every source file at start-up, not only what the visited routes import.
    // Deep imports such as @mui/icons-material/<Icon> are otherwise discovered the
    // first time a page needs one, and that discovery costs a full page reload.
    entries: ['index.html', 'src/**/*.{ts,tsx}', '!src/test/**', '!src/**/*.test.{ts,tsx}'],
    include: [
      '@dnd-kit/core',
      '@dnd-kit/sortable',
      '@dnd-kit/utilities',
      '@emotion/cache',
      '@emotion/react',
      '@emotion/styled',
      '@hookform/resolvers',
      '@mui/icons-material',
      '@mui/lab',
      '@mui/material',
      '@tanstack/react-query',
      '@tanstack/react-virtual',
      'axios',
      'clsx',
      'date-fns',
      'exceljs',
      'html2canvas',
      'i18next',
      'i18next-browser-languagedetector',
      'jsbarcode',
      'jspdf',
      'mammoth',
      'qrcode.react',
      'react',
      'react-dom',
      'react-hook-form',
      'react-i18next',
      'react-router-dom',
      'stylis',
      'stylis-plugin-rtl',
      'utif',
      'zod',
      'zustand',
    ],
  },
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
    testTimeout: 60_000,
    // Heavy page tests (sales order, process builder) starve each other when every
    // core runs a jsdom worker; half the cores keeps the full suite stable.
    maxWorkers: '50%',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'json-summary'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/test/**', 'src/**/*.d.ts', 'src/main.tsx'],
      thresholds: {
        statements: 20,
        branches: 15,
        functions: 20,
        lines: 20,
      },
    },
  },
});
