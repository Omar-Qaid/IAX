export function readControlMetadataForSave(value: string | null | undefined, field: string): Record<string, unknown> {
  if (!value?.trim()) return {};
  try {
    const parsed: unknown = JSON.parse(value);
    if (parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed))
      return parsed as Record<string, unknown>;
  } catch {
    // Preserve the original stored value by rejecting the save.
  }
  throw new Error(`${field} contains invalid or non-object JSON. Correct the stored metadata before saving this control.`);
}
