import type React from 'react';

export type SortDirection = 'asc' | 'desc' | null;

export interface DataGridHandle {
  startAddRow: () => void;
  startEditRow: (id: string | number) => void;
  saveEdit: () => void;
  cancelEdit: () => void;
  toggleSidebar: (tab?: 'columns' | 'filters' | 'features') => void;
}

export interface ColumnDef<T> {
  field: keyof T | string;
  headerName: string;
  width?: number;
  minWidth?: number;
  maxWidth?: number;
  flex?: number;
  sortable?: boolean;
  filterable?: boolean;
  hidden?: boolean;
  pinned?: 'left' | 'right' | null;
  renderCell?: (params: { row: T; value: unknown; rowIndex: number }) => React.ReactNode;
  valueGetter?: (params: { row: T }) => any;
  align?: 'left' | 'center' | 'right';
  headerAlign?: 'left' | 'center' | 'right';
  type?: 'text' | 'number' | 'date' | 'boolean' | 'singleSelect';
  valueOptions?: any[];
  /** When masterForm=true, cells in this column render as inputs while a row is being edited. */
  editable?: boolean;
}

export interface SortModel {
  field: string;
  sort: SortDirection;
}

export interface FilterModel {
  field: string;
  operator: 'contains' | 'equals' | 'startsWith' | 'endsWith' | 'gt' | 'lt' | 'notEquals' | 'doesNotContain' | 'in' | 'matches';
  value: unknown;
}

export type SelectionMode = 'single' | 'multiple';

// ─── Unified fetch params ─────────────────────────────────────────────────────

export interface FetchRowsParams {
  sort: SortModel[];
  filters: FilterModel[];
  globalSearch: string;
  page: number;
  pageSize: number;
  isFirstPage: boolean;
  signal: AbortSignal;
  columns?: { field: string; headerName: string }[];
}

// ─── Component props ──────────────────────────────────────────────────────────

export interface DataGridProps<T> {
  rows: T[];
  columns: ColumnDef<T>[];
  getRowId?: (row: T) => string | number;
  loading?: boolean;
  selectionMode?: SelectionMode;
  checkboxSelection?: boolean;
  onSelectionChange?: (selection: any[]) => void;
  height?: number | string;
  onRowClick?: (row: T) => void;
  onRowDoubleClick?: (row: T) => void;
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
  onDeleteSelected?: () => void;
  onViewHistory?: (row: T) => void;
  onShowAllFields?: (row: T) => void;
  onBuild?: (row: T) => void;
  processRowUpdate?: (newRow: T, oldRow: T) => T | Promise<T>;
  
  // ── Keyboard Actions ──────────────────────────────────────────────────────────
  onRefresh?: () => void;
  onValidate?: () => void;
  onExecute?: () => void;
  onPrint?: () => void;
  onCloseForm?: () => void;

  // ── Server-side mode ────────────────────────────────────────────────────────
  serverSide?: boolean;
  onFetchRows?: (params: FetchRowsParams) => void;
  pageSize?: number;
  totalRowCount?: number;
  hasMore?: boolean;

  // ── Server-side export ──────────────────────────────────────────────────────
  onServerExport?: (state: {
    sort: SortModel[];
    filters: FilterModel[];
    globalSearch: string;
    columns: { field: string; headerName: string }[];
  }) => Promise<void> | void;

  // ── Server-side import ──────────────────────────────────────────────────────
  onServerImport?: (file: File) => Promise<void> | void;
  onDownloadTemplate?: () => Promise<void> | void;

  // ── Customization ────────────────────────────────────────────────────────────
  rowHeight?: number;
  headerHeight?: number;
  showColumnBorders?: boolean;
  showCellBorders?: boolean;
  storageKey?: string;

  // ── Master-form inline editing ────────────────────────────────────────────
  masterForm?: boolean;
  onRowSave?: (row: Partial<T>, isNew: boolean) => Promise<void> | void;
  onNewRow?: () => Partial<T>;
  hideAddRowButton?: boolean;
  onEditingChange?: (isEditing: boolean) => void;
  hideInlineEditActions?: boolean;
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
  hideToolbar?: boolean;
  selectedIds?: (string | number)[];
  hideSidebar?: boolean;
  hideFooter?: boolean;
}
