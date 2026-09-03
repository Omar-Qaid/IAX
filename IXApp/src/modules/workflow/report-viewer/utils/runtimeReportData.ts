import type { UserProfile } from '@core/auth/types';
import type { reportCompany } from '@shared/components/report-viewer/ReportViewerDocument';
import type { MailRequestDetailsDto, WfRequestRecord } from '../../api/wfRequestApi';
import {
  formatRequestControlValue,
  type runtimeReportData,
} from '@shared/components/report-viewer';

const preferredFieldValue = (
  field: MailRequestDetailsDto['fields'][number],
  language: 'en' | 'ar'
): string => {
  const value =
    language === 'ar'
      ? field.valueAr || field.value || field.valueEn
      : field.valueEn || field.value || field.valueAr;
  return formatRequestControlValue(value, field.controlType);
};

export const createruntimeReportData = (
  request: WfRequestRecord,
  details: MailRequestDetailsDto,
  company: reportCompany,
  user: UserProfile | null,
  printedAt: Date,
  language: 'en' | 'ar' = 'en'
): runtimeReportData => {
  const controlTypeValues = Object.fromEntries(
    details.fields
      .filter((field) => field.controlId != null)
      .map((field) => [String(field.controlId), preferredFieldValue(field, language)])
  );
  const requestControlValues = Object.fromEntries(
    details.fields
      .filter((field) => field.controlDataId != null)
      .map((field) => [String(field.controlDataId), preferredFieldValue(field, language)])
  );

  return {
    system: {
      requestId: request.recId,
      requestNumber: request.code || request.recId,
      requestDate: details.requestDate || request.requestDate,
      requestStatus: details.status,
      status: details.status,
      processId: request.processId,
      processName: details.processName,
      processCode: details.processCode || '',
      createdBy: details.createdBy || '',
      createdDate: details.createdDate || request.requestDate,
      submittedBy: details.submittedBy || details.employeeName,
      submissionDate: details.submissionDate || details.requestDate || request.requestDate,
      employeeName: details.employeeName,
      employeeNumber: details.employeeNumber,
      responsibleEmployee: details.responsibleEmployee,
      transactionType: details.transactionType,
      transactionTime: details.transactionTime,
      transactionEndTime: details.transactionEndTime,
      currentUser: user?.displayName || user?.username || '',
      printDate: printedAt.toISOString(),
    },
    company: {
      name: company.name,
      arabicName: company.secondaryName,
      secondaryName: company.secondaryName,
      code: company.companyCode,
      logo: company.logoSource,
      logoSource: company.logoSource,
      address: company.addressLines?.join('\n'),
      contact: company.contactLines?.join('\n'),
      registration: company.registrationLines?.join('\n'),
      vatNumber: company.vatNumber,
      commercialRegistration: company.commercialRegistration,
      phone: company.phone,
      email: company.email,
    },
    report: {
      pageNumber: 1,
      totalPages: 1,
      pageNumberOfTotal: '1 / 1',
      currentDate: printedAt.toISOString(),
      currentTime: printedAt.toISOString(),
      printedDate: printedAt.toISOString(),
      printedBy: user?.displayName || user?.username || '',
    },
    // A stable WfRequestControl ID must win if it collides with a legacy control-type alias.
    requestControls: { ...controlTypeValues, ...requestControlValues },
    repeating: {
      items: details.fields.map((field) => ({
        key: field.label,
        label: field.label,
        labelAr: field.labelAr,
        value: preferredFieldValue(field, language),
        valueEn: formatRequestControlValue(field.valueEn || field.value, field.controlType),
        valueAr: formatRequestControlValue(field.valueAr || field.value, field.controlType),
        controlId: field.controlId,
        requestControlId: field.controlDataId,
      })),
    },
  };
};
