import i18n from '@core/localization/i18n';

export const validationMessage = {
  required: (field: string) => i18n.t('validation.required', { field }),
  invalidEmail: () => i18n.t('validation.invalidEmail'),
  invalidUrl: () => i18n.t('validation.invalidUrl'),
  minLength: (field: string, min: number) => i18n.t('validation.minLength', { field, min }),
  maxLength: (field: string, max: number) => i18n.t('validation.maxLength', { field, max }),
  minValue: (field: string, min: number) => i18n.t('validation.minValue', { field, min }),
  maxValue: (field: string, max: number) => i18n.t('validation.maxValue', { field, max }),
  invalidNumber: () => i18n.t('validation.invalidNumber'),
  invalidDate: () => i18n.t('validation.invalidDate'),
};
