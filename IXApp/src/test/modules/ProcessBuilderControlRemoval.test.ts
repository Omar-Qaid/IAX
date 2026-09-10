import { describe, expect, it } from 'vitest';
import { createProcessBuilderDocument, useProcessBuilderStore } from '@modules/process-builder/store/useProcessBuilderStore';
import type { BuilderControl } from '@modules/process-builder/types/processBuilderTypes';

describe('request control removal', () => {
  it('clears deleted visibility references while preserving unrelated settings', () => {
    const document = createProcessBuilderDocument('1');
    const control = (id: string) => ({ id, label: id, options: [], validations: [], visibilityCondition: null } as unknown as BuilderControl);
    const dependent = control('dependent');
    dependent.visibilityCondition = { variableId: 'removed', operator: '=', value: 'Yes' };
    dependent.optionFeatureConfigurations = [{ requireFileUpload: true, sendAlertMessage: true,
      alertMessage: 'Keep', performerIds: ['7'], showOtherControls: true,
      visibleControlIds: ['removed', 'retained'] }];
    const retained = control('retained');
    retained.visibilityCondition = { variableId: 'dependent', operator: '=', value: 'No' };
    document.requestControls = [control('removed'), dependent, retained];
    useProcessBuilderStore.getState().initialize(document);
    useProcessBuilderStore.getState().removeRequestControl('removed');
    const state = useProcessBuilderStore.getState();
    expect(state.document.requestControls.map((item) => item.id)).toEqual(['dependent', 'retained']);
    expect(state.document.requestControls[0].visibilityCondition).toBeNull();
    expect(state.document.requestControls[0].optionFeatureConfigurations?.[0]).toEqual({
      ...dependent.optionFeatureConfigurations[0], visibleControlIds: ['retained'],
    });
    expect(state.document.requestControls[1].visibilityCondition).toEqual(retained.visibilityCondition);
    expect(state.dirty).toBe(true);
  });
});
