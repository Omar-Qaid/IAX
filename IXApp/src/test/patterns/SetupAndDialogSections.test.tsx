import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@test/testUtils';
import { SetupPage } from '@patterns/setup/SetupPage';
import { FastTabsDialog } from '@patterns/dialog-fast-tabs/FastTabsDialog';

describe('Setup and dialog shared sections', () => {
  it('preserves setup anchors, independent expansion and saved field values', async () => {
    const save = vi.fn();
    render(
      <SetupPage
        title="Parameters"
        viewLabel="Standard"
        optionsLabel="Options"
        saveLabel="Save settings"
        yesLabel="Yes"
        noLabel="No"
        navigationItems={[{ id: 'general', label: 'Go to general' }]}
        initialValues={{ name: 'Original' }}
        onSave={save}
        sections={[
          {
            id: 'general',
            title: 'General settings',
            fields: [{ name: 'name', label: 'Setting name', type: 'text' }],
          },
          { id: 'advanced', title: 'Advanced settings', defaultExpanded: false, fields: [] },
        ]}
      />
    );
    const anchor = document.getElementById('setup-section-general')!;
    const scroll = vi.fn();
    anchor.scrollIntoView = scroll;
    fireEvent.click(screen.getByRole('button', { name: 'Go to general' }));
    expect(scroll).toHaveBeenCalledWith({ behavior: 'smooth', block: 'start' });
    fireEvent.change(screen.getByRole('textbox', { name: 'Setting name' }), {
      target: { value: 'Updated' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'General settings' }));
    expect(screen.getByRole('button', { name: 'Advanced settings' })).toHaveAttribute(
      'aria-expanded',
      'false'
    );
    fireEvent.click(screen.getByRole('button', { name: 'General settings' }));
    expect(screen.getByRole('textbox', { name: 'Setting name' })).toHaveValue('Updated');
    fireEvent.click(screen.getByRole('button', { name: 'Save settings' }));
    await waitFor(() => expect(save).toHaveBeenCalledWith({ name: 'Updated' }));
  });

  it('preserves dialog validation, summaries and submit mode when sections toggle', async () => {
    const submit = vi.fn();
    render(
      <FastTabsDialog
        open
        title="Create record"
        viewLabel="Standard"
        saveLabel="Create"
        cancelLabel="Cancel"
        onCancel={() => {}}
        initialValues={() => ({ name: '' })}
        onSubmit={submit}
        validate={(values): Record<string, string> =>
          values.name ? {} : { name: 'Name required' }
        }
        sections={[
          {
            id: 'general',
            title: 'Record fields',
            summary: 'Account summary',
            fields: [{ name: 'name', label: 'Record name', required: true }],
          },
        ]}
      />
    );
    fireEvent.click(screen.getByRole('button', { name: 'Create' }));
    await screen.findByText('Name required');
    expect(submit).not.toHaveBeenCalled();
    fireEvent.change(screen.getByRole('textbox', { name: 'Record name' }), {
      target: { value: 'Example' },
    });
    const header = screen.getByRole('button', { name: /Record fields/ });
    fireEvent.click(header);
    expect(screen.getByText('Account summary')).toBeVisible();
    fireEvent.click(header);
    fireEvent.click(screen.getByRole('button', { name: 'Create' }));
    await waitFor(() => expect(submit).toHaveBeenCalledWith({ name: 'Example' }, 'save'));
  });
});
