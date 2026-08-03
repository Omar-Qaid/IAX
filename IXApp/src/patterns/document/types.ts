export type DocumentState = 'draft' | 'open' | 'confirmed' | 'posted' | 'cancelled';
export interface DocumentEntity { id: string; number: string; status: DocumentState }
