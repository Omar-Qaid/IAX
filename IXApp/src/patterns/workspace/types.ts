import type { ReactNode } from 'react';
export interface WorkspaceTile { id: string; title: string; value?: ReactNode; icon?: ReactNode; onClick?: () => void }
