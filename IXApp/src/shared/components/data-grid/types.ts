import type React from 'react';

export type SortDirection = 'asc' | 'desc' | null;

export interface GridCellAddress {
  rowId: string | number;
  field: string;
}

export interface DataGridHandle {
  cancelPendingFocus: () => void;
  focusCell: (rowIndex: number, columnIndex: number) => Promise<void>;
  getCellAddress: (rowIndex: number, columnIndex: number) => GridCellAddress | undefined;
  focusRecordCell: (address: GridCellAddress) => Promise<void>;
  clearFilters: () => void;
  focusFilter: () => void;
  /** Scrolls the rows (desktop) or cards (mobile) back to the first record. */
  scrollToTop: () => void;
  /** Scrolls a loaded record into view; returns false when it is not loaded (yet). */
  scrollToRow: (id: string | number) => boolean;
  startAddRow: () => void;
  startEditRow: (id: string | number) => void;
  saveEdit: () => Promise<boolean>;
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
  /**
   * D365 record preview: hovering or focusing the cell shows a card with the
   * referenced master record (e.g. `{ entity: 'customer' }` on an account column).
   * The key defaults to the cell value.
   */
  preview?: { entity: string; getKey?: (row: T) => unknown };
  /** Makes a key cell link to the record's detail form. Return undefined for rows without a route. */
  getDetailHref?: (row: T) => string | undefined;
  valueGetter?: (params: { row: T }) => unknown;
  align?: 'left' | 'center' | 'right';
  headerAlign?: 'left' | 'center' | 'right';
  type?: 'text' | 'number' | 'date' | 'boolean' | 'singleSelect';
  valueOptions?: readonly (unknown | { value: unknown; label: React.ReactNode })[];
  /** When masterForm=true, cells in this column render as inputs while a row is being edited. */
  editable?: boolean;
  renderEditCell?: (params: {
    row: T;
    value: unknown;
    onChange: (value: unknown) => void;
    disabled: boolean;
  }) => React.ReactNode;
}

export interface SortModel {
  field: string;
  sort: SortDirection;
}

export interface FilterModel {
  field: string;
  operator:
    | 'contains'
    | 'equals'
    | 'startsWith'
    | 'endsWith'
    | 'gt'
    | 'lt'
    | 'notEquals'
    | 'doesNotContain'
    | 'in'
    | 'matches'
    | 'isEmpty'
    | 'isNotEmpty';
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
  /** Keep comparison matrices as scrollable tables on narrow viewports. Default: cards. */
  mobilePresentation?: 'cards' | 'table';
  rows: T[];
  columns: ColumnDef<T>[];
  getRowId?: (row: T) => string | number;
  loading?: boolean;
  selectionMode?: SelectionMode;
  checkboxSelection?: boolean;
  onSelectionChange?: (selection: (string | number)[]) => void;
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
  /** Disable master/detail links while the owning page is editing custom cells. */
  navigationDisabled?: boolean;
  /** Temporarily lock editors without changing the owning page's edit mode. */
  editDisabled?: boolean;
  onRowSave?: (row: Partial<T>, isNew: boolean) => Promise<void> | void;
  onNewRow?: () => Partial<T>;
  hideAddRowButton?: boolean;
  onEditingChange?: (isEditing: boolean) => void;
  hideInlineEditActions?: boolean;
  /**
   * Highlight of the row being edited: 'outline' (default, tinted row with a
   * border) or 'none' (cell-level editing look: only the edited cells show editors).
   */
  editRowHighlight?: 'outline' | 'none';
  /**
   * D365 "edit from view": editable cells show a pencil on hover while no row is
   * being edited. Grids with `masterForm` get it by default (the pencil edits that
   * row and focuses the cell). Pages that own edit mode pass this: the pencil calls
   * it (the page switches to edit mode) and, once `masterForm` is on, the grid edits
   * the row and focuses the clicked cell.
   */
  onCellEditRequest?: (row: T, field: string) => void;
  /** Hidden by default. Set false to display the per-column filter inputs. */
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
  hideToolbar?: boolean;
  selectedIds?: (string | number)[];
  hideSidebar?: boolean;
  /** Hide the side buttons while keeping panels accessible from column menus. */
  hideSidebarTabs?: boolean;
  hideFooter?: boolean;
}
