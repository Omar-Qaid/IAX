import type { LegalEntityRecord, LegalEntityRepository } from '../types/legalEntityTypes';

let records: LegalEntityRecord[] = [
  {
    id: '1',
    recId: 1,
    party: 1,
    dataArea: 'HBMC',
    name: 'AlHayat Building Materials Company',
    languageId: 'en-US',
    currencyCode: 'SAR',
    taxLicenseNum: '',
    federalTaxId: '',
    bankAccount: '',
    calendar: null,
    timeZone: 'Asia/Riyadh',
    memo: '',
    arabicName: 'شركة الحياة لمواد البناء المحدودة',
    localizedRegion: 'SA',
    logo: null,
    reportLogo: null,
    addresses: [],
    contacts: [],
  },
];

const copy = (record: LegalEntityRecord): LegalEntityRecord => structuredClone(record);
export const legalEntityMockRepository: LegalEntityRepository = {
  async list() {
    return records.map(copy);
  },
  async create(entity) {
    const recId = Math.max(0, ...records.map((item) => item.recId)) + 1;
    const created = { ...copy(entity), recId, id: String(recId) };
    records = [...records, created];
    return copy(created);
  },
  async update(entity) {
    records = records.map((item) => (item.id === entity.id ? copy(entity) : item));
    return copy(entity);
  },
  async delete(entity) {
    records = records.filter((item) => item.id !== entity.id);
  },
};
