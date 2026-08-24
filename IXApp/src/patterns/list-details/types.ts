import type { ReactNode } from 'react';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { RelatedInformationSection } from '@shared/components/page/RelatedInformationPanel';

export type DetailValue = string | number | boolean;
export type DetailValues = Record<string, DetailValue>;

export interface DetailFieldOption {
  value: string;
  label: string;
}
export interface DetailFieldConfig {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'boolean' | 'select' | 'display';
  options?: DetailFieldOption[];
  disabled?: boolean;
  width?: number | string;
  column?: number | string;
  row?: number | string;
  multiline?: boolean;
  rows?: number;
  sectionTitle?: string;
  linkStyle?: boolean;
  render?: (context: {
    value: DetailValue | undefined;
    editing: boolean;
    disabled: boolean;
    onChange: (value: DetailValue) => void;
  }) => ReactNode;
  renderOwnLabel?: boolean;
}
export interface DetailFieldGroup {
  id: string;
  title?: string;
  fields: DetailFieldConfig[];
  columns?: number;
  column?: string | number;
  width?: number | string;
}
export interface DetailSectionConfig {
  id: string;
  title: string;
  groups?: DetailFieldGroup[];
  content?: ReactNode;
  link?: ReactNode;
  defaultExpanded?: boolean;
  columns?: number;
  gridTemplateColumns?: string;
  columnGap?: number | string;
  minHeight?: number;
  detailsPadding?: number | string;
  visualVariant?: 'default' | 'legalEntity';
  hideHeader?: boolean;
}

export interface ListDetailRecord {
  id: string;
}

export interface ListDetailsCommand<T extends ListDetailRecord = ListDetailRecord> {
  id: string;
  label: string;
  disabled?: boolean;
  requiresSelection?: boolean;
  onClick?: (record: T | null) => void;
}

export interface ListDetailsHeaderField<T> extends Omit<DetailFieldConfig, 'name'> {
  id: string;
  getValue: (record: T) => DetailValue;
  setValue: (record: T, value: DetailValue) => T;
}

export interface EnterpriseListDetailsConfig<T extends ListDetailRecord> {
  /** Shows records as a selectable reference list without record-level CRUD actions. */
  readOnly?: boolean;
  dataSource: ListDetailsDataSource<T>;
  createRecord: () => T;
  getPrimaryText: (record: T) => string;
  getSecondaryText?: (record: T) => string;
  /** Initial value shown in the list Filter input. End users can edit or clear it. */
  initialQuery?: string;
  matchesSearch?: (record: T, query: string) => boolean;
  getValues: (record: T) => DetailValues;
  setValues: (record: T, values: DetailValues) => T;
  headerFields: ListDetailsHeaderField<T>[];
  sections:
    | DetailSectionConfig[]
    | ((context: {
        record: T;
        editing: boolean;
        onRecordChange: (record: T) => void;
      }) => DetailSectionConfig[]);
  viewLabel?: string;
  filterLabel?: string;
  informationLabel?: string;
  yesLabel?: string;
  noLabel?: string;
  crud?: Partial<{
    editLabel: string;
    newLabel: string;
    deleteLabel: string;
    saveLabel: string;
    cancelLabel: string;
  }>;
  commands?: ListDetailsCommand<T>[];
  actionPaneAfterListContent?: ReactNode;
  actionPaneEndContent?: ReactNode;
  attachments?: { refTableId: number; getRefRecId?: (record: T) => number };
  showAttachmentAction?: boolean;
  utilities?: Partial<{
    personalizeLabel: string;
    guideLabel: string;
    notificationsLabel: string;
    refreshLabel: string;
    openWindowLabel: string;
    notificationCount: number;
  }>;
  presentation?: {
    mode: 'list' | 'grid';
    columns?: ColumnDef<T>[];
    storageKey?: string;
    listWidth?: number;
    listMinWidth?: number;
    listMaxWidth?: number;
    listResizable?: boolean;
    listWidthStorageKey?: string;
    headerContent?: ReactNode;
    headerMaxWidth?: number;
    masterRowHeight?: number;
    masterHeaderHeight?: number;
    fullscreenCanvas?: boolean;
    compactRecordHeader?: boolean;
    listInitiallyVisible?: boolean;
    recordListBatchSize?: number;
  };
  permissions?: { view?: string; create?: string; edit?: string; delete?: string };
  validate?: (record: T) => Record<string, string> | Promise<Record<string, string>>;
  validationTitle?: string;
  showInformation?: boolean;
  advancedFilter?: {
    title?: string;
    addLabel?: string;
    fieldLabel: string;
    operatorLabel?: string;
    applyLabel?: string;
    resetLabel?: string;
    matches: (record: T, value: string) => boolean;
    getValue?: (record: T) => unknown;
    fields?: Array<{ id: string; label: string; getValue: (record: T) => unknown }>;
  };
  relatedInformation?: {
    title?: string;
    sections: (record: T | null) => RelatedInformationSection[];
  };
  advancedFilterOpenOnLoad?: boolean;
  informationOpenOnLoad?: boolean;
  /** Runtime behavior (automatic/manual/blocked) is loaded from SysNumberSequences. */
  numberSequence?: {
    key: string;
    field: keyof T;
  };
}

export type ListDetailsDataSource<T extends ListDetailRecord> =
  | { type: 'static'; records: T[] }
  | {
      type: 'controlled';
      records: T[];
      onRecordsChange: (records: T[]) => void;
      loading?: boolean;
      error?: string | null;
      refresh?: () => void | Promise<void>;
    }
  | {
      type: 'remote';
      key: string;
      load: (signal: AbortSignal) => Promise<T[]>;
      /** A create operation may persist one record or a batch created from one editor action. */
      create: (record: T) => Promise<T | T[]>;
      update: (record: T) => Promise<T>;
      delete: (record: T) => Promise<void>;
      initialRecords?: T[];
    };
