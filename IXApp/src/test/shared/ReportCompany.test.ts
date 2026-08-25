import { afterEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { documentApi } from '@shared/components/documents/documentApi';
import { fetchPrintoutCompany, REPORT_COMPANY_LOGO_ATTACHMENT, type ReportCompanyInfo } from '@shared/components/printout/reportCompany';

const company: ReportCompanyInfo = {
  recId: 7,
  dataArea: 'HBMC',
  name: 'AlHayat Company',
  arabicName: 'شركة الحياة',
  addresses: [{ address: 'Riyadh', street: 'King Road', city: 'Riyadh', state: '', zipCode: '12345', countryRegionId: 'SA', primary: true }],
  contacts: [{ type: 'Phone', number: '+966500000000', extension: '', primary: true }],
};

afterEach(() => vi.restoreAllMocks());

describe('report company loader', () => {
  it('loads CompanyInfo and its managed report-logo attachment', async () => {
    vi.spyOn(apiClient, 'get').mockResolvedValue({ data: { success: true, data: [company], message: '' } } as never);
    vi.spyOn(documentApi, 'list').mockResolvedValue({ items: [{ id: 91, name: REPORT_COMPANY_LOGO_ATTACHMENT }], pageNumber: 1, pageSize: 100, totalCount: 1 } as never);
    vi.spyOn(documentApi, 'previewBlob').mockResolvedValue(new Blob(['logo'], { type: 'image/png' }));

    const result = await fetchPrintoutCompany('hbmc');

    expect(result.name).toBe('AlHayat Company');
    expect(result.addressLines).toContain('Riyadh');
    expect(result.contactLines?.[0]).toContain('+966500000000');
    expect(result.logoSource).toMatch(/^data:image\/png;base64,/);
  });
});
