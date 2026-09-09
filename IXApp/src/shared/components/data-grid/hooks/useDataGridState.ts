import { useEffect, useRef, useState } from 'react';
import type { ColumnDef, SelectionMode } from '../types';
import type { GridInitialState } from './useGridPersistence';

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
  const [activeSidebarTab, setActiveSidebarTab] = useState<
    'columns' | 'filters' | 'features' | null
  >(null);
  const [focusedCell, setFocusedCell] = useState<{
    rowIndex: number;
    colIndex: number;
  } | null>(null);

  const prevInitialSelectionMode = useRef(initialSelectionMode);
  useEffect(() => {
    if (initialSelectionMode === prevInitialSelectionMode.current) return;
    prevInitialSelectionMode.current = initialSelectionMode;
    if (initialSelectionMode) setSelectionMode(initialSelectionMode);
  }, [initialSelectionMode]);

  const prevInitialStateColumns = useRef(initialState.columns);
  useEffect(() => {
    if (initialState.columns === prevInitialStateColumns.current) return;
    prevInitialStateColumns.current = initialState.columns;
    setColumns((current) => {
      const definitions = new Map(initialState.columns.map((column) => [String(column.field), column]));
      const next = current.flatMap((active) => {
        const column = definitions.get(String(active.field));
        if (!column) return [];
        definitions.delete(String(active.field));
        return [{ ...column, width: active.width, flex: active.flex, hidden: active.hidden, pinned: active.pinned }];
      });
      // Keep the user's order when editor callbacks, labels, or row values change.
      return [...next, ...definitions.values()];
    });
  }, [initialState.columns]);

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
