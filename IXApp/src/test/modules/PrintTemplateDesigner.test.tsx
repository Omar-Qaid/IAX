import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen } from '@test/testUtils';
import { TemplateDesigner } from '@modules/workflow/print-templates/components/TemplateDesigner';
import { createEmptyPrintTemplateDocument } from '@modules/workflow/print-templates/types/printTemplate.types';

vi.mock('@modules/workflow/api/dynamicRequestFormApi', () => ({
  dynamicRequestFormApi: {
    getDefinition: vi.fn().mockResolvedValue({
      processId: 7,
      processName: 'Daily closing',
      processDescription: null,
      controls: [
        {
          requestControlId: 2101,
          controlId: 10,
          code: 'TOTAL_SALES',
          label: 'Total sales',
          labelAr: 'إجمالي المبيعات',
          controlType: 'number',
          sortOrder: 1,
          score: 0,
          required: false,
          readOnly: false,
          uniqueKey: false,
          usedAsCriteria: false,
          defaultValue: null,
          visibilityCondition: null,
          options: [],
          validations: [],
        },
      ],
    }),
  },
}));

function DesignerHarness(): React.ReactElement {
  const [document, setDocument] = React.useState(() => createEmptyPrintTemplateDocument('en'));
  return <TemplateDesigner processId={7} document={document} onChange={setDocument} />;
}

describe('TemplateDesigner', () => {
  it('adds typed elements to the active document region and selects them', async () => {
    const user = userEvent.setup();
    render(<DesignerHarness />);

    await user.click(screen.getByRole('button', { name: 'Text' }));

    expect(screen.getByTestId('template-element-text')).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: 'Text' })).toHaveValue('Text');
  });

  it('adds child elements to a selected structured container', async () => {
    const user = userEvent.setup();
    render(<DesignerHarness />);

    await user.click(screen.getByRole('button', { name: 'Section' }));
    await user.click(screen.getByRole('button', { name: 'Dynamic field' }));

    expect(screen.getByTestId('template-element-section')).toBeInTheDocument();
    expect(screen.getByTestId('template-element-field')).toBeInTheDocument();
    expect(screen.getByText('{{system.requestNumber}}')).toBeInTheDocument();
  });

  it('selects a process request control from a code-and-name grid lookup', async () => {
    const user = userEvent.setup();
    render(<DesignerHarness />);

    await user.click(screen.getByRole('button', { name: 'Section' }));
    await user.click(screen.getByRole('button', { name: 'Dynamic field' }));
    await user.click(screen.getByRole('combobox', { name: 'Data source' }));
    await user.click(screen.getByRole('option', { name: 'Request dynamic fields' }));
    await user.click(screen.getByRole('textbox', { name: 'Request control ID' }));

    expect(await screen.findByText('TOTAL_SALES')).toBeInTheDocument();
    await user.click(screen.getByText('Total sales'));

    expect(screen.getByRole('textbox', { name: 'Request control ID' })).toHaveValue(
      'TOTAL_SALES - Total sales'
    );

    await user.click(screen.getByRole('button', { name: 'Text' }));
    await user.click(screen.getByTestId('template-element-field'));
    expect(screen.getByRole('textbox', { name: 'Request control ID' })).toHaveValue(
      'TOTAL_SALES - Total sales'
    );
  });
});
