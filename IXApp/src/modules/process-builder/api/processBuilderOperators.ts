import type { BuilderOperator } from '../types/processBuilderTypes';

interface OperatorMetadata {
  recId: number;
  code: string | null;
  name: string | null;
  isActive?: boolean;
}

const operators: Record<string, BuilderOperator> = {
  '=': '=', eq: '=', '!=': '!=', '<>': '!=', neq: '!=',
  '>': '>', gt: '>', '<': '<', lt: '<',
  '>=': '>=', gte: '>=', '<=': '<=', lte: '<=',
  contains: 'contains', isempty: 'isEmpty', between: 'between',
};

export function parseBuilderOperator(value: string): BuilderOperator {
  const operator = operators[value.trim().toLowerCase()];
  if (!operator) throw new Error(`Unsupported workflow operator '${value}'.`);
  return operator;
}

export function resolveBuilderOperator<T extends Pick<OperatorMetadata, 'code' | 'name'>>(metadata: T): BuilderOperator {
  // Codes carry semantics; names are only a compatibility source when no code exists.
  return parseBuilderOperator(metadata.code?.trim() || metadata.name || '');
}

export function isSupportedBuilderOperator(metadata: Pick<OperatorMetadata, 'code' | 'name'>): boolean {
  try { resolveBuilderOperator(metadata); return true; }
  catch { return false; }
}

export function resolveOperatorId(
  operator: BuilderOperator,
  catalog: OperatorMetadata[],
  currentId?: number
): number {
  const matches = catalog.filter((item) => {
    try { return resolveBuilderOperator(item) === operator; }
    catch { return false; }
  });
  const current = matches.find((item) => item.recId === currentId);
  if (current) return current.recId;
  const active = matches.filter((item) => item.isActive !== false);
  if (active.length !== 1)
    throw new Error(`Workflow operator '${operator}' requires exactly one supported active catalog entry.`);
  return active[0].recId;
}
