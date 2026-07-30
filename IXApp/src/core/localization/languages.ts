export interface LanguageOption {
  code: 'en' | 'ar';
  label: string;
  nativeName: string;
  dir: 'ltr' | 'rtl';
}

export const SUPPORTED_LANGUAGES: LanguageOption[] = [
  { code: 'en', label: 'English', nativeName: 'English', dir: 'ltr' },
  { code: 'ar', label: 'Arabic', nativeName: 'العربية', dir: 'rtl' },
];

export const DEFAULT_LANGUAGE: LanguageOption = SUPPORTED_LANGUAGES[0]!;
