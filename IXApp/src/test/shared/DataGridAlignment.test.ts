import { describe, expect, it } from 'vitest';
import {
  cellAlign,
  headerAlign,
  inferNumericColumns,
} from '@shared/components/data-grid/columnAlign';
import type { ColumnDef } from '@shared/components/data-grid/types';

describe('shared grid alignment defaults', () => {
  it('aligns text left and numbers right, with matching headers', () => {
    expect(cellAlign({ type: 'number' })).toBe('right');
    expect(cellAlign({})).toBe('left');
    expect(headerAlign({ type: 'number' })).toBe('right');
    expect(headerAlign({})).toBe('left');
  });

  it('keeps numeric-looking identifiers as text and infers actual numbers', () => {
    const columns: ColumnDef<{ code: string; quantity: number | null }>[] = [
      { field: 'code', headerName: 'Code' },
      { field: 'quantity', headerName: 'Quantity' },
    ];
    const inferred = inferNumericColumns(columns, [
      { code: '00123', quantity: null },
      { code: '00456', quantity: 3 },
    ]);
    expect(cellAlign(inferred[0])).toBe('left');
    expect(cellAlign(inferred[1])).toBe('right');
    expect(columns[1].align).toBeUndefined();
  });
});
