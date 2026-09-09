import React from 'react';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { Paper, Box, Grid } from '@mui/material';
import { RecordAttachmentsButton, recordTableId } from '@shared/components/documents';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { useNavigate } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface DocumentPageProps {
  title: string;
  subtitle?: string;
  statusBadge?: string;
  actionPane?: React.ReactNode;
  headerContent: React.ReactNode;
  linesContent: React.ReactNode;
  totalsContent?: React.ReactNode;
  dialogs?: React.ReactNode;
  refTableId?: number;
  refRecId?: number | null;
}

export const DocumentPage: React.FC<DocumentPageProps> = ({
  title,
  subtitle,
  statusBadge,
  actionPane,
  headerContent,
  linesContent,
  totalsContent,
  dialogs,
  refTableId,
  refRecId = null,
}) => {
  const navigate = useNavigate();
  const { t } = useAppTranslation();
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} badge={statusBadge} />
      <ActionPane endActions={<RecordAttachmentsButton refTableId={refTableId ?? recordTableId(title)} refRecId={refRecId} />}>
        <ActionPaneBackButton label={t('actions.back')} onClick={() => navigate(-1)} />
        {actionPane}
      </ActionPane>

      <Paper elevation={0} sx={{ p: 1.25, mb: 1, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
        <Box>{headerContent}</Box>
      </Paper>

      <Paper elevation={0} sx={{ p: 1.25, mb: 1, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}` }}>
        <Box sx={{ width: '100%' }}>{linesContent}</Box>
      </Paper>

      {totalsContent && (
        <Grid container sx={{ justifyContent: 'flex-end' }}>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <Paper elevation={0} sx={{ p: 1.25, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}`, bgcolor: (t) => (t.palette.mode === 'light' ? '#fafafa' : '#282828') }}>
              {totalsContent}
            </Paper>
          </Grid>
        </Grid>
      )}

      {dialogs}
    </PageContainer>
  );
};
