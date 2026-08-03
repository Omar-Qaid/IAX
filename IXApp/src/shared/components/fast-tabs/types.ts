import type { ReactNode } from 'react';
export interface FastTabDefinition { id: string; title: string; content: ReactNode; defaultExpanded?: boolean; disabled?: boolean }
