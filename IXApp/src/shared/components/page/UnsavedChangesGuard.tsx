import React from 'react';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';

export interface UnsavedChangesGuardProps {
  isDirty: boolean;
  message?: string;
}

export const UnsavedChangesGuard: React.FC<UnsavedChangesGuardProps> = ({ isDirty, message }) => {
  useUnsavedChanges(isDirty, message);
  return null;
};
