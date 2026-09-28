import { describe, expect, it } from 'vitest';
import { validationTypesForControl } from '@modules/process-builder/validationTypesForControl';
import { builderControlType, builderValidationType } from '@modules/process-builder/api/processBuilderApi';

describe('process builder validation types', () => {
  it('offers applicant and global uniqueness only for numeric controls', () => {
    expect(validationTypesForControl('digits')).toEqual(
      expect.arrayContaining(['uniquePerApplicant', 'uniqueGlobal'])
    );
    expect(validationTypesForControl('text')).not.toContain('uniquePerApplicant');
    expect(validationTypesForControl('text')).not.toContain('uniqueGlobal');
  });

  it('shows only canonical validation choices while legacy aliases remain loadable', () => {
    const textTypes = validationTypesForControl('text');
    expect(textTypes).toEqual(expect.arrayContaining(['exactLength', 'pattern', 'inputMask', 'compare', 'expression']));
    expect(textTypes).not.toEqual(expect.arrayContaining(['length', 'regex', 'mask', 'comparison', 'custom']));
    expect(new Set(textTypes).size).toBe(textTypes.length);
  });

  it('recognizes the stored number TextBox as a numeric control', () => {
    expect(builderControlType('number مربع رقمي TextBox')).toBe('digits');
  });

  it.each(['comparison', 'custom', 'customexpression', 'expression'])(
    'normalizes legacy %s validation to expression',
    (type) => expect(builderValidationType(type)).toBe('expression')
  );
});
