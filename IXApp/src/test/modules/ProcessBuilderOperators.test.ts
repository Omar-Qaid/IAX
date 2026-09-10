import { describe, expect, it } from 'vitest';
import { parseBuilderOperator, resolveBuilderOperator, resolveOperatorId } from '@modules/process-builder/api/processBuilderOperators';

describe('Process Builder operator catalog', () => {
  const catalog = [
    { recId: 11, code: 'EQ', name: 'Localized equality' },
    { recId: 22, code: 'GT', name: 'Localized greater than' },
  ];
  it('uses semantic codes instead of display names', () => {
    expect(resolveBuilderOperator(catalog[1])).toBe('>');
  });
  it('resolves a changed operator instead of preserving the stale ID', () => {
    expect(resolveOperatorId('>', catalog, 11)).toBe(22);
    expect(resolveOperatorId('=', catalog, 11)).toBe(11);
  });
  it('does not interpret unknown codes as equality', () => {
    expect(() => resolveBuilderOperator({ recId: 1, code: 'CUSTOM', name: '=' })).toThrow('Unsupported');
    expect(() => resolveOperatorId('=', [{ recId: 1, code: 'CUSTOM', name: '=' }])).toThrow('supported active');
  });
  it.each([[' <> ', '!='], ['BETWEEN', 'between'], [' IsEmpty ', 'isEmpty']] as const)(
    'normalizes supported operator %s', (input, output) => expect(parseBuilderOperator(input)).toBe(output)
  );
  it('rejects ambiguous mappings and preserves existing inactive operators', () => {
    const duplicate = [...catalog, { recId: 33, code: 'EQ', name: '=' }];
    expect(() => resolveOperatorId('=', duplicate)).toThrow('exactly one');
    expect(resolveOperatorId('=', duplicate, 11)).toBe(11);
    expect(resolveOperatorId('=', [{ ...catalog[0], isActive: false }], 11)).toBe(11);
    expect(() => resolveOperatorId('=', [{ ...catalog[0], isActive: false }])).toThrow('active');
  });
});
