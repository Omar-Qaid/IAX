import type { ReactNode } from 'react';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { RelatedInformationSection } from '@shared/components/page/RelatedInformationPanel';

export type DetailValue = string | number | boolean;
export type DetailValues = Record<string, DetailValue>;

export interface DetailFieldOption { value: string; label: string }
export interface DetailFieldConfig {
  name: string;
  label: string;
  type?: 'text' | 'number' | 'boolean' | 'select' | 'display';
  options?: DetailFieldOption[];
  disabled?: boolean;
  width?: number | string;
  column?: number;
  row?: number;
}
export interface DetailFieldGroup { id: string; title?: string; fields: DetailFieldConfig[]; columns?: number }
export interface DetailSectionConfig { id: string; title: string; groups?: DetailFieldGroup[]; content?: ReactNode; link?: ReactNode; defaultExpanded?: boolean; columns?: number }

export interface ListDetailRecord { id: string }

export interface ListDetailsCommand {
  id: string;
  label: string;
  disabled?: boolean;
  onClick?: () => void;
}

export interface ListDetailsHeaderField<T> extends Omit<DetailFieldConfig, 'name'> {
  id: string;
  getValue: (record: T) => DetailValue;
  setValue: (record: T, value: DetailValue) => T;
}

export interface EnterpriseListDetailsConfig<T extends ListDetailRecord> {
  dataSource: ListDetailsDataSource<T>;
  createRecord: () => T;
  getPrimaryText: (record: T) => string;
  getSecondaryText?: (record: T) => string;
  matchesSearch?: (record: T, query: string) => boolean;
  getValues: (record: T) => DetailValues;
  setValues: (record: T, values: DetailValues) => T;
  headerFields: ListDetailsHeaderField<T>[];
  sections: DetailSectionConfig[];
  viewLabel?: string;
  filterLabel?: string;
  informationLabel?: string;
  yesLabel?: string;
  noLabel?: string;
  crud?: Partial<{ editLabel: string; newLabel: string; deleteLabel: string; saveLabel: string; cancelLabel: string }>;
  commands?: ListDetailsCommand[];
  utilities?: Partial<{ personalizeLabel: string; guideLabel: string; notificationsLabel: string; refreshLabel: string; openWindowLabel: string; notificationCount: number }>;
  presentation?: {
    mode: 'list' | 'grid';
    columns?: ColumnDef<T>[];
    storageKey?: string;
    listWidth?: number;
    headerContent?: ReactNode;
    headerMaxWidth?: number;
    masterRowHeight?: number;
    masterHeaderHeight?: number;
  };
  permissions?: { view?: string; create?: string; edit?: string; delete?: string };
  validate?: (record: T) => Record<string, string> | Promise<Record<string, string>>;
  validationTitle?: string;
  showInformation?: boolean;
  advancedFilter?: { title?: string; addLabel?: string; fieldLabel: string; operatorLabel?: string; applyLabel?: string; resetLabel?: string; matches: (record: T, value: string) => boolean; getValue?: (record: T) => unknown; fields?: Array<{ id: string; label: string; getValue: (record: T) => unknown }> };
  relatedInformation?: { title?: string; sections: (record: T | null) => RelatedInformationSection[] };
  advancedFilterOpenOnLoad?: boolean;
  informationOpenOnLoad?: boolean;
}

export type ListDetailsDataSource<T extends ListDetailRecord> =
  | { type: 'static'; records: T[] }
  | { type: 'controlled'; records: T[]; onRecordsChange: (records: T[]) => void; loading?: boolean; error?: string | null; refresh?: () => void | Promise<void> }
  | { type: 'remote'; key: string; load: (signal: AbortSignal) => Promise<T[]>; create: (record: T) => Promise<T>; update: (record: T) => Promise<T>; delete: (record: T) => Promise<void>; initialRecords?: T[] };

