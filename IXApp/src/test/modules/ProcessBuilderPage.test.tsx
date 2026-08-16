import React from 'react';
import { beforeEach, describe, expect, it } from 'vitest';
import userEvent from '@testing-library/user-event';
import { act, render, screen, waitFor } from '@test/testUtils';
import { ProcessBuilderPage } from '@modules/process-builder/pages/ProcessBuilderPage';
import { useProcessBuilderStore } from '@modules/process-builder/store/useProcessBuilderStore';
import { createProcessBuilderDocument } from '@modules/process-builder/store/useProcessBuilderStore';
import { normalizeTransitionValue, TransitionValueField } from '@modules/process-builder/components/TransitionValueField';

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  useProcessBuilderStore.getState().initialize(createProcessBuilderDocument('test'));
});

describe('standalone ProcessBuilderPage', () => {
  it('uses the selected variable data type for transition comparison values', () => {
    expect(normalizeTransitionValue('not-a-number', 'number')).toBe('');
    expect(normalizeTransitionValue('42.5', 'number')).toBe('42.5');
    expect(normalizeTransitionValue('2026-08-16', 'date')).toBe('2026-08-16');
    expect(normalizeTransitionValue('16/08/2026', 'date')).toBe('');
    expect(normalizeTransitionValue('true', 'boolean')).toBe('true');
    expect(normalizeTransitionValue('yes', 'boolean')).toBe('');

    render(
      <TransitionValueField
        dataType="number"
        value=""
        onChange={() => undefined}
      />
    );
    expect(screen.getByRole('spinbutton', { name: 'Comparison value' })).toBeDefined();
  });

  it('supports the reference tree, palette, workspace, properties, and export workflow', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    expect(screen.getByRole('heading', { name: 'Process Builder' })).toBeDefined();
    expect(screen.getByRole('tab', { name: 'Designer' })).toBeDefined();
    expect(screen.getByRole('tab', { name: 'Transitions' })).toBeDefined();
    expect(
      Array.from(
        screen.getByRole('tablist', { name: 'Process Builder workspaces' })
          .querySelectorAll('[role="tab"]')
      )
        .map((tab) => tab.textContent)
    ).toEqual([
      'Designer',
      'Variables',
      'Request form',
      'Steps',
      'Activities',
      'Activity form',
      'Transitions',
      'Diagram',
    ]);
    await user.click(screen.getByRole('button', { name: 'Add variable' }));
    expect(useProcessBuilderStore.getState().document.variables).toHaveLength(1);
    await user.click(screen.getByRole('button', { name: 'Export' }));
    expect(screen.getByRole('dialog', { name: 'Export process' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Copy JSON' })).toBeDefined();
  });

  it('keeps builder editing independent in the local frontend store', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    const stepId = useProcessBuilderStore.getState().document.steps[0].id;
    store.addActivity(stepId, 'approval');
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    useProcessBuilderStore.getState().addActivityControl(stepId, activity.id, 'dropdown-manual');
    const control = useProcessBuilderStore.getState().document.steps[0].activities[0].controls[0];
    useProcessBuilderStore.getState().updateActivityControl(stepId, activity.id, control.id, {
      label: 'Decision',
      options: ['Approve', 'Reject'],
    });
    expect(useProcessBuilderStore.getState().document.steps[0].activities[0].controls[0]).toMatchObject({
      label: 'Decision',
      options: ['Approve', 'Reject'],
    });
  });

  it('offers the control palette without activity types', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('tab', { name: 'Palette' }));
    expect(screen.queryByText('ACTIVITY TYPES (click a step, then add)')).toBeNull();
    expect(screen.queryByRole('button', { name: /API Action/ })).toBeNull();
    expect(screen.getAllByRole('button', { name: /Signature/ })).toHaveLength(1);
    expect(screen.getAllByRole('button', { name: /EmployeeSearch/ })).toHaveLength(1);
    expect(screen.getByRole('button', { name: /Check Box List/ })).toBeDefined();
    expect(screen.getByRole('button', { name: /Radio Button List/ })).toBeDefined();
    expect(screen.getByRole('button', { name: /Fill From DataBase/ })).toBeDefined();
  });

  it('edits selectable request-control options directly in the request form', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);

    await user.click(screen.getByRole('tab', { name: 'Request form' }));
    await user.click(screen.getByRole('button', { name: 'Check Box List' }));

    expect(screen.getByText('Add at least one selectable option.')).toBeDefined();
    await user.click(screen.getByRole('button', { name: 'Add option to New field' }));
    expect(useProcessBuilderStore.getState().document.requestControls[0].options).toEqual(['Option 1']);
    expect(screen.getByRole('button', { name: 'Reorder option 1 for New field' })).toBeDefined();

    const optionInput = screen.getByRole('textbox', { name: 'Option 1 for New field' });
    await user.clear(optionInput);
    await user.type(optionInput, 'Finance');
    expect(useProcessBuilderStore.getState().document.requestControls[0].options).toEqual(['Finance']);

    await user.click(screen.getByRole('button', { name: 'Remove option 1 from New field' }));
    expect(useProcessBuilderStore.getState().document.requestControls[0].options).toEqual([]);
  });

  it('edits selectable activity-control options directly in the activity form', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('button', { name: 'Add Step' }));
    const step = useProcessBuilderStore.getState().document.steps[0];
    act(() => useProcessBuilderStore.getState().addActivity(step.id, 'approval'));
    await user.click(screen.getByRole('tab', { name: 'Activity form' }));
    await user.click(screen.getByRole('button', { name: 'Check Box List' }));

    expect(screen.getByText('Add at least one selectable option.')).toBeDefined();
    await user.click(screen.getByRole('button', { name: 'Add option to New field' }));
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    expect(activity.controls[0].options).toEqual(['Option 1']);

    const optionInput = screen.getByRole('textbox', { name: 'Option 1 for New field' });
    await user.clear(optionInput);
    await user.type(optionInput, 'Manager approval');
    expect(useProcessBuilderStore.getState().document.steps[0].activities[0].controls[0].options)
      .toEqual(['Manager approval']);

    await user.click(screen.getByRole('button', { name: 'Remove option 1 from New field' }));
    expect(useProcessBuilderStore.getState().document.steps[0].activities[0].controls[0].options)
      .toEqual([]);
  });

  it('reorders request-control options and preserves their values', () => {
    const store = useProcessBuilderStore.getState();
    store.addRequestControl('dropdown-manual');
    const controlId = useProcessBuilderStore.getState().document.requestControls[0].id;
    useProcessBuilderStore.getState().updateRequestControl(controlId, {
      options: ['First', 'Second', 'Third'],
    });

    useProcessBuilderStore.getState().reorderRequestControlOptions(controlId, 0, 2);

    expect(useProcessBuilderStore.getState().document.requestControls[0].options)
      .toEqual(['Second', 'Third', 'First']);
  });

  it('reorders steps without depending on workflow services', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    useProcessBuilderStore.getState().addStep();
    const [first, second] = useProcessBuilderStore.getState().document.steps;
    useProcessBuilderStore.getState().reorderSteps(first.id, second.id);
    expect(useProcessBuilderStore.getState().document.steps.map((step) => step.id)).toEqual([second.id, first.id]);
  });

  it('updates persisted ordering fields when activities and controls are reordered', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    const stepId = useProcessBuilderStore.getState().document.steps[0].id;
    store.addActivity(stepId);
    store.addActivity(stepId);
    const [firstActivity, secondActivity] = useProcessBuilderStore.getState().document.steps[0].activities;
    store.reorderActivities(stepId, firstActivity.id, secondActivity.id);
    const activities = useProcessBuilderStore.getState().document.steps[0].activities;
    expect(activities.map((activity) => [activity.id, activity.sortOrder])).toEqual([
      [secondActivity.id, 10],
      [firstActivity.id, 20],
    ]);

    const activityId = activities[0].id;
    store.addActivityControl(stepId, activityId);
    store.addActivityControl(stepId, activityId);
    const [firstControl, secondControl] = useProcessBuilderStore.getState().document.steps[0].activities[0].controls;
    store.reorderControls(stepId, activityId, firstControl.id, secondControl.id);
    expect(
      useProcessBuilderStore.getState().document.steps[0].activities[0].controls
        .map((control) => [control.id, control.sortOrder])
    ).toEqual([
      [secondControl.id, 10],
      [firstControl.id, 20],
    ]);
  });

  it('manages activity actions and keeps them in the local document', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    const stepId = useProcessBuilderStore.getState().document.steps[0].id;
    store.addActivity(stepId, 'approval');
    const activityId = useProcessBuilderStore.getState().document.steps[0].activities[0].id;
    useProcessBuilderStore.getState().addActivityAction(stepId, activityId, 'approve');
    const action = useProcessBuilderStore.getState().document.steps[0].activities[0].actions[0];
    useProcessBuilderStore.getState().updateActivityAction(stepId, activityId, action.id, { label: 'Approve request' });
    expect(useProcessBuilderStore.getState().document.steps[0].activities[0].actions[0].label).toBe('Approve request');
  });

  it('reorders variables with stable sort-order increments', () => {
    const store = useProcessBuilderStore.getState();
    store.addVariable();
    useProcessBuilderStore.getState().addVariable();
    const [first, second] = useProcessBuilderStore.getState().document.variables;
    useProcessBuilderStore.getState().reorderVariables(first.id, second.id);
    expect(useProcessBuilderStore.getState().document.variables.map((variable) => [variable.id, variable.sortOrder])).toEqual([[second.id, 10], [first.id, 20]]);
  });

  it('edits variable sort order from the Variable settings pane', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('button', { name: 'Add variable' }));
    const variable = useProcessBuilderStore.getState().document.variables[0];
    act(() => useProcessBuilderStore.getState().select({ kind: 'variable', id: variable.id }));

    const sortOrder = screen.getByRole('spinbutton', { name: 'Sort order' });
    await user.clear(sortOrder);
    await user.type(sortOrder, '25');

    expect(useProcessBuilderStore.getState().document.variables[0].sortOrder).toBe(25);
  });

  it('remaps request-control transition triggers after controls are persisted', () => {
    const store = useProcessBuilderStore.getState();
    store.addRequestControl('checkboxlist');
    const control = useProcessBuilderStore.getState().document.requestControls[0];
    useProcessBuilderStore.getState().addTransition({
      triggerSource: 'requestControl',
      triggerId: control.id,
    });
    const persistedControl = { ...control, id: '321' };

    useProcessBuilderStore.getState().setPersistedRequestControls(
      [persistedControl],
      { [control.id]: '321' }
    );

    expect(useProcessBuilderStore.getState().document.transitions[0]).toMatchObject({
      triggerSource: 'requestControl',
      triggerId: '321',
    });
    expect(useProcessBuilderStore.getState().dirty).toBe(true);
  });

  it('remaps variable references after variables receive server ids', () => {
    const store = useProcessBuilderStore.getState();
    store.addVariable();
    store.addStep();
    const variable = useProcessBuilderStore.getState().document.variables[0];
    useProcessBuilderStore.getState().addTransition();

    useProcessBuilderStore.getState().setPersistedVariables(
      [{ ...variable, id: '501' }],
      { [variable.id]: '501' }
    );

    expect(useProcessBuilderStore.getState().document.transitions[0].variableId).toBe('501');
  });

  it('remaps step references and preserves local activities after steps receive server ids', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    const step = useProcessBuilderStore.getState().document.steps[0];
    store.addActivity(step.id);
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    store.addTransition();
    const transition = useProcessBuilderStore.getState().document.transitions[0];
    store.updateTransition(transition.id, { sourceStepId: step.id, targetStepId: step.id });
    store.select({ kind: 'activity', stepId: step.id, id: activity.id });

    useProcessBuilderStore.getState().setPersistedSteps(
      [{ ...step, id: '551', code: 'STEP-000551', activities: [] }],
      { [step.id]: '551' }
    );

    const state = useProcessBuilderStore.getState();
    expect(state.document.steps[0]).toMatchObject({ id: '551', code: 'STEP-000551' });
    expect(state.document.steps[0].activities).toEqual([activity]);
    expect(state.document.transitions[0]).toMatchObject({ sourceStepId: '551', targetStepId: '551' });
    expect(state.selected).toEqual({ kind: 'activity', stepId: '551', id: activity.id });
  });

  it('preserves unrelated edits when activities are persisted', () => {
    const store = useProcessBuilderStore.getState();
    store.updateProcess({ description: 'Unsaved process description' });
    store.addVariable();
    store.addRequestControl();
    store.addStep();
    const local = useProcessBuilderStore.getState().document;
    const step = local.steps[0];
    store.addActivity(step.id);
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    store.addTransition({ triggerSource: 'activity', triggerId: activity.id });
    const current = useProcessBuilderStore.getState().document;
    const persisted = {
      ...current,
      description: '',
      variables: [],
      requestControls: [],
      transitions: [],
      steps: [{
        ...current.steps[0],
        name: 'Server step name',
        activities: [{ ...activity, id: '601', name: 'Persisted activity' }],
      }],
    };

    useProcessBuilderStore.getState().setPersistedActivities(
      persisted,
      { [activity.id]: '601' }
    );

    const document = useProcessBuilderStore.getState().document;
    expect(document.description).toBe('Unsaved process description');
    expect(document.variables).toHaveLength(1);
    expect(document.requestControls).toHaveLength(1);
    expect(document.steps[0].name).toBe(step.name);
    expect(document.steps[0].activities[0]).toMatchObject({ id: '601', name: 'Persisted activity' });
    expect(document.transitions[0].triggerId).toBe('601');
  });

  it('preserves other builder sections when transitions are persisted', () => {
    const store = useProcessBuilderStore.getState();
    store.updateProcess({ name: 'Unsaved process name' });
    store.addVariable();
    store.addStep();
    store.addTransition();
    const transition = useProcessBuilderStore.getState().document.transitions[0];

    useProcessBuilderStore.getState().setPersistedTransitions([
      { ...transition, id: '701', value: 'Approved' },
    ]);

    const document = useProcessBuilderStore.getState().document;
    expect(document.name).toBe('Unsaved process name');
    expect(document.variables).toHaveLength(1);
    expect(document.steps).toHaveLength(1);
    expect(document.transitions[0]).toMatchObject({ id: '701', value: 'Approved' });
  });

  it('clears invalid selections and transition references when items are removed', () => {
    const store = useProcessBuilderStore.getState();
    store.addVariable();
    store.addRequestControl();
    store.addStep();
    const initial = useProcessBuilderStore.getState().document;
    const variable = initial.variables[0];
    const requestControl = initial.requestControls[0];
    const step = initial.steps[0];
    store.addActivity(step.id);
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    store.addTransition({ triggerSource: 'activity', triggerId: activity.id });
    const transition = useProcessBuilderStore.getState().document.transitions[0];
    store.updateTransition(transition.id, { variableId: variable.id, targetStepId: step.id });
    store.select({ kind: 'activity', stepId: step.id, id: activity.id });

    store.removeActivity(step.id, activity.id);
    expect(useProcessBuilderStore.getState().selected).not.toEqual({
      kind: 'activity', stepId: step.id, id: activity.id,
    });
    expect(useProcessBuilderStore.getState().document.transitions[0]).toMatchObject({
      triggerSource: 'none', triggerId: '',
    });

    store.removeVariable(variable.id);
    expect(useProcessBuilderStore.getState().document.transitions[0].variableId).toBe('');

    store.updateTransition(transition.id, { triggerSource: 'requestControl', triggerId: requestControl.id });
    store.removeRequestControl(requestControl.id);
    expect(useProcessBuilderStore.getState().document.transitions[0]).toMatchObject({
      triggerSource: 'none', triggerId: '',
    });

    store.removeStep(step.id);
    expect(useProcessBuilderStore.getState().document.transitions[0].targetStepId).toBe('');
  });

  it('keeps the active workspace when applying a saved server document', () => {
    const store = useProcessBuilderStore.getState();
    store.setCenterTab(2);
    store.addStep();
    const persisted = {
      ...useProcessBuilderStore.getState().document,
      steps: useProcessBuilderStore.getState().document.steps.map((step, index) => ({
        ...step,
        id: String(index + 100),
        code: `STEP-${String(index + 1).padStart(6, '0')}`,
      })),
    };

    useProcessBuilderStore.getState().applyPersistedDocument(persisted);

    expect(useProcessBuilderStore.getState().centerTab).toBe(2);
    expect(useProcessBuilderStore.getState().dirty).toBe(false);
  });

  it('remembers the selected step when switching workspace tabs', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    useProcessBuilderStore.getState().addStep();
    const [, secondStep] = useProcessBuilderStore.getState().document.steps;
    useProcessBuilderStore.getState().addActivity(secondStep.id, 'approval');
    const secondStepActivity = useProcessBuilderStore.getState().document.steps[1].activities[0];

    useProcessBuilderStore.getState().select({ kind: 'step', id: secondStep.id });
    useProcessBuilderStore.getState().setCenterTab(2);
    useProcessBuilderStore.getState().setCenterTab(4);

    expect(useProcessBuilderStore.getState().selectedStepId).toBe(secondStep.id);
    expect(useProcessBuilderStore.getState().selected).toEqual({
      kind: 'activity',
      stepId: secondStep.id,
      id: secondStepActivity.id,
    });

    useProcessBuilderStore.getState().setCenterTab(5);
    expect(useProcessBuilderStore.getState().selected).toEqual({
      kind: 'activity',
      stepId: secondStep.id,
      id: secondStepActivity.id,
    });
  });

  it('restores the selected tab, step, and activity after reload', () => {
    const store = useProcessBuilderStore.getState();
    store.addStep();
    const step = useProcessBuilderStore.getState().document.steps[0];
    useProcessBuilderStore.getState().addActivity(step.id, 'review');
    const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
    useProcessBuilderStore.getState().select({ kind: 'activity', stepId: step.id, id: activity.id });
    useProcessBuilderStore.getState().setCenterTab(4);
    useProcessBuilderStore.getState().setLeftTab(1);
    const beforeReload = useProcessBuilderStore.getState();
    const navigation = {
      selected: beforeReload.selected,
      selectedStepId: beforeReload.selectedStepId,
      centerTab: beforeReload.centerTab,
      leftTab: beforeReload.leftTab,
    };
    const document = beforeReload.document;

    useProcessBuilderStore.getState().initialize(document);
    useProcessBuilderStore.getState().restoreNavigation(navigation);

    expect(useProcessBuilderStore.getState()).toMatchObject({
      centerTab: 4,
      leftTab: 1,
      selectedStepId: step.id,
      selected: { kind: 'activity', stepId: step.id, id: activity.id },
    });
  });

  it('shows an activity selector in Activity Form', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('button', { name: 'Add Step' }));
    const step = useProcessBuilderStore.getState().document.steps[0];
    useProcessBuilderStore.getState().addActivity(step.id, 'approval');

    await user.click(screen.getByRole('tab', { name: 'Activity form' }));

    expect(screen.getByRole('combobox', { name: 'Activity' })).toBeDefined();
  });

  it('shows settings for the selected workspace tab', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);

    await user.click(screen.getByRole('tab', { name: 'Variables' }));

    expect(useProcessBuilderStore.getState().selected).toEqual({ kind: 'workspace', tab: 1 });
    expect(screen.getByText('Variables Settings')).toBeDefined();
    expect(screen.getByText('Add or select a variable to edit its settings.')).toBeDefined();

    useProcessBuilderStore.getState().addStep();
    const step = useProcessBuilderStore.getState().document.steps[0];
    await user.click(screen.getByRole('tab', { name: 'Steps' }));

    expect(useProcessBuilderStore.getState().selected).toEqual({ kind: 'step', id: step.id });
    expect(screen.getByText('Step Settings')).toBeDefined();
  });

  it('uses configured lookups and persisted fields in Activity Settings', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('button', { name: 'Add Step' }));
    const step = useProcessBuilderStore.getState().document.steps[0];
    act(() => {
      useProcessBuilderStore.getState().addActivity(step.id, 'approval');
      const activity = useProcessBuilderStore.getState().document.steps[0].activities[0];
      useProcessBuilderStore.getState().select({ kind: 'activity', stepId: step.id, id: activity.id });
    });

    expect(screen.getByRole('heading', { name: 'Activity Settings' })).toBeDefined();
    expect(screen.getByRole('combobox', { name: /Activity Type/ })).toBeDefined();
    expect(screen.getByRole('combobox', { name: /Performer/ })).toBeDefined();
    expect(screen.getByRole('spinbutton', { name: 'Score' })).toBeDefined();
    expect(screen.queryByRole('combobox', { name: 'Assignment mode' })).toBeNull();
    const autoPassingHours = screen.getByRole('spinbutton', { name: 'Auto passing hours' });
    expect(autoPassingHours).toBeDisabled();
    await user.click(screen.getByRole('switch', { name: 'Auto pass enabled' }));
    expect(autoPassingHours).not.toBeDisabled();
  });

  it('provides responsive structure and settings drawer controls', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);

    await user.click(screen.getByRole('button', { name: 'Open process structure' }));
    expect(screen.getByRole('heading', { name: 'Process structure' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Close process structure' })).toBeDefined();

    await user.click(screen.getByRole('button', { name: 'Close process structure' }));
    await waitFor(() => expect(screen.queryByRole('heading', { name: 'Process structure' })).toBeNull());
    await user.click(screen.getByRole('button', { name: 'Open settings' }));
    expect(screen.getByRole('heading', { name: 'Settings' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Close settings' })).toBeDefined();
  });

  it('offers keyboard-accessible step ordering alternatives', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('button', { name: 'Add Step' }));
    await user.click(screen.getByRole('button', { name: 'Add Step' }));
    const [first, second] = useProcessBuilderStore.getState().document.steps;

    await user.click(screen.getByRole('button', { name: `Move ${first.name} down` }));
    expect(useProcessBuilderStore.getState().document.steps.map((step) => step.id)).toEqual([
      second.id,
      first.id,
    ]);
  });

  it('guides users when the activity form has no selected activity', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);

    await user.click(screen.getByRole('tab', { name: 'Activity form' }));
    expect(screen.getByRole('region', { name: 'Activity form empty state' })).toHaveTextContent(
      'Select an activity to design its form'
    );
    expect(screen.getByRole('button', { name: 'Open Activities' })).toBeDefined();
  });
});
