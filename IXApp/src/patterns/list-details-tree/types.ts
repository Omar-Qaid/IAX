import type { ReactNode } from 'react';
import type { EnterpriseListDetailsConfig, ListDetailRecord } from '@patterns/list-details/types';

export interface ListDetailsTreeConfig<T extends ListDetailRecord> {
  getParentId: (record: T) => string | null;
  getLabel?: (record: T) => string;
  getSecondaryText?: (record: T) => string;
  compare?: (left: T, right: T) => number;
  renderIcon?: (record: T) => ReactNode;
  initiallyExpanded?: 'all' | string[];
  ariaLabel: string;
  emptyLabel?: string;
  expandLabel?: string;
  collapseLabel?: string;
  indent?: number;
  rowHeight?: number;
}

export type ListDetailsTreePageConfig<T extends ListDetailRecord> = Omit<
  EnterpriseListDetailsConfig<T>,
  'renderListPane'
> & {
  tree: ListDetailsTreeConfig<T>;
};
