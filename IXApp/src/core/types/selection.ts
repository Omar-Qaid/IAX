export type SelectionMode = 'none' | 'single' | 'multiple';

export interface SelectionState<T = string> {
  selectedIds: T[];
  lastSelectedId?: T;
}
