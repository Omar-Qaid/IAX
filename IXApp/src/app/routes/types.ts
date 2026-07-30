import type { ReactNode } from 'react';

export interface AppRoute {
  path: string;
  element: ReactNode;
  title: string;
  breadcrumb?: string;
  permission?: string;
  module?: string;
  navigation?: boolean;
}
