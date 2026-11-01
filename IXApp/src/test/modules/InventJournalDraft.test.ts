import { describe, expect, it } from 'vitest';
import type { InventJournal } from '../../modules/finance/inventory/api/inventJournalApi';
import { canPostJournal, createJournalLineDraft } from '../../modules/finance/inventory/components/inventJournalDraft';

const journal = (overrides: Partial<InventJournal> = {}): InventJournal => ({
  journalId: 'IJ-1', journalType: 0, posted: false, countingFrozen: false, countingConfirmed: false,
  inventSiteId: 'JED', inventLocationId: 'MAIN',
  ...overrides,
} as InventJournal);

describe('inventory journal draft', () => {
  it('starts a new line at the journal header site and warehouse', () => {
    const line = createJournalLineDraft(journal());
    expect(line.inventSiteId).toBe('JED');
    expect(line.inventLocationId).toBe('MAIN');
    expect(line.toInventSiteId).toBe('');
    expect(line.toInventLocationId).toBe('');
    expect(line.unitQty).toBe(1);
    expect(line.qty).toBe(0);
  });

  it('offers counting posting only after confirmation and never for posted journals', () => {
    expect(canPostJournal(journal({ journalType: 2 }))).toBe(false);
    expect(canPostJournal(journal({ journalType: 2, countingConfirmed: true }))).toBe(true);
    expect(canPostJournal(journal({ posted: true }))).toBe(false);
    expect(canPostJournal(journal({ journalType: 0 }))).toBe(true);
  });
});
