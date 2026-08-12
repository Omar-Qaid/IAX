import React from 'react';
import { Box, Chip, Paper, Tab, Tabs, Typography, useMediaQuery, useTheme } from '@mui/material';
import AccountTreeOutlined from '@mui/icons-material/AccountTreeOutlined';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { EmptyDataWatermark } from '@shared/components/feedback/EmptyDataWatermark';
import { ProcessBuilderTree } from './ProcessBuilderTree';
import { useProcessBuilderPage } from './useProcessBuilderPage';
import type { ProcessBuilderNode, ProcessBuilderSummaryItem, ProcessBuilderTab } from './types';

export function ProcessBuilderPage({ title, code, active, nodes, tabs, summary, loading, error, onRetry, properties }: {
  title: string;
  code?: string | null;
  active: boolean;
  nodes: ProcessBuilderNode[];
  tabs: ProcessBuilderTab[];
  summary: ProcessBuilderSummaryItem[];
  loading?: boolean;
  error?: string | null;
  onRetry?: () => void;
  properties: (node: ProcessBuilderNode | null) => React.ReactNode;
}): React.ReactElement {
  const theme = useTheme();
  const compact = useMediaQuery(theme.breakpoints.down('md'));
  const state = useProcessBuilderPage(nodes);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={onRetry} />;

  return <Box sx={{ height: { xs: 'auto', md: 'calc(100vh - 116px)' }, minHeight: 560, display: 'flex', flexDirection: 'column', bgcolor: 'background.paper', border: 1, borderColor: 'divider' }}>
    <Box sx={{ minHeight: 48, px: 1.5, display: 'flex', alignItems: 'center', gap: 1, borderBottom: 1, borderColor: 'divider', flexWrap: 'wrap' }}>
      <AccountTreeOutlined color="primary" />
      <Typography component="h1" sx={{ fontSize: '1rem', fontWeight: 700 }}>{title}</Typography>
      <Chip size="small" label={active ? 'Active' : 'Inactive'} color={active ? 'success' : 'default'} />
      {code && <Chip size="small" variant="outlined" label={code} />}
      <Box sx={{ flex: 1 }} />
      {summary.map((item) => <Chip key={item.label} size="small" variant="outlined" label={`${item.value} ${item.label}`} />)}
    </Box>
    <Box sx={{ flex: 1, minHeight: 0, display: 'grid', gridTemplateColumns: { xs: '1fr', md: '250px minmax(0, 1fr) 280px' }, gridTemplateRows: { xs: 'auto minmax(430px, auto) auto', md: '1fr' } }}>
      <Paper square elevation={0} sx={{ minHeight: 0, overflow: 'auto', borderInlineEnd: { md: 1 }, borderBottom: { xs: 1, md: 0 }, borderColor: 'divider' }}>
        <Typography sx={{ px: 1.5, py: 1, fontSize: '0.6875rem', fontWeight: 700, textTransform: 'uppercase', color: 'text.secondary' }}>Structure</Typography>
        <ProcessBuilderTree nodes={nodes} selectedId={state.selectedId} onSelect={state.setSelectedId} />
      </Paper>
      <Box sx={{ minWidth: 0, minHeight: 0, display: 'flex', flexDirection: 'column', bgcolor: 'background.default' }}>
        <Tabs value={state.activeTab} onChange={(_, value: number) => state.setActiveTab(value)} variant="scrollable" scrollButtons="auto" sx={{ minHeight: 40, bgcolor: 'background.paper', borderBottom: 1, borderColor: 'divider', '& .MuiTab-root': { minHeight: 40, py: 0, fontSize: '0.75rem' } }}>
          {tabs.map((tab) => <Tab key={tab.id} label={tab.label} />)}
        </Tabs>
        <Box role="tabpanel" sx={{ flex: 1, minHeight: 0, overflow: 'auto', p: { xs: 1, md: 2 } }}>
          {tabs[state.activeTab]?.content ?? <EmptyDataWatermark />}
        </Box>
      </Box>
      <Paper square elevation={0} sx={{ minHeight: compact ? 180 : 0, overflow: 'auto', borderInlineStart: { md: 1 }, borderTop: { xs: 1, md: 0 }, borderColor: 'divider' }}>
        <Typography sx={{ px: 1.5, py: 1, fontSize: '0.6875rem', fontWeight: 700, textTransform: 'uppercase', color: 'text.secondary', borderBottom: 1, borderColor: 'divider' }}>Properties</Typography>
        {properties(state.selectedNode)}
      </Paper>
    </Box>
  </Box>;
}
