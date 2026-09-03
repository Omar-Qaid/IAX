import React from 'react';
import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { render } from '@test/testUtils';
import type { LegalEntity } from '@modules/organization/types/legalEntityTypes';
import type { MailRequestDetailsDto, WfRequestRecord } from '@modules/workflow/api/wfRequestApi';
import {
  WorkflowMailReportViewerBody,
  toReportCompany,
} from '@modules/workflow/report-viewer/components/WorkflowMailReportViewer';
import { ReportViewerDocument } from '@shared/components/report-viewer/ReportViewerDocument';
import i18n from '@core/localization/i18n';

const company: LegalEntity = {
  recId: 1,
  party: 1,
  dataArea: 'HBMC',
  name: 'AlHayat Company',
  arabicName: 'شركة الحياة',
  languageId: 'en',
  currencyCode: 'SAR',
  taxLicenseNum: 'TAX-123',
  federalTaxId: null,
  bankAccount: null,
  calendar: null,
  timeZone: null,
  memo: null,
  localizedRegion: null,
  logo: 'iVBOR-logo',
  reportLogo: 'iVBOR-report-logo',
  addresses: [
    {
      id: '1',
      location: 1,
      locationId: 'HQ',
      description: 'Head office',
      address: 'Riyadh',
      primary: true,
      street: 'King Road',
      city: 'Riyadh',
      state: '',
      zipCode: '12345',
      county: '',
      countryRegionId: 'SA',
      districtName: '',
      validFrom: null,
      validTo: null,
      roles: [],
    },
  ],
  contacts: [
    {
      id: '1',
      location: 1,
      locationId: 'HQ',
      description: 'Phone',
      type: 'Phone',
      number: '+966500000000',
      extension: '',
      primary: true,
      roles: [],
    },
  ],
};

const request: WfRequestRecord = {
  id: '42',
  recId: 42,
  code: 'REQ-42',
  name: 'Leave request',
  description: null,
  requestDate: '2026-08-25T08:00:00Z',
  processId: 7,
  employeeId: 9,
  requestDetails: null,
  isFinished: false,
  finishedDate: null,
  isStopped: false,
  stoppedDate: null,
  score: 0,
  progress: 50,
  notes: 'Manager review required',
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'HBMC',
};

const details: MailRequestDetailsDto = {
  requestId: 42,
  processName: 'Annual Leave',
  status: 'In progress',
  requestDate: '2026-08-25T08:00:00Z',
  employeeName: 'Omar Qaid',
  employeeNumber: 'E-9',
  transactionType: 'Review',
  transactionTime: '2026-08-25T08:00:00Z',
  transactionEndTime: null,
  responsibleEmployee: 'Line manager',
  history: [],
  fields: [
    {
      detailId: 2,
      controlId: 2,
      controlDataId: 2,
      label: 'Reason',
      labelAr: '',
      value: 'Family trip',
      valueAr: '',
      valueEn: 'Family trip',
      controlType: 'longtext',
      controlOrder: 2,
    },
    {
      detailId: 1,
      controlId: 1,
      controlDataId: 1,
      label: 'Days',
      labelAr: '',
      value: '5',
      valueAr: '',
      valueEn: '5',
      controlType: 'number',
      controlOrder: 1,
    },
  ],
};

