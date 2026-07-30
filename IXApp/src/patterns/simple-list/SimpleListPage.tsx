import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { AppDataGrid, type AppDataGridProps } from '@shared/components/data-grid/AppDataGrid';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { Box } from '@mui/material';

export interface SimpleListPageProps<T extends { id: string } = { id: string }> {
  title: string;
  subtitle?: string;
  actionPane?: React.ReactNode;
  dataGridProps: AppDataGridProps<T>;
  loading?: boolean;
  error?: string | null;
  onRetry?: () => void;
  dialogs?: React.ReactNode;
}

export function SimpleListPage<T extends { id: string } = { id: string }>({
  title,
  subtitle,
  actionPane,
  dataGridProps,
  loading = false,
  error,
  onRetry,
  dialogs,
}: SimpleListPageProps<T>): React.ReactElement {
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      {actionPane && <ActionPane>{actionPane}</ActionPane>}

      {error ? (
        <ErrorState message={error} onRetry={onRetry} />
      ) : loading ? (
        <LoadingState message="Loading list records..." />
      ) : (
        <Box sx={{ width: '100%' }}>
          <AppDataGrid {...dataGridProps} />
        </Box>
      )}

      {dialogs}
    </PageContainer>
  );
}
