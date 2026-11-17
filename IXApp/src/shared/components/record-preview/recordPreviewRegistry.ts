import type { RecordPreviewDefinition } from './types';

const definitions = new Map<string, RecordPreviewDefinition<never>>();
const entitiesByMasterRoute = new Map<string, string>();

/** Registers (or replaces) the preview for a master-data entity. */
export function registerRecordPreview<TRecord>(
  entity: string,
  definition: RecordPreviewDefinition<TRecord>
): void {
  definitions.set(entity, definition as unknown as RecordPreviewDefinition<never>);
  if (definition.masterRoute) entitiesByMasterRoute.set(definition.masterRoute, entity);
}

export function getRecordPreview(entity: string): RecordPreviewDefinition | undefined {
  return definitions.get(entity) as RecordPreviewDefinition | undefined;
}

/** The entity registered for a master form route, if any. */
export function getRecordPreviewEntityByRoute(masterRoute: string | undefined): string | undefined {
  return masterRoute ? entitiesByMasterRoute.get(masterRoute) : undefined;
}

/** Resolves an explicit entity first, then the field's master route. */
export function resolveRecordPreviewEntity(
  entity: string | undefined,
  masterRoute: string | undefined
): string | undefined {
  const resolved = entity ?? getRecordPreviewEntityByRoute(masterRoute);
  return resolved && definitions.has(resolved) ? resolved : undefined;
}
