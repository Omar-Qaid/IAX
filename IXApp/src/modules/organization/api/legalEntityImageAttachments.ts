import { queryClient } from '@core/api/queryClient';
import { documentApi, type DocumentDto } from '@shared/components/documents/documentApi';
import {
  REPORT_COMPANY_LOGO_ATTACHMENT,
  REPORT_COMPANY_TABLE_ID,
} from '@shared/components/printout/reportCompany';
import type { LegalEntityRecord } from '../types/legalEntityTypes';

export const LEGAL_ENTITY_TABLE_ID = REPORT_COMPANY_TABLE_ID;
export const LEGAL_ENTITY_DASHBOARD_IMAGE = 'Legal entity dashboard image';
export const LEGAL_ENTITY_REPORT_LOGO = REPORT_COMPANY_LOGO_ATTACHMENT;

const matchingDocuments = (documents: DocumentDto[], name: string) =>
  documents.filter((document) => document.name === name);

async function syncImage(
  refRecId: number,
  name: string,
  file: File | null,
  documents: DocumentDto[]
): Promise<void> {
  const existing = matchingDocuments(documents, name);
  if (!file) {
    await Promise.all(existing.map((document) => documentApi.remove(document.id)));
    return;
  }

  const types = await documentApi.types();
  const imageType =
    types.find(
      (type) =>
        type.kind === 'Image' &&
        (type.allowedMimeTypes.length === 0 || type.allowedMimeTypes.includes(file.type))
    ) ?? types.find((type) => type.kind === 'Image');
  if (!imageType) throw new Error('No image attachment type is configured.');

  // Validate that an image type is available before removing the current logo.
  // Otherwise a failed replacement would leave the legal entity without an image.
  await Promise.all(existing.map((document) => documentApi.remove(document.id)));
  await documentApi.create(LEGAL_ENTITY_TABLE_ID, refRecId, {
    typeId: imageType.typeId,
    name,
    notes: name,
    url: '',
    file,
  });
}

export async function saveLegalEntityImageAttachments(
  source: LegalEntityRecord,
  saved: LegalEntityRecord
): Promise<void> {
  const dashboardChanged = Object.prototype.hasOwnProperty.call(source, 'logoFile');
  const reportChanged = Object.prototype.hasOwnProperty.call(source, 'reportLogoFile');
  if (!dashboardChanged && !reportChanged) return;

  const documents = (await documentApi.list(LEGAL_ENTITY_TABLE_ID, saved.recId)).items;
  if (dashboardChanged)
    await syncImage(saved.recId, LEGAL_ENTITY_DASHBOARD_IMAGE, source.logoFile ?? null, documents);
  if (reportChanged)
    await syncImage(
      saved.recId,
      LEGAL_ENTITY_REPORT_LOGO,
      source.reportLogoFile ?? null,
      documents
    );

  await queryClient.invalidateQueries({
    queryKey: ['documents', LEGAL_ENTITY_TABLE_ID, saved.recId],
  });
}
