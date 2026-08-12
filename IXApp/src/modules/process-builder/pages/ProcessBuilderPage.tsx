import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Paper,
  Tab,
  Tabs,
  Tooltip,
  Typography,
} from '@mui/material';
import AccountTree from '@mui/icons-material/AccountTree';
import ContentCopy from '@mui/icons-material/ContentCopy';
import Download from '@mui/icons-material/Download';
import RestartAlt from '@mui/icons-material/RestartAlt';
import ChevronLeft from '@mui/icons-material/ChevronLeft';
import ChevronRight from '@mui/icons-material/ChevronRight';
import Bolt from '@mui/icons-material/Bolt';
import FormatListBulleted from '@mui/icons-material/FormatListBulleted';
import ViewWeek from '@mui/icons-material/ViewWeek';
import TextFields from '@mui/icons-material/TextFields';
import Visibility from '@mui/icons-material/Visibility';
import AltRoute from '@mui/icons-material/AltRoute';
import { useParams } from 'react-router-dom';
import { ProcessBuilderPalette } from '../components/ProcessBuilderPalette';
import { ProcessBuilderSettingsPanel } from '../components/ProcessBuilderSettingsPanel';
import { ProcessBuilderTreePanel } from '../components/ProcessBuilderTreePanel';
import {
  ActivitiesWorkspace,
  ActivityFormWorkspace,
  DesignerWorkspace,
  DiagramWorkspace,
  RequestFormWorkspace,
  StepsWorkspace,
  TransitionsWorkspace,
  VariablesWorkspace,
} from '../components/ProcessBuilderWorkspace';
import {
  createProcessBuilderDocument,
  useProcessBuilderStore,
} from '../store/useProcessBuilderStore';
import { loadProcessBuilderDraft, useProcessBuilderDraft } from '../hooks/useProcessBuilderDraft';
import { processBuilderTokens as tokens } from '../components/processBuilderTokens';

