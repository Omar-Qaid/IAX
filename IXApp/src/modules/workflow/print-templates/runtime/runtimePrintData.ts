import type { UserProfile } from '@core/auth/types';
import type { PrintoutCompany } from '@shared/components/printout/PrintoutDocument';
import type { MailRequestDetailsDto, WfRequestRecord } from '../../api/wfRequestApi';
import type { PrintFieldBinding } from '../types/printTemplate.types';

export interface RuntimePrintData {
  system: Record<string, unknown>;
  company: Record<string, unknown>;
  requestControls: Record<string, unknown>;
}

export const createRuntimePrintData = (
  request: WfRequestRecord,
  details: MailRequestDetailsDto,
  company: PrintoutCompany,
  user: UserProfile | null,
  printedAt: Date,
  language: 'en' | 'ar' = 'en'
): RuntimePrintData => ({
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
  requestControls: Object.fromEntries(
    details.fields.flatMap((field) =>
      [field.controlDataId, field.controlId]
        .filter((id): id is number => id != null)
        .map((id) => [
          String(id),
          language === 'ar'
            ? field.valueAr || field.value || field.valueEn
            : field.valueEn || field.value || field.valueAr,
        ])
    )
  ),
});

export const resolveRuntimeBinding = (data: RuntimePrintData, binding: PrintFieldBinding): unknown => {
  if (binding.sourceType === 'requestControl') {
    const id = binding.requestControlId ?? binding.controlId;
    return id == null ? undefined : data.requestControls[String(id)];
  }
  if (binding.sourceType === 'system') return binding.source ? data.system[binding.source] : undefined;
  if (binding.sourceType === 'company') return binding.source ? data.company[binding.source] : undefined;
  return undefined;
};
