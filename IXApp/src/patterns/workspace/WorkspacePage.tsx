import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { Box } from '@mui/material';

export interface WorkspacePageProps {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}

export const WorkspacePage: React.FC<WorkspacePageProps> = ({ title, subtitle, children }) => {
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>{children}</Box>
    </PageContainer>
  );
};
