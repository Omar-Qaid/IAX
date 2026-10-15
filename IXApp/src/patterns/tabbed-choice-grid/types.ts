import type { ReactNode } from 'react';
import type { ColumnDef, DataGridProps } from '@shared/components/data-grid/types';

export interface ChoiceGridChoice {
  id: string;
  label: string;
}
export interface ChoiceGridTab {
  id: string;
  label: string;
  choices: ChoiceGridChoice[];
}

export interface TabbedChoiceGridPageProps<T extends { id: string }> {
  title: string;
  viewLabel?: string;
  chooserLabel?: string;
  tabs: ChoiceGridTab[];
  rows: T[];
  columns: ColumnDef<T>[];
  loading?: boolean;
  error?: ReactNode;
  editing?: boolean;
  saving?: boolean;
  canEdit?: boolean;
  canCreate?: boolean;
  canDelete?: boolean;
  selectedRowIds?: (string | number)[];
  initialTabId?: string;
  initialChoiceId?: string;
  storageKey?: string;
  gridProps?: Partial<DataGridProps<T>>;
  onContextChange?: (tabId: string, choiceId: string) => void;
  onSelectionChange?: (ids: (string | number)[]) => void;
  onEdit?: () => void;
  onNew?: () => void;
  onDelete?: () => void;
  onSave?: () => void;
  onCancel?: () => void;
  onRefresh?: () => void;
}
