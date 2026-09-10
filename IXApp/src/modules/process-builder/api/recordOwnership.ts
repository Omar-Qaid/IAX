/** Local keys denote new records; numeric keys must still belong to their saved parent. */
export function validateRecordOwnership(
  records: readonly { id: string }[],
  stored: readonly { recId: number }[],
  context: string
): void {
  const seen = new Set<string>();
  const available = new Set(stored.map((record) => String(record.recId)));
  for (const record of records) {
    if (!record.id || seen.has(record.id))
      throw new Error(`${context}: missing or duplicate record identity '${record.id}'.`);
    seen.add(record.id);
    if (/^\d+$/.test(record.id) && !available.has(record.id))
      throw new Error(`${context}: record '${record.id}' is stale or belongs to another parent. Reload before saving.`);
  }
}
