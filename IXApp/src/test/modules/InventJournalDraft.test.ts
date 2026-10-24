import { describe, expect, it } from 'vitest';
import type { InventJournal } from '../../modules/finance/inventory/api/inventJournalApi';
import { canPostJournal, createJournalLineDraft } from '../../modules/finance/inventory/components/inventJournalDraft';

const journal = (overrides: Partial<InventJournal> = {}): InventJournal => ({
  journalId: 'IJ-1', journalType: 0, posted: false, inventSiteId: 'JED', inventLocationId: 'MAIN',
  ...overrides,
} as InventJournal);

describe('inventory journal draft', () => {
  it('starts a new line at the journal header site and warehouse', () => {
    const line = createJournalLineDraft(journal());
    expect(line.inventSiteId).toBe('JED');
    expect(line.inventLocationId).toBe('MAIN');
    expect(line.toInventSiteId).toBe('');
    expect(line.toInventLocationId).toBe('');
  });

  it('does not offer posting for counting or already posted journals', () => {
    expect(canPostJournal(journal({ journalType: 2 }))).toBe(false);
    expect(canPostJournal(journal({ posted: true }))).toBe(false);
    expect(canPostJournal(journal({ journalType: 0 }))).toBe(true);
  });
});
