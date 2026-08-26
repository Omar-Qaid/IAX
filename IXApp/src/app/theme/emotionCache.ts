import createCache from '@emotion/cache';
import { prefixer } from 'stylis';
import rtlPlugin from 'stylis-plugin-rtl';

export const ltrEmotionCache = createCache({
  key: 'mui-ltr',
  prepend: true,
});

export const rtlEmotionCache = createCache({
  key: 'mui-rtl',
  prepend: true,
  stylisPlugins: [prefixer, rtlPlugin],
});
