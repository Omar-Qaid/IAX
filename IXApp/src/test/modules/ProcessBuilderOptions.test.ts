import { describe, expect, it } from 'vitest';
import { matchStoredOptions } from '@modules/process-builder/api/processBuilderOptions';

describe('stored option matching', () => {
  const options = [{ recId: 1, name: 'Approve', value: 'APP' }, { recId: 2, name: 'Reject', value: 'REJ' }];
  it('keeps row identity and values on reorder and deletion', () => {
    expect(matchStoredOptions(['Reject', 'Approve'], options)).toEqual([options[1], options[0]]);
    expect(matchStoredOptions(['Reject'], options)).toEqual([options[1]]);
  });
  it('preserves the single renamed option and distinguishes additions', () => {
    expect(matchStoredOptions(['Accept', 'Reject'], options)).toEqual(options);
    expect(matchStoredOptions(['Approve', 'Reject', 'Return'], options)[2]).toBeUndefined();
  });
  it('rejects ambiguous edits instead of overwriting unrelated options', () => {
    expect(() => matchStoredOptions(['Accept', 'Decline'], options)).toThrow('ambiguous');
    expect(() => matchStoredOptions(['Accept'], options)).toThrow('ambiguous');
  });
  it('supports bulk renames and replacement using explicit identities', () => {
    expect(matchStoredOptions(['Accept', 'Decline'], options, ['1', '2'])).toEqual(options);
    expect(matchStoredOptions(['Return', 'Decline'], options, [null, '2'])).toEqual([undefined, options[1]]);
    expect(() => matchStoredOptions(['A'], options, ['99'])).toThrow('belongs');
    expect(() => matchStoredOptions(['A', 'B'], options, ['1', '1'])).toThrow('Duplicate');
  });
});
