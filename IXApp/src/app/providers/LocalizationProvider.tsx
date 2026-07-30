import React from 'react';
import '@core/localization/i18n';

export const LocalizationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return <>{children}</>;
};
