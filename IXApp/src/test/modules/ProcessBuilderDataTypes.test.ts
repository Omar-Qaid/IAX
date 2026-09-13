import { describe, expect, it } from 'vitest';
import {
  resolveBuilderDataType,
  resolveVariableDataTypeId,
} from '@modules/process-builder/api/processBuilderDataTypes';

describe('Process Builder workflow metadata', () => {
  it.each([[1, 'STR', 'text'], [2, 'NUM', 'number'], [3, 'BOOL', 'boolean'], [4, 'DATE', 'date']] as const)(
    'round-trips API-seeded type %s (%s)',
    (recId, code, type) => {
      const seededCatalog = [{ recId, code, isActive: true }];
      expect(resolveBuilderDataType(recId, seededCatalog)).toBe(type);
      expect(resolveVariableDataTypeId(type, seededCatalog)).toBe(recId);
      expect(resolveVariableDataTypeId(type, seededCatalog, recId)).toBe(recId);
    }
  );

  const catalog = [
    { recId: 41, code: 'INT', isActive: true },
    { recId: 12, code: 'STR', isActive: true },
    { recId: 83, code: 'DT', isActive: true },
    { recId: 24, code: 'BOOL', isActive: true },
  ];

  it.each([[41, 'number'], [12, 'text'], [83, 'date'], [24, 'boolean']] as const)(
    'round-trips catalog ID %s by semantic code',
    (id, type) => {
      expect(resolveBuilderDataType(id, catalog)).toBe(type);
      expect(resolveVariableDataTypeId(type, catalog)).toBe(id);
    }
  );

  it('preserves an existing inactive type without assigning it to new variables', () => {
    const types = [{ recId: 90, code: 'STR', isActive: false }, ...catalog];
    expect(resolveVariableDataTypeId('text', types, 90)).toBe(90);
    expect(resolveVariableDataTypeId('text', types)).toBe(12);
  });

  it('rejects unknown metadata and unsupported object values instead of coercing them', () => {
    expect(() => resolveBuilderDataType(1, catalog)).toThrow('unsupported');
    expect(() => resolveBuilderDataType(1, [{ recId: 1, code: 'CUSTOM' }])).toThrow('unsupported');
    expect(() => resolveVariableDataTypeId('object', catalog)).toThrow('supported active');
  });

  it('rejects ambiguous new mappings but preserves an existing identity', () => {
    const types = [...catalog, { recId: 99, code: 'STR', isActive: true }];
    expect(() => resolveVariableDataTypeId('text', types)).toThrow('exactly one');
    expect(resolveVariableDataTypeId('text', types, 12)).toBe(12);
  });
});
