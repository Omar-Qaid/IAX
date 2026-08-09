import { describe, it, expect } from 'vitest';
import { SUPPORTED_LANGUAGES, DEFAULT_LANGUAGE } from '@core/localization/languages';
import i18n from '@core/localization/i18n';
import en from '../../../public/locales/en/translation.json';
import ar from '../../../public/locales/ar/translation.json';

const flattenKeys = (value: object, prefix = ''): string[] =>
  Object.entries(value).flatMap(([key, child]) => {
    const path = prefix ? `${prefix}.${key}` : key;
    return child && typeof child === 'object' ? flattenKeys(child, path) : [path];
  });

const normalizePluralKey = (key: string) => key.replace(/_(zero|one|two|few|many|other)$/, '');

describe('localization', () => {
  it('defines English and Arabic as supported languages', () => {
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('en');
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('ar');
  });

  it('sets English as default language', () => {
    expect(DEFAULT_LANGUAGE.code).toBe('en');
  });

  it('keeps English and Arabic resource namespaces in parity', () => {
    const enKeys = new Set(flattenKeys(en).map(normalizePluralKey));
    const arKeys = new Set(flattenKeys(ar).map(normalizePluralKey));
    expect([...enKeys].filter((key) => !arKeys.has(key))).toEqual([]);
    expect([...arKeys].filter((key) => !enKeys.has(key))).toEqual([]);
  });

  it('provides readable Arabic navigation, page, validation, and action text', async () => {
    await i18n.changeLanguage('ar');
    expect(i18n.t('nav.customers')).toBe('العملاء');
    expect(i18n.t('pages.salesOrders.title')).toBe('أوامر البيع');
    expect(i18n.t('actions.save')).toBe('حفظ');
    expect(i18n.t('validation.required', { field: 'الاسم' })).toBe('حقل الاسم مطلوب.');
    expect(i18n.t('lookups.searchOptions')).toBe('البحث في الخيارات…');
    expect(i18n.t('messages.unsavedChanges')).toContain('تغييرات غير محفوظة');
    expect(document.documentElement.dir).toBe('rtl');
    await i18n.changeLanguage('en');
  });
});
