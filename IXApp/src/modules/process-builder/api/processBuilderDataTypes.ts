import type { BuilderDataType } from '../types/processBuilderTypes';

interface DataTypeMetadata {
  recId: number;
  code: string | null;
  isActive?: boolean;
}

const semanticTypes: Record<string, BuilderDataType> = {
  INT: 'number',
  STR: 'text',
  DT: 'date',
  BOOL: 'boolean',
};

export function resolveBuilderDataType(id: number, catalog: DataTypeMetadata[]): BuilderDataType {
  const metadata = catalog.find((item) => item.recId === id);
  const type = semanticTypes[metadata?.code?.trim().toUpperCase() ?? ''];
  if (!type) throw new Error(`Workflow data type ${id} has an unsupported semantic code.`);
  return type;
}

export function resolveVariableDataTypeId(
  type: BuilderDataType,
  catalog: DataTypeMetadata[],
  currentId?: number
): number {
  // Keep the existing identity, including an inactive type, when its meaning is unchanged.
  if (currentId != null && resolveBuilderDataType(currentId, catalog) === type) return currentId;
  const matches = catalog.filter(
    (item) => item.isActive !== false && semanticTypes[item.code?.trim().toUpperCase() ?? ''] === type
  );
  if (matches.length !== 1)
    throw new Error(`Workflow data type '${type}' requires exactly one supported active catalog entry.`);
  return matches[0].recId;
}
