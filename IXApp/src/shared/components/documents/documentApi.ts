import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface DocuTypeDto {
  id: number; typeId: string; name: string; typeGroup: number; kind: 'File' | 'Note' | 'URL' | 'Image';
  filePlace: number | null; description: string | null; allowedExtensions: string[];
  allowedMimeTypes: string[]; maxFileSizeBytes: number | null;
}
export interface DocumentDto {
  id: number; refTableId: number; refRecId: number; refCompanyId: string | null; typeId: string;
  documentTypeName: string; typeGroup: number; kind: DocuTypeDto['kind']; valueRecId: number | null;
  name: string; fileName: string | null; originalFileName: string | null; fileType: string | null;
  contentType: string | null; fileSize: number | null; notes: string | null; url: string | null;
  restriction: number | null; createdBy: string | null; createdAt: string | null;
  modifiedBy: string | null; modifiedAt: string | null;
}
export interface DocumentPageDto { items: DocumentDto[]; pageNumber: number; pageSize: number; totalCount: number }
export interface CreateDocumentInput { typeId: string; name: string; notes: string; url: string; file: File | null }
export interface UpdateDocumentInput { fileName?: string; name?: string; notes?: string; url?: string; restriction?: number | null }

const endpoint = '/v1/documents';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The document response did not contain data.', 500);
  return response.data;
};
const saveBlob = (blob: Blob, fileName: string) => {
  const url = URL.createObjectURL(blob); const anchor = document.createElement('a');
  anchor.href = url; anchor.download = fileName; anchor.click(); window.setTimeout(() => URL.revokeObjectURL(url), 1000);
};

export const documentApi = {
  async types(signal?: AbortSignal): Promise<DocuTypeDto[]> {
    const response = await apiClient.get<ApiResponse<DocuTypeDto[]>>(`${endpoint}/types`, { signal });
    return requireData(response.data);
  },
  async list(refTableId: number, refRecId: number, signal?: AbortSignal): Promise<DocumentPageDto> {
    const response = await apiClient.get<ApiResponse<DocumentPageDto>>(`${endpoint}/record/${refTableId}/${refRecId}`, { signal, params: { pageSize: 100 } });
    return requireData(response.data);
  },
  async create(refTableId: number, refRecId: number, input: CreateDocumentInput, onProgress?: (percent: number) => void): Promise<DocumentDto> {
    const form = new FormData(); form.append('typeId', input.typeId); form.append('name', input.name); form.append('notes', input.notes); form.append('url', input.url);
    if (input.file) form.append('file', input.file);
    const response = await apiClient.post<ApiResponse<DocumentDto>>(`${endpoint}/record/${refTableId}/${refRecId}`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }, onUploadProgress: (event) => onProgress?.(event.total ? Math.round(event.loaded * 100 / event.total) : 0),
    });
    return requireData(response.data);
  },
  async update(id: number, input: UpdateDocumentInput): Promise<DocumentDto> {
    const response = await apiClient.put<ApiResponse<DocumentDto>>(`${endpoint}/${id}`, input); return requireData(response.data);
  },
  async remove(id: number): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${id}`); requireData(response.data);
  },
  async preview(item: DocumentDto): Promise<void> {
    if (item.kind === 'URL' && item.url) { window.open(item.url, '_blank', 'noopener,noreferrer'); return; }
    const response = await apiClient.get<Blob>(`${endpoint}/${item.id}/preview`, { responseType: 'blob' });
    const url = URL.createObjectURL(response.data); window.open(url, '_blank', 'noopener,noreferrer'); window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
  },
  async previewBlob(id: number, signal?: AbortSignal): Promise<Blob> {
    const response = await apiClient.get<Blob>(`${endpoint}/${id}/preview`, { responseType: 'blob', signal });
    return response.data;
  },
  async download(item: DocumentDto): Promise<void> {
    const response = await apiClient.get<Blob>(`${endpoint}/${item.id}/download`, { responseType: 'blob' });
    saveBlob(response.data, item.fileName || item.name || 'document');
  },
};
