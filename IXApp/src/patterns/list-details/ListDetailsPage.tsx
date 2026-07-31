import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { Grid, Paper, Box } from '@mui/material';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { DataGridProps } from '@shared/components/data-grid/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { EmptyState } from '@shared/components/feedback/EmptyState';

export interface ListDetailsPageProps<T extends { id: string } = { id: string }> {
  title: string;
  subtitle?: string;
  actionPane?: React.ReactNode;
  dataGridProps: DataGridProps<T>;
  detailsPane?: React.ReactNode;
  loading?: boolean;
  selectedId?: string | null;
  dialogs?: React.ReactNode;
}

export function ListDetailsPage<T extends { id: string } = { id: string }>({
  title,
  subtitle,
  actionPane,
  dataGridProps,
  detailsPane,
  loading = false,
  selectedId,
  dialogs,
}: ListDetailsPageProps<T>): React.ReactElement {
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      {actionPane && <ActionPane>{actionPane}</ActionPane>}

      <Grid container spacing={2}>
        {/* Left pane: Data Grid List */}
        <Grid size={{ xs: 12, md: selectedId ? 5 : 12, lg: selectedId ? 4 : 12 }}>
          <Box sx={{ height: 600, width: '100%' }}>
            <DataGrid {...dataGridProps} />
          </Box>
        </Grid>

        {/* Right pane: Details FastTabs */}
        {selectedId && (
          <Grid size={{ xs: 12, md: 7, lg: 8 }}>
            <Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}`, minHeight: 600 }}>
              {loading ? (
                <LoadingState message="Loading details..." />
              ) : detailsPane ? (
                detailsPane
              ) : (
                <EmptyState title="Select a record" message="Choose an item from the list to inspect details." />
              )}
            </Paper>
          </Grid>
        )}
      </Grid>

      {dialogs}
    </PageContainer>
  );
}
