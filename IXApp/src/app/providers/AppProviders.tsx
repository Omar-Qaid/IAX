import React from 'react';
import { AuthProvider } from '@core/auth/AuthProvider';
import { QueryProvider } from './QueryProvider';
import { ThemeProvider } from './ThemeProvider';
import { LocalizationProvider } from './LocalizationProvider';
import { NotificationProvider } from './NotificationProvider';
import { ErrorBoundary } from '@core/errors/ErrorBoundary';

export const AppProviders: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <ErrorBoundary>
      <LocalizationProvider>
        <AuthProvider>
          <QueryProvider>
            <ThemeProvider>
              <NotificationProvider>{children}</NotificationProvider>
            </ThemeProvider>
          </QueryProvider>
        </AuthProvider>
      </LocalizationProvider>
    </ErrorBoundary>
  );
};
