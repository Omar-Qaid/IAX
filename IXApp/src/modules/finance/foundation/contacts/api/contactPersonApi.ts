import { apiClient } from '@core/api/apiClient';
import type { Contact } from '../contactTypes';
import { toContactInput, toRecord, type ContactRecord } from '../details/contactPersonModel';

export const contactPersonApi = {
  async load(endpoint: string, signal?: AbortSignal): Promise<ContactRecord[]> {
    return (await apiClient.get<{ contacts: Contact[] }>(endpoint, { signal })).data.contacts.map(
      toRecord
    );
  },
  async save(endpoint: string, row: ContactRecord, create: boolean): Promise<number> {
    const body = toContactInput(row);
    const response = create
      ? await apiClient.post<number>(endpoint, body)
      : await apiClient.put<number>(`${endpoint}/${row.recId}`, body);
    return Number(response.data);
  },
  async remove(recId: number): Promise<void> {
    await apiClient.delete(`/v1/ContactPerson/${recId}`);
  },
};
