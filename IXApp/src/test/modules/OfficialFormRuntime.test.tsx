import React from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import {
  formatPrintValue,
  RuntimePrintTemplate,
} from '@modules/workflow/print-templates/runtime/RuntimePrintTemplate';
import {
  createRuntimePrintData,
  formatRequestControlValue,
  resolveRuntimeBinding,
} from '@modules/workflow/print-templates/runtime/runtimePrintData';
import {
  selectDefaultPublishedTemplate,
  selectPublishedTemplates,
} from '@modules/workflow/print-templates/runtime/publishedTemplateSelection';
import type {
  PrintTemplateDocument,
  PrintTemplateSummary,
} from '@modules/workflow/print-templates/types/printTemplate.types';
import type { MailRequestDetailsDto, WfRequestRecord } from '@modules/workflow/api/wfRequestApi';

const request: WfRequestRecord = {
  id: '42',
  recId: 42,
  code: 'REQ-42',
  name: 'Request',
  description: null,
  requestDate: '2026-08-25T08:00:00Z',
  processId: 7,
  employeeId: 9,
  requestDetails: null,
  isFinished: true,
  finishedDate: '2026-08-25T09:00:00Z',
  isStopped: false,
  stoppedDate: null,
  score: 0,
  progress: 100,
  notes: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'HBMC',
};

const details: MailRequestDetailsDto = {
  requestId: 42,
  processName: 'Daily closing',
  processCode: 'DAILY_CLOSE',
  createdBy: 'creator',
  createdDate: '2026-08-25T07:55:00Z',
  submittedBy: 'Employee',
  submissionDate: '2026-08-25T08:00:00Z',
  status: 'Completed',
  requestDate: '2026-08-25T08:00:00Z',
  employeeName: 'Employee',
  employeeNumber: 'D141',
  transactionType: 'Completed',
  transactionTime: '2026-08-25T08:00:00Z',
  transactionEndTime: '2026-08-25T09:00:00Z',
  responsibleEmployee: null,
  history: [],
  fields: [
    {
      detailId: 1,
      controlId: 10,
      controlDataId: 2101,
      label: 'Total',
      labelAr: '',
      value: '5239',
      valueAr: '',
      valueEn: '5239',
      controlType: 'number',
      controlOrder: 1,
    },
  ],
};

const runtimeData = createRuntimePrintData(
  request,
  details,
  {
    name: 'Company',
    secondaryName: 'الشركة',
    companyCode: 'HBMC',
    logoSource: 'data:image/png;base64,AA==',
    addressLines: ['Riyadh'],
    vatNumber: 'VAT-1',
    commercialRegistration: 'CR-1',
    phone: '011',
    email: 'mail@example.com',
  },
  { id: '1', username: 'omar', email: '', displayName: 'Omar', roles: [], permissions: [] },
  new Date('2026-08-27T10:00:00Z')
);

const summary = (overrides: Partial<PrintTemplateSummary>): PrintTemplateSummary => ({
  templateId: 1,
  processId: 7,
  processName: 'Process',
  code: 'FORM',
  name: 'Form',
  description: null,
  pageSize: 'A4',
  orientation: 'portrait',
  language: 'en',
  isDefault: false,
  status: 1,
  currentVersionId: 1,
  currentVersionNo: 1,
  latestVersionNo: 1,
  hasDraft: false,
  isActive: true,
  lastModifiedAt: null,
  ...overrides,
});

