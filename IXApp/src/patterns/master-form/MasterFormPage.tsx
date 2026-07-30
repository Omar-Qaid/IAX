import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { Paper, Box } from '@mui/material';

export interface MasterFormPageProps {
  title: string;
  subtitle?: string;
  actionPane?: React.ReactNode;
  children: React.ReactNode;
}

export const MasterFormPage: React.FC<MasterFormPageProps> = ({
  title,
  subtitle,
  actionPane,
  children,
}) => {
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      {actionPane && <ActionPane>{actionPane}</ActionPane>}
      <Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
        <Box sx={{ width: '100%' }}>{children}</Box>
      </Paper>
    </PageContainer>
  );
};
