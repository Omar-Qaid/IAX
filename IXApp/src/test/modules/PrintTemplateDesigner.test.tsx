import React from 'react';
import { describe, expect, it } from 'vitest';
import userEvent from '@testing-library/user-event';
import { render, screen } from '@test/testUtils';
import { TemplateDesigner } from '@modules/workflow/print-templates/components/TemplateDesigner';
import { createEmptyPrintTemplateDocument } from '@modules/workflow/print-templates/types/printTemplate.types';

function DesignerHarness(): React.ReactElement {
  const [document, setDocument] = React.useState(() => createEmptyPrintTemplateDocument('en'));
  return <TemplateDesigner document={document} onChange={setDocument} />;
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
});
