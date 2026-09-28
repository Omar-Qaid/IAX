export function readControlMetadataForSave(value: string | null | undefined, field: string): Record<string, unknown> {
  if (!value?.trim()) return {};
  const storedValue = value.trim();
  try {
    const parsed: unknown = JSON.parse(storedValue);
    if (parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed))
      return parsed as Record<string, unknown>;
  } catch {
    // Older workflow rows stored metadata as XML. Keep it while the save rewrites
    // the column as a JSON object, so existing processes can be edited normally.
    if (storedValue.startsWith('<') && storedValue.endsWith('>')) {
      try {
        const document = new DOMParser().parseFromString(storedValue, 'application/xml');
        if (!document.querySelector('parsererror') && document.documentElement.nodeName !== 'parsererror')
          return { legacyXml: storedValue };
      } catch {
        // Fall through to the actionable validation error below.
      }
    }
  }
  throw new Error(`${field} contains invalid or non-object JSON. Correct the stored metadata before saving this control.`);
}
