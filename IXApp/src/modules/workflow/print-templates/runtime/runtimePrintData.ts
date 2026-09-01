import type { UserProfile } from '@core/auth/types';
import type { PrintoutCompany } from '@shared/components/printout/PrintoutDocument';
import type { MailRequestDetailsDto, WfRequestRecord } from '../../api/wfRequestApi';
import type { PrintFieldBinding } from '../types/printTemplate.types';

export interface RuntimePrintData {
  system: Record<string, unknown>;
  company: Record<string, unknown>;
  report: Record<string, unknown>;
  requestControls: Record<string, unknown>;
  repeating: Record<string, unknown>;
}

interface LocationControlValue {
  address?: unknown;
  latitude?: unknown;
  longitude?: unknown;
}

export const formatRequestControlValue = (value: string, controlType: string): string => {
  const normalizedType = controlType.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  if (normalizedType !== 'location' || !value.trim()) return value;

  try {
    const parsed = JSON.parse(value) as unknown;
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) return value;

    const location = parsed as LocationControlValue;
    const address = typeof location.address === 'string' ? location.address.trim() : '';
    if (address) return address;

    const hasCoordinates =
      (typeof location.latitude === 'number' || typeof location.latitude === 'string') &&
      (typeof location.longitude === 'number' || typeof location.longitude === 'string');
    return hasCoordinates ? `${location.latitude}, ${location.longitude}` : value;
  } catch {
    // Preserve plain-text and malformed legacy location values instead of hiding them.
    return value;
  }
};

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

export const createRuntimePrintData = (
  request: WfRequestRecord,
  details: MailRequestDetailsDto,
  company: PrintoutCompany,
  user: UserProfile | null,
  printedAt: Date,
  language: 'en' | 'ar' = 'en'
): RuntimePrintData => {
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
        value: formatRequestControlValue(field.valueEn || field.value, field.controlType),
        valueAr: formatRequestControlValue(field.valueAr || field.value, field.controlType),
        controlId: field.controlId,
        requestControlId: field.controlDataId,
      })),
    },
  };
};

export const resolveRuntimeBinding = (
  data: RuntimePrintData,
  binding: PrintFieldBinding
): unknown => {
  if (binding.sourceType === 'requestControl') {
    const id = binding.requestControlId ?? binding.controlId;
    return id == null ? undefined : data.requestControls[String(id)];
  }
  if (binding.sourceType === 'system')
    return binding.source ? data.system[binding.source] : undefined;
  if (binding.sourceType === 'company')
    return binding.source ? data.company[binding.source] : undefined;
  if (binding.sourceType === 'report')
    return binding.source ? data.report[binding.source] : undefined;
  if (binding.sourceType === 'repeating')
    return binding.source ? data.repeating[binding.source] : undefined;
  return undefined;
};
