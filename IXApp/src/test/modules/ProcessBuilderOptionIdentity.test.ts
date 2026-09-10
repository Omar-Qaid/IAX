import { describe, expect, it } from 'vitest';
import { updateControlOptions, moveOptionMetadata } from '@modules/process-builder/optionIdentity';
import type { BuilderControl } from '@modules/process-builder/types/processBuilderTypes';
import { createProcessBuilderDocument, useProcessBuilderStore } from '@modules/process-builder/store/useProcessBuilderStore';

describe('option identity edits', () => {
  const control = { options: ['A', 'B'], optionIds: ['1', '2'], optionAliases: ['AA', 'BB'],
    optionScores: [10, 20], optionFeatureConfigurations: [{ alertMessage: 'first' }, { alertMessage: 'second' }],
  } as BuilderControl;
  it('keeps identities on rename, removal and addition', () => {
    expect(updateControlOptions(control, { options: ['New A', 'New B'] }).optionIds).toEqual(['1', '2']);
    expect(updateControlOptions(control, { options: ['B'] }).optionIds).toEqual(['2']);
    expect(updateControlOptions(control, { options: ['A', 'B', 'C'] }).optionIds).toEqual(['1', '2', null]);
  });
  it('moves IDs, aliases, scores and features together', () => {
    expect(moveOptionMetadata(control, 0, 1)).toMatchObject({ optionIds: ['2', '1'],
      optionAliases: ['BB', 'AA'], optionScores: [20, 10],
      optionFeatureConfigurations: [{ alertMessage: 'second' }, { alertMessage: 'first' }],
    });
  });
  it('retains separate metadata for multiple unsaved options during rename and deletion', () => {
    const unsaved = { ...control, optionIds: [null, null] };
    const renamed = updateControlOptions(unsaved, { options: ['New A', 'B'] });
    expect(renamed).toMatchObject({ optionIds: [null, null], optionAliases: ['AA', 'BB'],
      optionScores: [10, 20], optionFeatureConfigurations: control.optionFeatureConfigurations });
    const removed = updateControlOptions(renamed, { options: ['B'] });
    expect(removed).toMatchObject({ optionIds: [null], optionAliases: ['BB'], optionScores: [20],
      optionFeatureConfigurations: [{ alertMessage: 'second' }] });
  });
  it('retains existing unsaved metadata when another option is added', () => {
    const added = updateControlOptions({ ...control, optionIds: ['1', null] }, { options: ['A', 'B', 'C'] });
    expect(added.optionAliases).toEqual(['AA', 'BB', '']);
    expect(added.optionScores).toEqual([10, 20, 0]);
    expect(added.optionFeatureConfigurations?.[1]).toEqual({ alertMessage: 'second' });
    expect(added.optionIds).toEqual(['1', null, null]);
  });
  it('applies identities through the request-control store editor', () => {
    const document = createProcessBuilderDocument('1');
    document.requestControls = [{ ...control, id: '10' }];
    useProcessBuilderStore.getState().initialize(document);
    useProcessBuilderStore.getState().updateRequestControl('10', { options: ['Renamed A', 'Renamed B'] });
    expect(useProcessBuilderStore.getState().document.requestControls[0].optionIds).toEqual(['1', '2']);
    useProcessBuilderStore.getState().reorderRequestControlOptions('10', 0, 1);
    expect(useProcessBuilderStore.getState().document.requestControls[0].optionIds).toEqual(['2', '1']);
  });
});
