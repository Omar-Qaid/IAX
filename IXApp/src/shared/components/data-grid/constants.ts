type ColumnKind = 'text' | 'number' | 'date' | 'boolean' | 'singleSelect';

/**
 * Filter operators offered per column type, mirroring the D365 F&O grid filter
 * (begins with / contains / is / is not / is one of / greater / less / blank).
 */
export const GRID_FILTER_OPERATORS: ReadonlyArray<{
  value: string;
  labelKey: string;
  types?: readonly ColumnKind[];
}> = [
  { value: 'contains', labelKey: 'grid.operators.contains', types: ['text', 'singleSelect'] },
  { value: 'doesNotContain', labelKey: 'grid.operators.does_not_contain', types: ['text', 'singleSelect'] },
  { value: 'equals', labelKey: 'grid.operators.equals' },
  { value: 'notEquals', labelKey: 'grid.operators.not_equals' },
  { value: 'startsWith', labelKey: 'grid.operators.starts_with', types: ['text', 'singleSelect'] },
  { value: 'endsWith', labelKey: 'grid.operators.ends_with', types: ['text', 'singleSelect'] },
  { value: 'gt', labelKey: 'grid.operators.greater_than', types: ['number', 'date'] },
  { value: 'lt', labelKey: 'grid.operators.less_than', types: ['number', 'date'] },
  { value: 'in', labelKey: 'grid.operators.is_one_of' },
  { value: 'isEmpty', labelKey: 'grid.operators.is_empty' },
  { value: 'isNotEmpty', labelKey: 'grid.operators.is_not_empty' },
];

/** Operators that take no value: they must survive an empty value on apply. */
export const VALUELESS_FILTER_OPERATORS: ReadonlySet<string> = new Set(['isEmpty', 'isNotEmpty']);

export function filterOperatorsFor(type: ColumnKind | undefined) {
  const kind = type ?? 'text';
  return GRID_FILTER_OPERATORS.filter((op) => !op.types || op.types.includes(kind));
}

/** The operator a quick filter-row entry uses: numbers, booleans and dates match exactly. */
export function defaultFilterOperator(type: ColumnKind | undefined): 'contains' | 'equals' | 'startsWith' {
  if (type === 'number' || type === 'boolean') return 'equals';
  if (type === 'date') return 'startsWith';
  return 'contains';
}

export const GRID_SELECTION_COLUMN_WIDTH = 40;
