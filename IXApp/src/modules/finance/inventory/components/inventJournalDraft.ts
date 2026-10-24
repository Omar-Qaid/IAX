import type { InventJournal, InventJournalLineInput } from '../api/inventJournalApi';

export function createJournalLineDraft(journal?: InventJournal): InventJournalLineInput {
  return {
    transDate: new Date().toISOString().slice(0, 10),
    itemId: '', inventSiteId: journal?.inventSiteId ?? '', inventLocationId: journal?.inventLocationId ?? '',
    inventBatchId: '', inventSerialId: '', wmsLocationId: '', toInventSiteId: '', toInventLocationId: '',
    toWmsLocationId: '', qty: 1, counted: 0, unit: '', costPrice: 0, ledgerDimension: 0,
  };
}

export function canPostJournal(journal: InventJournal): boolean {
  return !journal.posted && journal.journalType !== 2;
}
