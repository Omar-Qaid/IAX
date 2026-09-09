import { createContext, useContext, type ReactNode } from 'react';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { SalesOrderLineRecord } from '../api/salesOrderLinesApi';

type LineColumn = ColumnDef<SalesOrderLineRecord>;
type CellParams = Parameters<NonNullable<LineColumn['renderCell']>>[0];
type RenderCell = (column: LineColumn, params: CellParams) => ReactNode;

const CellRendererContext = createContext<RenderCell | null>(null);

/** Update editor contents without replacing column definitions and grid layout. */
export function SalesLineCellProvider({
  renderCell,
  children,
}: {
  renderCell: RenderCell;
  children: ReactNode;
}) {
  return <CellRendererContext.Provider value={renderCell}>{children}</CellRendererContext.Provider>;
}

export function SalesLineCell({ column, params }: { column: LineColumn; params: CellParams }) {
  const renderCell = useContext(CellRendererContext);
  if (!renderCell) throw new Error('Sales line cells require their editor provider.');
  return renderCell(column, params);
}
