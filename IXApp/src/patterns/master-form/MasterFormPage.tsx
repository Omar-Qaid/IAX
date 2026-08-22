import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { Paper, Box } from '@mui/material';
import { RecordAttachmentsButton, recordTableId } from '@shared/components/documents';

export interface MasterFormPageProps {
  title: string;
  subtitle?: string;
  actionPane?: React.ReactNode;
  children: React.ReactNode;
  refTableId?: number;
  refRecId?: number | null;
}

export const MasterFormPage: React.FC<MasterFormPageProps> = ({
  title,
  subtitle,
  actionPane,
  children,
  refTableId,
  refRecId = null,
}) => {
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      <ActionPane endActions={<RecordAttachmentsButton refTableId={refTableId ?? recordTableId(title)} refRecId={refRecId} />}>{actionPane}</ActionPane>
      <Paper elevation={0} sx={{ p: 1.25, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
        <Box sx={{ width: '100%' }}>{children}</Box>
      </Paper>
    </PageContainer>
  );
};
