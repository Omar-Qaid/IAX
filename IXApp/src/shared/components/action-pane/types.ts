import type { ReactNode } from 'react';
import type { PageMode } from '@shared/hooks/usePageMode';

export type ActionGroupType = 'New' | 'Maintain' | 'Process' | 'Inquiries' | 'Print' | 'Options';

export interface ActionDefinition {
  id: string;
  label: string;
  icon?: ReactNode;
  onClick?: () => void;
  group?: ActionGroupType;
  order?: number;
  hidden?: boolean;
  disabled?: boolean;
  loading?: boolean;
  permission?: string;
  requiresSelection?: boolean;
  allowedPageModes?: PageMode[];
  tooltip?: string;
  keyboardShortcut?: string;
}
