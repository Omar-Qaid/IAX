import { useState } from 'react';
import type { ColumnDef, SelectionMode } from '../Types';
import type { GridInitialState } from './useGridPersistence';

function areColumnsStructurallyEqual<T>(cols1: ColumnDef<T>[], cols2: ColumnDef<T>[]): boolean {
  if (cols1.length !== cols2.length) return false;
  for (let i = 0; i < cols1.length; i++) {
    if (cols1[i].field !== cols2[i].field) return false;
    if (cols1[i].headerName !== cols2[i].headerName) return false;
  }
  return true;
}

interface UIStateOptions<T> {
  initialState: GridInitialState<T>;
  initialColumns: ColumnDef<T>[];
  initialSelectionMode?: SelectionMode;
  initialShowColumnBorders?: boolean;
  initialShowCellBorders?: boolean;
  rowHeight: number;
}

export function useDataGridState<T>(options: UIStateOptions<T>) {
  const {
    initialState,
    initialColumns,
    initialSelectionMode,
    initialShowColumnBorders,
    initialShowCellBorders,
    rowHeight,
  } = options;

  const [columns, setColumns] = useState<ColumnDef<T>[]>(initialState.columns || initialColumns);
  const [selectionMode, setSelectionMode] = useState<SelectionMode>(
    initialState.selectionMode ?? initialSelectionMode ?? 'single'
  );
  const [localRowHeight, setLocalRowHeight] = useState(initialState.rowHeight ?? rowHeight);
  const [showColumnBorders, setShowColumnBorders] = useState(
    initialState.showColumnBorders ?? initialShowColumnBorders ?? false
  );
  const [showCellBorders, setShowCellBorders] = useState(
    initialState.showCellBorders ?? initialShowCellBorders ?? true
  );
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [activeSidebarTab, setActiveSidebarTab] = useState<'columns' | 'filters' | 'features' | null>(null);
  
  const [focusedCell, setFocusedCell] = useState<{ rowIndex: number; colIndex: number } | null>(null);

  const [prevInitialSelectionMode, setPrevInitialSelectionMode] = useState(initialSelectionMode);
  if (initialSelectionMode !== prevInitialSelectionMode) {
    setPrevInitialSelectionMode(initialSelectionMode);
    if (initialSelectionMode) {
      setSelectionMode(initialSelectionMode);
    }
  }

  const [prevInitialStateColumns, setPrevInitialStateColumns] = useState(initialState.columns);
  if (initialState.columns !== prevInitialStateColumns) {
    setPrevInitialStateColumns(initialState.columns);
    const isEqual = areColumnsStructurallyEqual(initialState.columns, prevInitialStateColumns);
    if (!isEqual) {
      setColumns(initialState.columns);
    } else {
      setColumns(current => {
        const currentByField = new Map(current.map(c => [String(c.field), c]));
        return initialState.columns.map(col => {
          const active = currentByField.get(String(col.field));
          if (active) {
            return {
              ...col,
              width: active.width,
              flex: active.flex,
              hidden: active.hidden,
              pinned: active.pinned,
            };
          }
          return col;
        });
      });
    }
  }

  return {
    columns,
    setColumns,
    selectionMode,
    setSelectionMode,
    localRowHeight,
    setLocalRowHeight,
    showColumnBorders,
    setShowColumnBorders,
    showCellBorders,
    setShowCellBorders,
    isSidebarOpen,
    setIsSidebarOpen,
    activeSidebarTab,
    setActiveSidebarTab,
    focusedCell,
    setFocusedCell,
  };
}
