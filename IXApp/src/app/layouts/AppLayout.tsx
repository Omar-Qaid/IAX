import React from 'react';
import { AppShell } from '@shared/components/app-shell/AppShell';

export const AppLayout: React.FC<{ children?: React.ReactNode }> = ({ children }) => {
  return <AppShell>{children}</AppShell>;
};
