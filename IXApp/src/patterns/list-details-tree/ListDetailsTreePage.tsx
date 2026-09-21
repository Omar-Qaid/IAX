import React from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig, ListDetailRecord } from '@patterns/list-details/types';
import { TreeNavigationPane } from './TreeNavigationPane';
import type { ListDetailsTreePageConfig } from './types';

export interface ListDetailsTreePageProps<T extends ListDetailRecord> {
  title: string;
  config: ListDetailsTreePageConfig<T>;
  dialogs?: React.ReactNode;
}

export function ListDetailsTreePage<T extends ListDetailRecord>({
  title,
  config,
  dialogs,
}: ListDetailsTreePageProps<T>): React.ReactElement {
  const enhancedConfig = React.useMemo<EnterpriseListDetailsConfig<T>>(
    () => ({
      ...config,
      presentation: {
        ...config.presentation,
        mode: config.presentation?.mode ?? 'list',
        listWidth: config.presentation?.listWidth ?? 320,
      },
      renderListPane: (context) => (
        <TreeNavigationPane
          {...context}
          tree={config.tree}
          getPrimaryText={config.getPrimaryText}
          getSecondaryText={config.getSecondaryText}
        />
      ),
    }),
    [config]
  );

  return (
    <ListDetailsPage variant="enterprise" title={title} config={enhancedConfig} dialogs={dialogs} />
  );
}
