import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import enTranslation from '../../../public/locales/en/translation.json';
import arTranslation from '../../../public/locales/ar/translation.json';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: enTranslation },
      ar: { translation: arTranslation },
    },
    fallbackLng: 'en',
    supportedLngs: ['en', 'ar'],
    load: 'languageOnly',
    returnNull: false,
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  });

const applyDocumentLanguage = (language: string) => {
  const code = language.substring(0, 2);
  document.documentElement.lang = code;
  document.documentElement.dir = code === 'ar' ? 'rtl' : 'ltr';
};

applyDocumentLanguage(i18n.resolvedLanguage ?? i18n.language ?? 'en');
i18n.on('languageChanged', applyDocumentLanguage);

export default i18n;
