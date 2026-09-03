import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { documentApi } from '@shared/components/documents/documentApi';
import { recordTableId } from '@shared/components/documents/recordTableIds';
import type { reportCompany } from './ReportViewerDocument';

export const REPORT_COMPANY_TABLE_ID = recordTableId('Legal entities');
export const REPORT_COMPANY_LOGO_ATTACHMENT = 'Legal entity report logo';

export interface ReportCompanyAddress {
  address: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  countryRegionId: string;
  primary: boolean;
}

export interface ReportCompanyContact {
  type: string;
  number: string;
  extension: string;
  primary: boolean;
}

export interface ReportCompanyInfo {
  recId: number;
  dataArea: string;
  name: string;
  arabicName?: string | null;
  taxLicenseNum?: string | null;
  federalTaxId?: string | null;
  logo?: string | null;
  reportLogo?: string | null;
  addresses: ReportCompanyAddress[];
  contacts: ReportCompanyContact[];
}

const asImageSource = (value?: string | null): string | null => {
  const image = value?.trim();
  if (!image) return null;
  if (/^(data:|blob:|https?:\/\/)/i.test(image)) return image;
  if (image.startsWith('/9j/')) return `data:image/jpeg;base64,${image}`;
  if (image.startsWith('R0lGOD')) return `data:image/gif;base64,${image}`;
  if (image.startsWith('UklGR')) return `data:image/webp;base64,${image}`;
  return `data:image/png;base64,${image}`;
};

const primaryFirst = <T extends { primary: boolean }>(items: T[]): T[] =>
  [...items].sort((left, right) => Number(right.primary) - Number(left.primary));

export const toReportCompany = (entity: ReportCompanyInfo | undefined, companyCode: string, reportLogoSource?: string | null): reportCompany => {
  if (!entity) return { name: companyCode || 'Company', companyCode };
  const address = primaryFirst(entity.addresses)[0];
  const contacts = primaryFirst(entity.contacts).slice(0, 3);
  const addressLines = [address?.address, [address?.street, address?.city, address?.state, address?.zipCode].filter(Boolean).join(', '), address?.countryRegionId].filter((line): line is string => Boolean(line?.trim()));
  const contactLines = contacts.map((contact) => [contact.type, contact.number, contact.extension ? `Ext. ${contact.extension}` : ''].filter(Boolean).join(': '));
  const registrationLines = [entity.taxLicenseNum ? `Tax: ${entity.taxLicenseNum}` : '', entity.federalTaxId ? `Federal ID: ${entity.federalTaxId}` : ''].filter(Boolean);
  const phone = contacts.find((contact) => contact.type.toLowerCase().includes('phone'))?.number ?? contacts[0]?.number ?? null;
  const email = contacts.find((contact) => contact.type.toLowerCase().includes('mail'))?.number ?? null;
  return { name: entity.name || companyCode || 'Company', secondaryName: entity.arabicName, companyCode: entity.dataArea || companyCode, logoSource: reportLogoSource || asImageSource(entity.reportLogo || entity.logo), addressLines, contactLines, registrationLines, vatNumber: entity.taxLicenseNum, commercialRegistration: entity.federalTaxId, phone, email };
};

const blobAsDataUrl = (blob: Blob, signal?: AbortSignal): Promise<string> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    const abort = () => reader.abort();
    reader.onload = () => resolve(String(reader.result));
    reader.onerror = () => reject(reader.error ?? new Error('Unable to read the report logo.'));
    reader.onabort = () => reject(new DOMException('The report-logo request was aborted.', 'AbortError'));
    signal?.addEventListener('abort', abort, { once: true });
    reader.onloadend = () => signal?.removeEventListener('abort', abort);
    reader.readAsDataURL(blob);
  });

export async function fetchreportCompany(companyCode: string, signal?: AbortSignal): Promise<reportCompany> {
  const response = await apiClient.get<ApiResponse<ReportCompanyInfo[]>>('/v1/CompanyInfo', { signal });
  if (!response.data.success || !response.data.data) throw new ApiError(response.data.message || 'Unable to load report company information.', 500);
  const entity = response.data.data.find((item) => item.dataArea.localeCompare(companyCode, undefined, { sensitivity: 'accent' }) === 0);
  if (!entity) return toReportCompany(undefined, companyCode);

  try {
    const documents = await documentApi.list(REPORT_COMPANY_TABLE_ID, entity.recId, signal);
    const attachment = documents.items.find((document) => document.name === REPORT_COMPANY_LOGO_ATTACHMENT);
    if (!attachment) return toReportCompany(entity, companyCode);
    const logo = await documentApi.previewBlob(attachment.id, signal);
    return toReportCompany(entity, companyCode, await blobAsDataUrl(logo, signal));
  } catch (error) {
    if (signal?.aborted) throw error;
    return toReportCompany(entity, companyCode);
  }
}
