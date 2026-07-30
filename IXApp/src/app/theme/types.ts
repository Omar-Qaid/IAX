import type { ThemeMode } from '@app/store/usePreferenceStore';

export interface ThemeConfig {
  mode: ThemeMode;
  direction: 'ltr' | 'rtl';
}
