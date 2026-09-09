import type { ReactNode } from 'react';

export interface MasterDetailRecord {
  id: string;
  title: string;
  subtitle?: string;
  description?: string;
}
export interface MasterDetailPageProps {
  records: MasterDetailRecord[];
  selectedId?: string;
  onSelect: (id: string) => void;
  title: string;
  subtitle?: string;
  status?: ReactNode;
  filterLabel: string;
  emptyLabel: string;
  actionPane: ReactNode;
  backAction?: ReactNode;
  endActions?: ReactNode;
  viewLabel?: string;
  navigationLabel?: string;
  tabs?: ReactNode;
  children: ReactNode;
}
