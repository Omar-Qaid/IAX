import { describe, expect, it } from 'vitest';
import type { BuilderControl } from '@modules/process-builder/types/processBuilderTypes';
import { validateVisibilityCycles } from '@modules/process-builder/api/visibilityGraph';

const field = (id: string, source?: string) => ({ id, label: id,
  visibilityCondition: source ? { variableId: source, operator: '=', value: 'Yes' } : null,
} as BuilderControl);

describe('request visibility cycles', () => {
  it('allows chains rooted in an unconditional field', () => {
    expect(() => validateVisibilityCycles([field('A'), field('B', 'A'), field('C', 'B')])).not.toThrow();
  });
  it('rejects self and indirect closed cycles', () => {
    expect(() => validateVisibilityCycles([field('A', 'A')])).toThrow('closed cycle');
    expect(() => validateVisibilityCycles([field('A', 'B'), field('B', 'A')])).toThrow('closed cycle');
  });
  it('does not reject a cycle that has an option-based entry point', () => {
    const root = field('Root');
    root.optionFeatureConfigurations = [{ showOtherControls: true, visibleControlIds: ['A'],
      requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [] }];
    expect(() => validateVisibilityCycles([root, field('A', 'B'), field('B', 'A')])).not.toThrow();
  });
});
