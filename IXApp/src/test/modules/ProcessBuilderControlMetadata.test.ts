import { describe, expect, it } from 'vitest';
import { readControlMetadataForSave } from '@modules/process-builder/api/controlMetadata';

describe('control metadata save validation', () => {
  it('preserves valid nested metadata and accepts missing metadata', () => {
    expect(readControlMetadataForSave('{"custom":{"value":42}}', 'Metadata')).toEqual({ custom: { value: 42 } });
    expect(readControlMetadataForSave(null, 'Metadata')).toEqual({});
    expect(readControlMetadataForSave(' ', 'Metadata')).toEqual({});
  });
  it.each(['{broken', 'null', '[]', '42', '"text"', 'true'])(
    'rejects non-object or malformed JSON %s', (value) => {
      expect(() => readControlMetadataForSave(value, 'Decision: ExtendedProperties')).toThrow('Decision: ExtendedProperties');
    }
  );
});
