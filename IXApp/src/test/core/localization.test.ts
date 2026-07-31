import { describe, it, expect } from 'vitest';
import { SUPPORTED_LANGUAGES, DEFAULT_LANGUAGE } from '@core/localization/languages';

describe('localization', () => {
  it('defines English and Arabic as supported languages', () => {
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('en');
    expect(SUPPORTED_LANGUAGES.map((l) => l.code)).toContain('ar');
  });

  it('sets English as default language', () => {
    expect(DEFAULT_LANGUAGE.code).toBe('en');
  });
});
