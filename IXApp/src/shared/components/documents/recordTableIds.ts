export const documentTableIds = {
  wfRequest: 1001,
  wfRequestDetail: 1002,
} as const;

const knownTableIds: Record<string, number> = {
  wfrequests: documentTableIds.wfRequest, workflowrequests: documentTableIds.wfRequest,
  wfrequestdetails: documentTableIds.wfRequestDetail, customers: 2001, custtable: 2001,
  salesorders: 2002, salestable: 2002, vendors: 3001, vendtable: 3001,
  purchaseorders: 3002, purchtable: 3002, employees: 4001, hcmworkers: 4001,
};

export const recordTableId = (name: string): number => {
  const normalized = name.replace(/[^a-z0-9]/gi, '').toLowerCase();
  const known = knownTableIds[normalized]; if (known) return known;
  let hash = 2166136261;
  for (const character of normalized) { hash ^= character.charCodeAt(0); hash = Math.imul(hash, 16777619); }
  return 10_000 + (Math.abs(hash) % 2_000_000_000);
};