describe('Workflow mail printout', () => {
  it('maps CompanyInfo into a reusable header model and prefers the report logo', () => {
    const mapped = toReportCompany(company, 'HBMC');
    expect(mapped.name).toBe('AlHayat Company');
    expect(mapped.logoSource).toBe('data:image/png;base64,iVBOR-report-logo');
    expect(mapped.addressLines).toContain('Riyadh');
    expect(mapped.contactLines?.[0]).toContain('+966500000000');
    expect(mapped.registrationLines).toContain('Tax: TAX-123');
  });

  it('uses the managed report-logo attachment ahead of legacy CompanyInfo images', () => {
    expect(toReportCompany(company, 'HBMC', 'blob:managed-report-logo').logoSource).toBe(
      'blob:managed-report-logo'
    );
  });

  it('renders company, request metadata, dynamic fields, and footer reference', () => {
    const printCompany = toReportCompany(company, 'HBMC');
    render(
      <ReportViewerDocument
        company={printCompany}
        title="Workflow Mail"
        reference="REQ-42"
        reportDate="2026-08-25T08:00:00Z"
        status="In progress"
        generatedBy="Omar"
        generatedAt="2026-08-25T20:52:00Z"
      >
        <WorkflowMailReportViewerBody request={request} details={details} />
      </ReportViewerDocument>
    );
    expect(screen.getByText('AlHayat Company')).toBeInTheDocument();
    expect(screen.getByText('Omar Qaid')).toBeInTheDocument();
    expect(screen.getByText('Family trip')).toBeInTheDocument();
    expect(screen.getAllByText('REQ-42').length).toBeGreaterThan(0);
    expect(screen.getAllByText('In progress').length).toBeGreaterThanOrEqual(2);
    expect(screen.getAllByText(/Omar/).length).toBeGreaterThan(0);
    expect(screen.getByText('Confidential / Internal Use Only')).toBeInTheDocument();
    expect(screen.getByText('Manager review required')).toBeInTheDocument();
  });

  it('falls back to the regular logo and a company-code-only header', () => {
    expect(toReportCompany({ ...company, reportLogo: null }, 'HBMC').logoSource).toContain(
      'iVBOR-logo'
    );
    expect(toReportCompany(undefined, 'DAT')).toMatchObject({ name: 'DAT', companyCode: 'DAT' });
  });

  it('can independently hide the header, body, and footer', () => {
    render(
      <ReportViewerDocument
        company={toReportCompany(company, 'HBMC')}
        title="Workflow Mail"
        showHeader={false}
        showBody={false}
        showFooter={false}
      >
        <span>Hidden body</span>
      </ReportViewerDocument>
    );
    expect(screen.queryByText('Workflow Mail')).not.toBeInTheDocument();
    expect(screen.queryByText('Hidden body')).not.toBeInTheDocument();
    expect(screen.queryByText('Workflow mail printout')).not.toBeInTheDocument();
  });

  it('supports report-specific header and footer overrides without replacing the page shell', () => {
    render(
      <ReportViewerDocument
        company={toReportCompany(company, 'HBMC')}
        title="Workflow Mail"
        header={<div>Custom report header</div>}
        footer={<div>Custom report footer</div>}
      >
        <span>Shared page content</span>
      </ReportViewerDocument>
    );
    expect(screen.getByText('Custom report header')).toBeInTheDocument();
    expect(screen.getByText('Shared page content')).toBeInTheDocument();
    expect(screen.getByText('Custom report footer')).toBeInTheDocument();
    expect(screen.queryByText('Confidential / Internal Use Only')).not.toBeInTheDocument();
  });

  it('renders the printed document and localized request data in RTL Arabic', async () => {
    await i18n.changeLanguage('ar');
    const arabicDetails: MailRequestDetailsDto = {
      ...details,
      fields: [{ ...details.fields[0]!, labelAr: 'السبب', valueAr: 'رحلة عائلية' }],
    };
    const { container } = render(
      <ReportViewerDocument
        company={toReportCompany(company, 'HBMC')}
        title={i18n.t('mail.print.title')}
        reference="REQ-42"
        reportDate={details.requestDate}
        status={details.status}
        pageSettings={{ direction: 'rtl' }}
      >
        <WorkflowMailReportViewerBody request={request} details={arabicDetails} />
      </ReportViewerDocument>
    );

    expect(container.querySelector('.printout-document')).toHaveAttribute('dir', 'rtl');
    expect(screen.getByText('معلومات الطلب')).toBeInTheDocument();
    expect(screen.getByText('بيانات الطلب')).toBeInTheDocument();
    expect(screen.getByText('السبب')).toBeInTheDocument();
    expect(screen.getByText('رحلة عائلية')).toBeInTheDocument();
    expect(screen.getByText('رقم التقرير:')).toBeInTheDocument();
    expect(screen.getByText('قيد التنفيذ')).toBeInTheDocument();
    expect(screen.getByText('شركة الحياة')).toBeInTheDocument();
    await i18n.changeLanguage('en');
  });
});
