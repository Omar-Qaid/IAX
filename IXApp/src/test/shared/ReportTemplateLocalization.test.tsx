import React from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { ReportTemplateRenderer } from '@shared/components/report-viewer';
import type { PrintTemplateDocument } from '@shared/components/report-designer';

const data = {
  system: { requestNumber: 'REQ-1' },
  company: {},
  report: {},
  requestControls: {},
  repeating: {},
};

const documentFor = (direction: 'ltr' | 'rtl'): PrintTemplateDocument => ({
  schemaVersion: 1,
  language: direction === 'rtl' ? 'ar' : 'en',
  direction,
  page: {
    size: 'A4',
    orientation: 'portrait',
    margins: { top: 10, right: 10, bottom: 10, left: 10 },
  },
  header: [],
  sections: [
    { id: 'text', type: 'text', value: 'English text', valueAlias: 'النص العربي' },
    {
      id: 'field',
      type: 'field',
      label: 'Request number',
      labelAlias: 'رقم الطلب',
      binding: { sourceType: 'system', source: 'requestNumber' },
    },
  ],
  footer: [],
  missingFieldBehavior: 'empty',
});

describe('ReportTemplateRenderer localization', () => {
  it('uses textAlias when valueAlias is absent and prefers valueAlias when both exist', () => {
    const template = documentFor('rtl');
    template.sections = [
      { id: 'legacy', type: 'text', value: 'Primary', textAlias: 'Text alias' },
      { id: 'both', type: 'text', value: 'Primary', textAlias: 'Unused alias', valueAlias: 'Value alias' },
    ];
    render(<ReportTemplateRenderer template={template} data={data} company={{ name: '' }} />);
    expect(screen.getByText('Text alias')).toBeInTheDocument();
    expect(screen.getByText('Value alias')).toBeInTheDocument();
    expect(screen.queryByText('Unused alias')).not.toBeInTheDocument();
  });

  it('renders primary text and labels in LTR mode', () => {
    render(
      <ReportTemplateRenderer template={documentFor('ltr')} data={data} company={{ name: '' }} />
    );
    expect(screen.getByText('English text')).toBeInTheDocument();
    expect(screen.getByText('Request number')).toBeInTheDocument();
    expect(screen.queryByText('النص العربي')).not.toBeInTheDocument();
  });

  it('renders aliases in RTL mode', () => {
    render(
      <ReportTemplateRenderer template={documentFor('rtl')} data={data} company={{ name: '' }} />
    );
    expect(screen.getByText('النص العربي')).toBeInTheDocument();
    expect(screen.getByText('رقم الطلب')).toBeInTheDocument();
    expect(screen.queryByText('English text')).not.toBeInTheDocument();
  });
});
