import { useEffect } from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function useUnsavedChanges(isDirty: boolean, message?: string) {
  const { t } = useAppTranslation();
  const resolvedMessage = message ?? t('messages.unsavedChanges');

  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (isDirty) {
        event.preventDefault();
        event.returnValue = resolvedMessage;
        return resolvedMessage;
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [isDirty, resolvedMessage]);
}
