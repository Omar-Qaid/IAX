interface StoredOption {
  recId: number;
  name: string;
  value: string;
}

// Prefer explicit control-scoped identities; label matching supports older drafts.
export function matchStoredOptions<T extends StoredOption>(labels: string[], existing: T[], ids?: (string | null)[]): (T | undefined)[] {
  if (ids) {
    if (ids.length !== labels.length) throw new Error('Option identities do not match the option list.');
    const seen = new Set<string>();
    return ids.map((id) => {
      if (id === null) return undefined;
      if (seen.has(id)) throw new Error('Duplicate option identity.');
      seen.add(id);
      const row = existing.find((item) => String(item.recId) === id);
      if (!row) throw new Error(`Option ${id} no longer belongs to this control. Reload before saving.`);
      return row;
    });
  }
  const used = new Set<number>();
  const matches = labels.map((label) => {
    const candidates = existing.filter((item) => (item.name || item.value).trim() === label.trim());
    if (candidates.length > 1) throw new Error(`Option '${label}' has ambiguous stored identities.`);
    const match = candidates[0];
    if (match && used.has(match.recId)) throw new Error(`Option '${label}' is duplicated.`);
    if (match) used.add(match.recId);
    return match;
  });
  const unmatched = labels.map((_, index) => index).filter((index) => !matches[index]);
  const remaining = existing.filter((item) => !used.has(item.recId));
  if (unmatched.length && remaining.length) {
    if (unmatched.length !== 1 || remaining.length !== 1 || labels.length !== existing.length)
      throw new Error('Option changes are ambiguous. Save renames separately from additions or deletions.');
    matches[unmatched[0]] = remaining[0];
  }
  return matches;
}
