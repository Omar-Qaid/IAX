export const documentTableIds = {
  wfRequest: 1001,
  wfRequestDetail: 1002,
  custTable: 2001,
  salesTable: 2002,
  salesLine: 2003,
  vendTable: 3001,
  purchTable: 3002,
  hcmWorker: 4001,
} as const;

const knownTableIds: Record<string, number> = {
  wfrequests: documentTableIds.wfRequest, workflowrequests: documentTableIds.wfRequest,
  wfrequestdetails: documentTableIds.wfRequestDetail, customers: documentTableIds.custTable, custtable: documentTableIds.custTable,
  salesorders: documentTableIds.salesTable, salestable: documentTableIds.salesTable,
  saleslines: documentTableIds.salesLine, salesline: documentTableIds.salesLine,
  vendors: documentTableIds.vendTable, vendtable: documentTableIds.vendTable,
  purchaseorders: documentTableIds.purchTable, purchtable: documentTableIds.purchTable,
  employees: documentTableIds.hcmWorker, hcmworkers: documentTableIds.hcmWorker,
};

export const recordTableId = (name: string): number => {
  const normalized = name.replace(/[^a-z0-9]/gi, '').toLowerCase();
  const known = knownTableIds[normalized]; if (known) return known;
  let hash = 2166136261;
  for (const character of normalized) { hash ^= character.charCodeAt(0); hash = Math.imul(hash, 16777619); }
  return 10_000 + (Math.abs(hash) % 2_000_000_000);
};
