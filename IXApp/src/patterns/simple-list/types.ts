export type SimpleListDataSource<T> =
  | { type: 'static'; rows: T[] }
  | { type: 'controlled'; rows: T[]; loading?: boolean; error?: string | null; refresh?: () => void }
  | { type: 'remote'; key: string; load: (signal: AbortSignal) => Promise<T[]>; initialRows?: T[] };

