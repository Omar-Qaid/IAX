export interface FileMetadata {
  name: string;
  size: number;
  type: string;
}

export const readFileMetadata = (value: string): FileMetadata[] => {
  if (!value) return [];
  try {
    const parsed = JSON.parse(value) as unknown;
    return Array.isArray(parsed)
      ? parsed.flatMap((item) => {
          if (!item || typeof item !== 'object') return [];
          const candidate = item as Partial<FileMetadata> & { n?: string; s?: number; t?: string };
          const name = candidate.name ?? candidate.n;
          const size = candidate.size ?? candidate.s;
          return typeof name === 'string' && typeof size === 'number'
            ? [{ name, size, type: candidate.type ?? candidate.t ?? '' }]
            : [];
        })
      : [];
  } catch {
    return [];
  }
};