export function ProcessBuilderPage() {
  const { builderId = 'new' } = useParams();
  const s = useProcessBuilderStore();
  const initialize = s.initialize;
  const [exportOpen, setExportOpen] = useState(false);
  const [leftOpen, setLeftOpen] = useState(true);
  const [rightOpen, setRightOpen] = useState(true);
  const draft = useProcessBuilderDraft(s.document, s.dirty, s.markDraftSaved);
  useEffect(() => {
    const fallback = createProcessBuilderDocument(builderId);
    initialize(loadProcessBuilderDraft(builderId, fallback));
  }, [builderId, initialize]);
  const activities = s.document.steps.reduce((n, x) => n + x.activities.length, 0);
  const controls =
    s.document.requestControls.length +
    s.document.steps.reduce(
      (n, x) => n + x.activities.reduce((m, a) => m + a.controls.length, 0),
      0
    );
  const tabs = [
    <DesignerWorkspace />,
    <VariablesWorkspace />,
    <StepsWorkspace />,
    <ActivitiesWorkspace />,
    <RequestFormWorkspace />,
    <ActivityFormWorkspace />,
    <DiagramWorkspace />,
    <TransitionsWorkspace />,
  ];
  const tabDefinitions = [
    { label: 'Designer', icon: <Bolt /> },
    { label: 'Variables', icon: <FormatListBulleted /> },
    { label: 'Steps', icon: <ViewWeek /> },
    { label: 'Activities', icon: <Bolt /> },
    { label: 'Request form', icon: <TextFields /> },
    { label: 'Activity form', icon: <Visibility /> },
    { label: 'Diagram', icon: <AccountTree /> },
    { label: 'Transitions', icon: <AltRoute /> },
  ];
  const reset = () => {
    if (
      !window.confirm(
        'Discard this local Process Builder draft? Persisted server records will not be deleted.'
      )
    )
      return;
    draft.clear();
    s.initialize(createProcessBuilderDocument(builderId));
  };
  const download = () => {
    const url = URL.createObjectURL(
      new Blob([JSON.stringify(s.document, null, 2)], { type: 'application/json' })
    );
    const a = document.createElement('a');
    a.href = url;
    a.download = `${s.document.code || 'process'}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };
  return (
    <Box
      sx={{
        height: { xs: 'auto', md: 'calc(100vh - 108px)' },
        minHeight: 620,
        bgcolor: '#fff',
        borderTop: `2px solid ${tokens.accent}`,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      <Typography
        component="h1"
        sx={{
          position: 'absolute',
          width: 1,
          height: 1,
          p: 0,
          m: -1,
          overflow: 'hidden',
          clip: 'rect(0 0 0 0)',
          whiteSpace: 'nowrap',
          border: 0,
        }}
      >
        Process Builder
      </Typography>
      <Box
        sx={{
          minHeight: tokens.headerHeight,
          px: 1.25,
          display: 'flex',
          alignItems: 'center',
          gap: 0.75,
          borderBottom: `1px solid ${tokens.border}`,
          flexWrap: 'nowrap',
          overflowX: 'auto',
        }}
      >
        <AccountTree sx={{ color: tokens.accent, fontSize: 22 }} />
        <Typography sx={{ fontSize: 17, fontWeight: 700, color: '#1f2937', whiteSpace: 'nowrap' }}>
          Process Builder
        </Typography>
        <Chip
          size="small"
          color={s.document.active ? 'success' : 'default'}
          label={s.document.active ? 'Active' : 'Inactive'}
          sx={{ height: 25 }}
        />
        <Chip size="small" variant="outlined" label={`#${s.document.id}`} sx={{ height: 25 }} />
        <Chip size="small" variant="outlined" label={s.document.code} sx={{ height: 25 }} />
        <Box sx={{ flex: 1, minWidth: 12 }} />
        {s.dirty ? (
          <Chip size="small" label="Local changes" sx={{ bgcolor: '#f59e0b' }} />
        ) : (
          draft.savedAt && (
            <Typography sx={{ fontSize: 12, color: tokens.textMuted }}>
              <Box component="span" sx={{ color: '#10b981' }}>
                ●
              </Box>{' '}
              Draft auto-saved {draft.savedAt.toLocaleTimeString()}
            </Typography>
          )
        )}
        <Chip
          size="small"
          label={`${s.document.steps.length}S / ${activities}A / ${controls}C / ${s.document.transitions.length}T`}
          sx={{ height: 26, borderRadius: 1 }}
        />
        <Button size="small" sx={{ color: '#d97706' }} startIcon={<RestartAlt />} onClick={reset}>
          Reset
        </Button>
        <Button
          variant="contained"
          startIcon={<Download />}
          onClick={() => setExportOpen(true)}
          sx={{
            bgcolor: tokens.accent,
            borderRadius: 1.5,
            fontWeight: 700,
            '&:hover': { bgcolor: '#5546d7' },
          }}
        >
          Export
        </Button>
      </Box>
      <Box
        sx={{
          flex: 1,
          minHeight: 0,
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: `${leftOpen ? tokens.leftWidth : 42}px minmax(0, 1fr) ${rightOpen ? tokens.rightWidth : 42}px`,
          },
          gridTemplateRows: { xs: 'auto minmax(520px, auto) auto', md: '1fr' },
          transition: 'grid-template-columns 160ms ease',
        }}
      >
        <Paper
          square
          elevation={0}
          sx={{ minWidth: 0, overflow: 'auto', borderInlineEnd: `1px solid ${tokens.border}` }}
        >
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', minHeight: 36 }}>
            <Tooltip title={leftOpen ? 'Collapse navigation' : 'Expand navigation'}>
              <IconButton
                size="small"
                aria-label={leftOpen ? 'Collapse navigation' : 'Expand navigation'}
                onClick={() => setLeftOpen((value) => !value)}
                sx={{ m: 0.25 }}
              >
                {leftOpen ? <ChevronLeft /> : <ChevronRight />}
              </IconButton>
            </Tooltip>
          </Box>
          {leftOpen && (
            <>
              <Tabs
                value={s.leftTab}
                onChange={(_, v: number) => s.setLeftTab(v)}
                variant="fullWidth"
                sx={{
                  minHeight: 48,
                  '& .MuiTab-root': {
                    minHeight: 48,
                    fontSize: 13,
                    fontWeight: 700,
                    color: tokens.textMuted,
                  },
                  '& .Mui-selected': { color: `${tokens.accent} !important` },
                  '& .MuiTabs-indicator': { bgcolor: tokens.accent },
                }}
              >
                <Tab label="Tree" />
                <Tab label="Palette" />
              </Tabs>
              {s.leftTab === 0 ? <ProcessBuilderTreePanel /> : <ProcessBuilderPalette />}
            </>
          )}
        </Paper>
        <Box sx={{ minWidth: 0, overflow: 'auto', bgcolor: tokens.canvas }}>
          <Tabs
            value={s.centerTab}
            onChange={(_, v: number) => s.setCenterTab(v)}
            variant="scrollable"
            scrollButtons="auto"
            allowScrollButtonsMobile
            sx={{
              position: 'sticky',
              top: 0,
              zIndex: 2,
              minHeight: tokens.tabsHeight,
              bgcolor: '#fff',
              borderBottom: `1px solid ${tokens.border}`,
              '& .MuiTab-root': {
                minHeight: tokens.tabsHeight,
                px: 1.5,
                minWidth: 'auto',
                fontSize: 12,
                fontWeight: 700,
                color: tokens.textMuted,
                textTransform: 'uppercase',
                whiteSpace: 'nowrap',
              },
              '& .MuiSvgIcon-root': { fontSize: 18 },
              '& .Mui-selected': { color: `${tokens.accent} !important` },
              '& .MuiTabs-indicator': { bgcolor: tokens.accent, height: 2 },
            }}
          >
            {tabDefinitions.map((x) => (
              <Tab key={x.label} label={x.label} icon={x.icon} iconPosition="start" />
            ))}
          </Tabs>
          <Box role="tabpanel" sx={{ p: { xs: 1, sm: 1.5 } }}>
            {tabs[s.centerTab]}
          </Box>
        </Box>
        <Paper
          square
          elevation={0}
          sx={{
            minWidth: 0,
            overflowY: 'auto',
            overflowX: 'hidden',
            borderInlineStart: `1px solid ${tokens.border}`,
          }}
        >
          <Box
            sx={{
              position: 'sticky',
              top: 0,
              zIndex: 2,
              p: 1,
              display: 'flex',
              alignItems: 'center',
              bgcolor: '#fff',
              borderBottom: `1px solid ${tokens.border}`,
            }}
          >
            <Tooltip title={rightOpen ? 'Collapse settings' : 'Expand settings'}>
              <IconButton
                size="small"
                aria-label={rightOpen ? 'Collapse settings' : 'Expand settings'}
                onClick={() => setRightOpen((value) => !value)}
              >
                {rightOpen ? <ChevronRight /> : <ChevronLeft />}
              </IconButton>
            </Tooltip>
            {rightOpen && (
              <>
                <Typography sx={{ flex: 1, fontSize: 16, fontWeight: 800 }}>Settings</Typography>
                {s.dirty && (
                  <Chip
                    size="small"
                    label="unsaved"
                    sx={{ bgcolor: '#f59e0b', color: '#111827', height: 24 }}
                  />
                )}
              </>
            )}
          </Box>
          {rightOpen && <ProcessBuilderSettingsPanel />}
        </Paper>
      </Box>
      <Dialog open={exportOpen} onClose={() => setExportOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Export process</DialogTitle>
        <DialogContent dividers>
          <Box
            component="pre"
            sx={{
              m: 0,
              p: 1.5,
              bgcolor: 'background.default',
              overflow: 'auto',
              maxHeight: 420,
              fontSize: 11,
            }}
          >
            {JSON.stringify(s.document, null, 2)}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button
            startIcon={<ContentCopy />}
            onClick={() => void navigator.clipboard.writeText(JSON.stringify(s.document, null, 2))}
          >
            Copy JSON
          </Button>
          <Button startIcon={<Download />} onClick={download}>
            Download
          </Button>
          <Button variant="contained" onClick={() => setExportOpen(false)}>
            Close
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
