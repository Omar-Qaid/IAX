import React from 'react';
import { AppProviders } from '@app/providers/AppProviders';
import { AppRoutes } from '@app/routes/AppRoutes';

export function App(): React.ReactElement {
  return (
    <AppProviders>
      <AppRoutes />
    </AppProviders>
  );
}
