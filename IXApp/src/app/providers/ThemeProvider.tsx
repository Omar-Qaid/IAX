import React, { useMemo, useEffect } from 'react';
import { ThemeProvider as MuiThemeProvider, CssBaseline } from '@mui/material';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { createAppTheme } from '@app/theme/createAppTheme';

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const themeMode = usePreferenceStore((s) => s.themeMode);
  const { currentLanguage } = useAppTranslation();

  const theme = useMemo(() => {
    return createAppTheme(themeMode, currentLanguage.dir);
  }, [themeMode, currentLanguage.dir]);

  useEffect(() => {
    document.dir = currentLanguage.dir;
    document.documentElement.lang = currentLanguage.code;
  }, [currentLanguage.dir, currentLanguage.code]);

  return (
    <MuiThemeProvider theme={theme}>
      <CssBaseline />
      {children}
    </MuiThemeProvider>
  );
};
