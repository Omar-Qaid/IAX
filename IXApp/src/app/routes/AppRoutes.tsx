import React from 'react';
import { useRoutes, BrowserRouter } from 'react-router-dom';
import { appRoutes } from './routeConfig';

const AppRoutesContent: React.FC = () => {
  const element = useRoutes(appRoutes);
  return element;
};

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <AppRoutesContent />
    </BrowserRouter>
  );
};
