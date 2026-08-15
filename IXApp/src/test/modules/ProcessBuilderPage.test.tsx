import React from 'react';
import { beforeEach, describe, expect, it } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen, waitFor } from '@test/testUtils';
import { ProcessBuilderPage } from '@modules/process-builder/pages/ProcessBuilderPage';
import { useProcessBuilderStore } from '@modules/process-builder/store/useProcessBuilderStore';
import { createProcessBuilderDocument } from '@modules/process-builder/store/useProcessBuilderStore';

beforeEach(() => {
  localStorage.clear();
  sessionStorage.clear();
  useProcessBuilderStore.getState().initialize(createProcessBuilderDocument('test'));
});

describe('standalone ProcessBuilderPage', () => {
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
