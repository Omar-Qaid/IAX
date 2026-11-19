import { createContext, useContext, type ReactNode } from 'react';
import type { ColumnDef } from './types';

/** Stable column definitions with independently updated, externally controlled cell editors. */
export function createCellRenderer<T>() {
  type Column = ColumnDef<T>;
  type Params = Parameters<NonNullable<Column['renderCell']>>[0];
  type RenderCell = (column: Column, params: Params) => ReactNode;
  const Context = createContext<RenderCell | null>(null);
  function Provider({ renderCell, children }: { renderCell: RenderCell; children: ReactNode }) {
    return <Context.Provider value={renderCell}>{children}</Context.Provider>;
  }
  function Cell({ column, params }: { column: Column; params: Params }) {
    const renderCell = useContext(Context);
    if (!renderCell) throw new Error('External grid cells require their renderer provider.');
    return renderCell(column, params);
  }
  return { Provider, Cell };
}
