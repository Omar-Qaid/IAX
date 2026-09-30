import type { ReactNode } from 'react';

export interface TreeControlConfig<T> {
  getId: (node: T) => string;
  getLabel: (node: T) => ReactNode;
  getChildren: (node: T) => readonly T[];
  isBranch?: (node: T) => boolean;
  selectedId?: string | null;
  initialExpandedIds?: readonly string[];
  expandAllLabel?: string;
  collapseAllLabel?: string;
  emptyChildrenLabel?: ReactNode;
  maxHeight?: number | string;
  ariaLabel?: string;
}

export interface TreeControlProps<T> {
  nodes: readonly T[];
  config: TreeControlConfig<T>;
}
