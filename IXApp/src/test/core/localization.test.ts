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

const flattenValues = (value: object): unknown[] =>
  Object.values(value).flatMap((child) =>
    child && typeof child === 'object' ? flattenValues(child) : [child]
  );

describe('localization', () => {
  it('defines English and Arabic as supported languages', () => {
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('en');
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('ar');
  });

  it('sets English as default language', () => {
    expect(DEFAULT_LANGUAGE.code).toBe('en');
  });

  it('keeps English and Arabic resource namespaces in parity', () => {
    const enKeys = new Set(flattenKeys(en));
    const arKeys = new Set(flattenKeys(ar));
    expect([...enKeys].filter((key) => !arKeys.has(key))).toEqual([]);
    expect([...arKeys].filter((key) => !enKeys.has(key))).toEqual([]);
  });

  it('does not contain empty or undefined translations', () => {
    for (const resource of [en, ar]) {
      expect(flattenValues(resource).every((entry) => typeof entry === 'string' && entry.trim()))
        .toBe(true);
    }
  });

  it('provides readable Arabic navigation, page, validation, and action text', async () => {
    await i18n.changeLanguage('ar');
    expect(i18n.t('nav.customers')).toBe('العملاء');
    expect(i18n.t('pages.salesOrders.title')).toBe('أوامر البيع');
    expect(i18n.t('pages.requestSubmission.title')).toBe('تقديم الطلبات');
    expect(i18n.t('pages.requestSubmission.chooseType')).toBe('اختر نوع الطلب');
    expect(i18n.t('actions.save')).toBe('حفظ');
    expect(i18n.t('actions.back')).toBe('رجوع');
    expect(i18n.t('common.options')).toBe('خيارات');
    expect(i18n.t('nav.workspaces')).toBe('مساحات العمل');
    expect(i18n.t('nav.finance_operations')).toBe('المالية والعمليات');
    expect(i18n.t('mail.folders.inbox')).toBe('الوارد');
    expect(i18n.t('mail.transactionDetails')).toBe('تفاصيل المعاملة');
    expect(i18n.t('mail.trackingLog')).toBe('سجل التتبع');
    expect(i18n.t('wfProcessBuilder.title')).toBe('مصمم العمليات');
    expect(i18n.t('wfProcessBuilder.tabsExtended.requestForm')).toBe('نموذج الطلب');
    expect(i18n.t('wfProcessBuilder.actions.save')).toBe('حفظ');
    expect(i18n.t('wfProcessBuilder.controlTypes.employeesearch')).toBe('بحث عن موظف');
    expect(i18n.t('validation.required', { field: 'الاسم' })).toBe('حقل الاسم مطلوب.');
    expect(i18n.t('lookups.searchOptions')).toBe('البحث في الخيارات…');
    expect(i18n.t('messages.unsavedChanges')).toContain('تغييرات غير محفوظة');
    expect(document.documentElement.dir).toBe('rtl');
    await i18n.changeLanguage('en');
  });
});