describe('official-form runtime', () => {
  it('filters unavailable templates and orders the default first', () => {
    const result = selectPublishedTemplates([
      summary({ templateId: 2, name: 'Beta' }),
      summary({ templateId: 3, name: 'Draft', status: 0 }),
      summary({ templateId: 4, name: 'Inactive', isActive: false }),
      summary({ templateId: 1, name: 'Alpha', isDefault: true }),
    ]);
    expect(result.map((template) => template.templateId)).toEqual([1, 2]);
  });

  it('selects only an active published default for the main Printout action', () => {
    expect(
      selectDefaultPublishedTemplate([
        summary({ templateId: 2, isDefault: false }),
        summary({ templateId: 1, isDefault: true }),
      ])?.templateId
    ).toBe(1);
    expect(selectDefaultPublishedTemplate([summary({ isDefault: false })])).toBeUndefined();
  });

  it('resolves all designer system/company fields and both request-control identifiers', () => {
    expect(runtimeData.system).toMatchObject({
      processCode: 'DAILY_CLOSE',
      createdBy: 'creator',
      submittedBy: 'Employee',
      currentUser: 'Omar',
    });
    expect(runtimeData.company).toMatchObject({
      vatNumber: 'VAT-1',
      commercialRegistration: 'CR-1',
      phone: '011',
      email: 'mail@example.com',
    });
    expect(
      resolveRuntimeBinding(runtimeData, { sourceType: 'requestControl', requestControlId: 2101 })
    ).toBe('5239');
    expect(
      resolveRuntimeBinding(runtimeData, { sourceType: 'requestControl', controlId: 10 })
    ).toBe('5239');
    expect(resolveRuntimeBinding(runtimeData, { sourceType: 'report', source: 'printedBy' })).toBe(
      'Omar'
    );
  });

  it('keeps the stable request-control value when a control-type ID has the same number', () => {
    const collisionData = createRuntimePrintData(
      request,
      {
        ...details,
        fields: [
          {
            ...details.fields[0],
            controlId: 3,
            controlDataId: 10,
            label: 'Employee number',
            value: '2323',
            valueAr: '2323',
            valueEn: '2323',
          },
          {
            ...details.fields[0],
            detailId: 2,
            controlId: 10,
            controlDataId: 20,
            label: 'Approvals',
            value: '[{"department":"HR"}]',
            valueAr: '[{"department":"HR"}]',
            valueEn: '[{"department":"HR"}]',
            controlType: 'table',
          },
        ],
      },
      { name: 'Company' },
      null,
      new Date('2026-08-27T10:00:00Z'),
      'ar'
    );

    expect(
      resolveRuntimeBinding(collisionData, {
        sourceType: 'requestControl',
        requestControlId: 10,
      })
    ).toBe('2323');
  });

  it('formats numeric and date fields using designer options', () => {
    expect(
      formatPrintValue(
        -1234.5,
        {
          type: 'number',
          decimalPlaces: 2,
          useGrouping: true,
          negativeFormat: 'parentheses',
        },
        'en'
      )
    ).toBe('(1,234.50)');
    expect(
      formatPrintValue('2026-08-27T10:00:00Z', { type: 'date', pattern: 'yyyy-MM-dd' }, 'en')
    ).toBe('2026-08-27');
  });

  it('prints a location control as an address instead of serialized JSON', () => {
    const serializedLocation = JSON.stringify({
      address: 'Al-Safa, Jeddah, Makkah Region, Saudi Arabia',
      latitude: 21.595884,
      longitude: 39.207973,
    });

    expect(formatRequestControlValue(serializedLocation, 'Location')).toBe(
      'Al-Safa, Jeddah, Makkah Region, Saudi Arabia'
    );
    expect(
      formatRequestControlValue(
        JSON.stringify({ latitude: 21.595884, longitude: 39.207973 }),
        'location'
      )
    ).toBe('21.595884, 39.207973');
    expect(formatRequestControlValue('Jeddah', 'location')).toBe('Jeddah');
  });

  it('renders report page fields with printable page counters', () => {
    const template: PrintTemplateDocument = {
      schemaVersion: 1,
      language: 'en',
      direction: 'ltr',
      page: {
        size: 'A4',
        orientation: 'portrait',
        margins: { top: 15, right: 15, bottom: 15, left: 15 },
      },
      missingFieldBehavior: 'empty',
      header: [],
      sections: [],
      footer: [
        {
          id: 'page-x-of-y',
          type: 'field',
          label: '',
          binding: { sourceType: 'report', source: 'pageNumberOfTotal' },
        },
      ],
    };

    const { container } = render(
      <RuntimePrintTemplate template={template} data={runtimeData} company={{ name: 'Company' }} />
    );

    expect(container.querySelector('.printout-page-number')).toBeInTheDocument();
    expect(container.querySelector('.printout-page-count')).toBeInTheDocument();
  });

  it('does not print controls added after the request snapshot was created', () => {
    const template: PrintTemplateDocument = {
      schemaVersion: 1,
      language: 'en',
      direction: 'ltr',
      page: {
        size: 'A4',
        orientation: 'portrait',
        margins: { top: 15, right: 15, bottom: 15, left: 15 },
      },
      missingFieldBehavior: 'na',
      header: [],
      sections: [
        {
          id: 'existing-blank',
          type: 'field',
          label: 'Existing blank control',
          binding: { sourceType: 'requestControl', requestControlId: 2101 },
        },
        {
          id: 'new-field',
          type: 'field',
          label: 'New field control',
          binding: { sourceType: 'requestControl', requestControlId: 9999 },
        },
        {
          id: 'new-table',
          type: 'table',
          dataSource: { sourceType: 'requestControl', requestControlId: 9998 },
          repeatHeader: false,
          columns: [{ id: 'value', label: 'New table column', field: 'value' }],
        },
        {
          id: 'new-qr',
          type: 'qrCode',
          binding: { sourceType: 'requestControl', requestControlId: 9997 },
        },
      ],
      footer: [],
    };
    const historicalData = {
      ...runtimeData,
      requestControls: { ...runtimeData.requestControls, '2101': '' },
    };

    render(
      <RuntimePrintTemplate
        template={template}
        data={historicalData}
        company={{ name: 'Company' }}
      />
    );

    expect(screen.getByText('Existing blank control')).toBeInTheDocument();
    expect(screen.getByText('N/A')).toBeInTheDocument();
    expect(screen.queryByText('New field control')).not.toBeInTheDocument();
    expect(screen.queryByText('New table column')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('qrCode')).not.toBeInTheDocument();
  });

  it('renders the published layout with conditions, formatting, RTL, and exact margins', () => {
    const template: PrintTemplateDocument = {
      schemaVersion: 1,
      language: 'ar',
      direction: 'rtl',
      page: {
        size: 'A4',
        orientation: 'landscape',
        margins: { top: 11, right: 12, bottom: 13, left: 14 },
      },
      missingFieldBehavior: 'na',
      header: [{ id: 'header', type: 'text', value: 'Official header' }],
      sections: [
        {
          id: 'section',
          type: 'section',
          title: 'Request',
          columns: 1,
          elements: [
            {
              id: 'amount',
              type: 'field',
              label: 'Total',
              binding: { sourceType: 'requestControl', requestControlId: 2101 },
              format: { type: 'number' },
            },
            {
              id: 'missing',
              type: 'field',
              label: 'Missing',
              binding: { sourceType: 'system', source: 'unknown' },
            },
            {
              id: 'hidden',
              type: 'text',
              value: 'Do not show',
              visibleWhen: {
                field: { sourceType: 'system', source: 'requestStatus' },
                operator: '=',
                value: 'Open',
              },
            },
            { id: 'divider', type: 'divider' },
            {
              id: 'row',
              type: 'row',
              elements: [
                {
                  id: 'column',
                  type: 'column',
                  span: 1,
                  elements: [{ id: 'date', type: 'printDate' }],
                },
              ],
            },
            { id: 'spacer', type: 'spacer', height: 8 },
            { id: 'break', type: 'pageBreak' },
          ],
        },
      ],
      footer: [{ id: 'footer', type: 'text', value: 'Official footer' }],
    };
    const { container } = render(
      <RuntimePrintTemplate template={template} data={runtimeData} company={{ name: 'Company' }} />
    );
    expect(screen.getByText('Official header')).toBeInTheDocument();
    expect(screen.getByText('Official footer')).toBeInTheDocument();
    expect(screen.getByText(/5.239/)).toBeInTheDocument();
    expect(screen.getByText('N/A')).toBeInTheDocument();
    expect(screen.queryByText('Do not show')).not.toBeInTheDocument();
    const document = container.querySelector('.printout-document');
    expect(document).toHaveAttribute('dir', 'rtl');
    expect(Number.parseFloat(getComputedStyle(document!).paddingTop)).toBeCloseTo(41.57, 1);
    expect(Number.parseFloat(getComputedStyle(document!).paddingLeft)).toBeCloseTo(52.91, 1);
  });

  it('applies advanced image layout and appearance styles to printed output', () => {
    const template: PrintTemplateDocument = {
      schemaVersion: 1,
      language: 'en',
      direction: 'ltr',
      page: {
        size: 'A4',
        orientation: 'portrait',
        margins: { top: 15, right: 15, bottom: 15, left: 15 },
      },
      missingFieldBehavior: 'empty',
      header: [],
      sections: [
        {
          id: 'logo',
          type: 'image',
          sourceType: 'companyLogo',
          altText: 'Styled logo',
          style: {
            width: 45,
            height: 120,
            alignment: 'end',
            objectFit: 'cover',
            padding: 6,
            marginBottom: 10,
            backgroundColor: '#eef2f6',
            borderWidth: 2,
            borderColor: '#174f82',
            borderRadius: 8,
          },
        },
      ],
      footer: [],
    };

    render(
      <RuntimePrintTemplate template={template} data={runtimeData} company={{ name: 'Company' }} />
    );

    const image = screen.getByRole('img', { name: 'Styled logo' });
    expect(image).toHaveStyle({
      width: '45%',
      height: '120px',
      objectFit: 'cover',
      padding: '6px',
      marginBottom: '10px',
      backgroundColor: '#eef2f6',
      borderWidth: '2px',
      borderColor: '#174f82',
      borderRadius: '8px',
    });
  });

  it('renders configured tables, QR codes, and barcodes as printable output', () => {
    const template: PrintTemplateDocument = {
      schemaVersion: 1,
      language: 'en',
      direction: 'ltr',
      page: {
        size: 'A4',
        orientation: 'portrait',
        margins: { top: 15, right: 15, bottom: 15, left: 15 },
      },
      missingFieldBehavior: 'empty',
      header: [],
      sections: [
        {
          id: 'table',
          type: 'table',
          dataSource: { sourceType: 'repeating', source: 'items' },
          repeatHeader: false,
          columns: [
            { id: 'label', label: 'Label', field: 'label', width: 35 },
            { id: 'value', label: 'Value', field: 'value', width: 65 },
          ],
        },
        {
          id: 'qr',
          type: 'qrCode',
          binding: { sourceType: 'system', source: 'requestNumber' },
        },
        {
          id: 'barcode',
          type: 'barcode',
          binding: { sourceType: 'system', source: 'requestNumber' },
          format: 'code128',
        },
      ],
      footer: [],
    };

    const { container } = render(
      <RuntimePrintTemplate template={template} data={runtimeData} company={{ name: 'Company' }} />
    );

    expect(screen.getByText('Total')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: 'qr code REQ-42' })).toBeInTheDocument();
    expect(screen.getByLabelText('barcode REQ-42')).toBeInTheDocument();
    expect(container.querySelector('thead')).toHaveStyle({ display: 'table-row-group' });
    expect(container.querySelectorAll('col')[0]).toHaveStyle({ width: '35%' });
  });
});
