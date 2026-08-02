import React from 'react';
import { AppShell } from '@shared/components/app-shell/AppShell';
import { Outlet } from 'react-router-dom';

export const AppLayout: React.FC<{ children?: React.ReactNode }> = ({ children }) => {
  return <AppShell>{children ?? <Outlet />}</AppShell>;
};
