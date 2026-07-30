import React, { useEffect } from 'react';

export interface UnsavedChangesGuardProps {
  isDirty: boolean;
  message?: string;
}

export const UnsavedChangesGuard: React.FC<UnsavedChangesGuardProps> = ({
  isDirty,
  message = 'You have unsaved changes. Are you sure you want to leave?',
}) => {
  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!isDirty) return;
      event.preventDefault();
      event.returnValue = message;
      return message;
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty, message]);

  return null;
};
