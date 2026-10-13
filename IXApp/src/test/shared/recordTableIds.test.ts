import { describe, expect, it } from 'vitest';
import { documentTableIds, recordTableId } from '../../shared/components/documents/recordTableIds';

describe('Finance document table numbers', () => {
  it('resolves the same persisted numbers through table names and aliases', () => {
    expect(recordTableId('CustTable')).toBe(documentTableIds.custTable);
    expect(recordTableId('SalesTable')).toBe(documentTableIds.salesTable);
    expect(recordTableId('SalesLine')).toBe(documentTableIds.salesLine);
    expect(recordTableId('VendTable')).toBe(documentTableIds.vendTable);
    expect(recordTableId('HcmWorkers')).toBe(documentTableIds.hcmWorker);
  });
});
