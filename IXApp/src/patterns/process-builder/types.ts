import type { ReactNode } from 'react';

export type ProcessBuilderNodeKind = 'process' | 'variable' | 'step' | 'activity';

export interface ProcessBuilderNode {
  id: string;
  kind: ProcessBuilderNodeKind;
  label: string;
  secondaryText?: string;
  children?: ProcessBuilderNode[];
}

export interface ProcessBuilderTab {
  id: string;
  label: string;
  content: ReactNode;
}

export interface ProcessBuilderSummaryItem {
  label: string;
  value: number;
}
