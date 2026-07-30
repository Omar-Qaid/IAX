import { useTranslation } from 'react-i18next';
import { SUPPORTED_LANGUAGES, type LanguageOption } from './languages';

export function useAppTranslation() {
  const { t, i18n } = useTranslation();

  const currentLanguageCode = (i18n.language || 'en').substring(0, 2) as 'en' | 'ar';
  const currentLanguage: LanguageOption =
    SUPPORTED_LANGUAGES.find((l) => l.code === currentLanguageCode) || SUPPORTED_LANGUAGES[0]!;

  const changeLanguage = (code: 'en' | 'ar') => {
    i18n.changeLanguage(code);
    document.dir = code === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = code;
  };

  return {
    t,
    i18n,
    currentLanguage,
    changeLanguage,
    isRtl: currentLanguage.dir === 'rtl',
  };
}
