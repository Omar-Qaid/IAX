import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import userEvent from '@testing-library/user-event';
import { fireEvent, render, screen } from '@test/testUtils';
import { within } from '@testing-library/react';
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
    expect(
      within(screen.getByTestId('template-element-field')).getByText('Total sales')
    ).toBeDefined();

    await user.click(screen.getByRole('button', { name: 'Text' }));
    await user.click(screen.getByTestId('template-element-field'));
    expect(screen.getByRole('textbox', { name: 'Request control ID' })).toHaveValue(
      'TOTAL_SALES - Total sales'
    );
  });

  it('provides persistent image layout and appearance controls', async () => {
    const user = userEvent.setup();
    render(<DesignerHarness />);

    await user.click(screen.getByRole('button', { name: 'Image' }));
    fireEvent.change(screen.getByRole('textbox', { name: 'Alternative text' }), {
      target: { value: 'Company mark' },
    });
    const layoutSection = screen.getByRole('button', { name: 'Layout' });
    expect(layoutSection).toHaveAttribute('aria-expanded', 'false');
    await user.click(layoutSection);
    expect(layoutSection).toHaveAttribute('aria-expanded', 'true');
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Height (px)' }), {
      target: { value: '140' },
    });
    await user.click(screen.getByRole('combobox', { name: 'Image fit' }));
    await user.click(screen.getByRole('option', { name: 'Cover' }));
    await user.click(screen.getByRole('button', { name: 'Appearance' }));
    fireEvent.change(screen.getByRole('textbox', { name: 'Background color' }), {
      target: { value: '#eef2f6' },
    });
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Corner radius (px)' }), {
      target: { value: '8' },
    });

    await user.click(screen.getByRole('button', { name: 'Text' }));
    await user.click(screen.getByTestId('template-element-image'));

    expect(screen.getByRole('textbox', { name: 'Alternative text' })).toHaveValue('Company mark');
    expect(screen.getByRole('spinbutton', { name: 'Height (px)' })).toHaveValue(140);
    expect(screen.getByRole('combobox', { name: 'Image fit' })).toHaveTextContent('Cover');
    expect(screen.getByRole('textbox', { name: 'Background color' })).toHaveValue('#eef2f6');

    await user.click(layoutSection);
    expect(layoutSection).toHaveAttribute('aria-expanded', 'false');
  });

  it('offers the extended report component palette', () => {
    render(<DesignerHarness />);

    [
      'Rich text',
      'Label + value',
      'Company logo',
      'Barcode',
      'QR code',
      'Container',
      'Dynamic table',
      'Repeating section',
      'Key/value table',
      'Checkbox',
      'Signature',
      'Date/time',
      'Page number',
      'Page X of Y',
      'Page break',
    ].forEach((name) => expect(screen.getByRole('button', { name })).toBeInTheDocument());
  });

  it('selects report fields and configures numeric and date formats', async () => {
    const user = userEvent.setup();
    render(<DesignerHarness />);

    await user.click(screen.getByRole('button', { name: 'Dynamic field' }));
    await user.click(screen.getByRole('combobox', { name: 'Data source' }));
    await user.click(screen.getByRole('option', { name: 'Report fields' }));
    await user.click(screen.getByRole('combobox', { name: 'Field' }));
    await user.click(screen.getByRole('option', { name: 'Printed date' }));
    await user.click(screen.getByRole('button', { name: 'Format' }));

    expect(screen.getByRole('combobox', { name: 'Date format' })).toHaveTextContent('dd/MM/yyyy');

    await user.click(screen.getByRole('combobox', { name: 'Format' }));
    await user.click(screen.getByRole('option', { name: 'Currency' }));
    fireEvent.change(screen.getByRole('spinbutton', { name: 'Decimal places' }), {
      target: { value: '3' },
    });

    expect(screen.getByRole('spinbutton', { name: 'Decimal places' })).toHaveValue(3);
    expect(screen.getByRole('switch', { name: 'Thousand separator' })).toBeChecked();
    expect(screen.getByRole('textbox', { name: 'Currency' })).toHaveValue('SAR');
  });
});
