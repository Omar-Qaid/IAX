import { describe, expect, it } from 'vitest';
import { validateRecordOwnership } from '@modules/process-builder/api/recordOwnership';

describe('Process Builder record ownership', () => {
  it('allows new keys and records belonging to the selected parent', () => {
    expect(() => validateRecordOwnership([{ id: 'new-field' }, { id: '42' }], [{ recId: 42 }], 'Controls')).not.toThrow();
  });

  it.each(['43', '0', '042'])('rejects unrecognized persisted identity %s', (id) => {
    expect(() => validateRecordOwnership([{ id }], [{ recId: 42 }], 'Controls')).toThrow('stale or belongs to another parent');
  });

  it('rejects duplicate local identities before generated IDs can collide', () => {
    expect(() => validateRecordOwnership([{ id: 'new-field' }, { id: 'new-field' }], [], 'Controls')).toThrow('duplicate record identity');
  });
});
