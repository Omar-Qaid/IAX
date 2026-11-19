import { createContext } from 'react';
import type { DataGridHandle } from './types';

export type PageEditor = Pick<DataGridHandle, 'saveEdit' | 'cancelEdit'>;
export interface PageEditSession {
  editing: boolean;
  saving: boolean;
  requestEdit?: () => void;
  registerEditor: (editor: PageEditor) => () => void;
}

/** Page-owned editing shared by its nested grids. Standalone grids remain independent. */
export const PageEditSessionContext = createContext<PageEditSession | null>(null);
