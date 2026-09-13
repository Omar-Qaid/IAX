import React from 'react';
import { createBrowserRouter, RouterProvider, Outlet, useBlocker } from 'react-router-dom';
import { unsavedChanges } from '@core/navigation/unsavedChanges';
import { appRoutes } from './routeConfig';

function NavigationGuard(): React.ReactElement {
  const dirty = React.useSyncExternalStore(unsavedChanges.subscribe, unsavedChanges.isDirty);
  const blocker = useBlocker(dirty);
  React.useEffect(() => {
    if (blocker.state !== 'blocked') return;
    if (unsavedChanges.confirmDiscard()) blocker.proceed();
    else blocker.reset();
  }, [blocker]);
  return <Outlet />;
}

export const AppRoutes: React.FC = () => {
  const [router] = React.useState(() =>
    createBrowserRouter([{ element: <NavigationGuard />, children: appRoutes }])
  );
  return <RouterProvider router={router} />;
};
