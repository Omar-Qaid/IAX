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

  it('offers the complete reference activity and control palettes', async () => {
    const user = userEvent.setup();
    render(<ProcessBuilderPage />);
    await user.click(screen.getByRole('tab', { name: 'Palette' }));
    expect(screen.getAllByRole('button', { name: /API Action/ }).length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: /Signature/ })).toHaveLength(1);
    expect(screen.getAllByRole('button', { name: /EmployeeSearch/ })).toHaveLength(1);
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
