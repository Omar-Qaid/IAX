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
        color: tokens.text,
        fontFamily: 'Roboto, Inter, "Segoe UI", Arial, sans-serif',
        fontSize: 10,
        '& .MuiButton-root': {
          minHeight: 32,
          borderRadius: `${tokens.radius}px`,
          fontSize: 9,
          fontWeight: 500,
          textTransform: 'none',
        },
        '& .MuiButton-sizeSmall': { minHeight: 28 },
        '& .MuiButton-outlined': { borderColor: tokens.border, color: tokens.textMuted },
        '& .MuiButton-text:not(.MuiButton-colorError)': { color: tokens.accent },
        '& .MuiButton-contained.Mui-disabled': {
          bgcolor: '#dedede',
          color: '#a3a3a3',
          opacity: 1,
        },
        '& .MuiIconButton-root': { borderRadius: `${tokens.radius}px` },
        '& .MuiOutlinedInput-root': {
          minHeight: tokens.controlHeight,
          borderRadius: `${tokens.radius}px`,
          fontSize: 10,
          bgcolor: '#fff',
        },
        '& .MuiInputLabel-root': { fontSize: 9, fontWeight: 500 },
        '& .MuiFormControlLabel-label': { fontSize: 10 },
        '& .MuiChip-root': { fontSize: 9 },
        '& .MuiSvgIcon-root': { fontSize: 16 },
        '& .MuiSwitch-root': { width: 34, height: 20, p: 0.25 },
        '& .MuiSwitch-switchBase': { p: 0.5 },
        '& .MuiSwitch-thumb': { width: 14, height: 14 },
        '& .MuiSwitch-track': { borderRadius: 9, bgcolor: '#a3a3a3' },
        '& .MuiSwitch-switchBase.Mui-checked': {
          transform: 'translateX(14px)',
          color: tokens.accent,
        },
        '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
          bgcolor: tokens.accentLight,
          opacity: 1,
        },
        '& .MuiPaper-outlined': { borderColor: tokens.border },
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
        <AccountTree sx={{ color: tokens.accent, fontSize: 16 }} />
        <Typography sx={{ fontSize: 13, fontWeight: 500, color: '#1f2937', whiteSpace: 'nowrap' }}>
          Process Builder
        </Typography>
        <Chip
          size="small"
          color={s.document.active ? 'success' : 'default'}
          label={s.document.active ? 'Active' : 'Inactive'}
          sx={{
            height: 22,
            bgcolor: s.document.active ? tokens.success : '#e0e0e0',
            color: s.document.active ? '#fff' : tokens.textMuted,
          }}
        />
        <Chip size="small" variant="outlined" label={`#${s.document.id}`} sx={{ height: 22 }} />
        <Chip size="small" variant="outlined" label={s.document.code} sx={{ height: 22 }} />
        <Box sx={{ flex: 1, minWidth: 12 }} />
        {s.dirty ? (
          <Chip size="small" label="Local changes" sx={{ bgcolor: '#f59e0b' }} />
        ) : (
          draft.savedAt && (
            <Typography sx={{ fontSize: 9, color: tokens.textMuted }}>
              <Box component="span" sx={{ color: '#10b981' }}>
                ●
              </Box>{' '}
              Draft auto-saved {draft.savedAt.toLocaleTimeString()}
            </Typography>
          )
        )}
        <Tooltip title="Steps / Activities / Controls / Transitions">
          <Chip
            size="small"
            aria-label="Process statistics"
            label={`${s.document.steps.length}S / ${activities}A / ${controls}C / ${s.document.transitions.length}T`}
            sx={{ height: 24, borderRadius: 12, bgcolor: '#eeeeee' }}
          />
        </Tooltip>
        <Button size="small" sx={{ color: '#d97706' }} startIcon={<RestartAlt />} onClick={reset}>
          Reset
        </Button>
        <Button
          variant="contained"
          startIcon={<Download />}
          onClick={() => setExportOpen(true)}
          sx={{
            bgcolor: tokens.accent,
            borderRadius: `${tokens.radius}px`,
            fontWeight: 700,
            '&:hover': { bgcolor: tokens.accentHover },
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
          sx={{
            position: 'relative',
            minWidth: 0,
            overflow: 'auto',
            borderInlineEnd: `1px solid ${tokens.dividerStrong}`,
          }}
        >
          <Tooltip title={leftOpen ? 'Collapse navigation' : 'Expand navigation'}>
            <IconButton
              size="small"
              aria-label={leftOpen ? 'Collapse navigation' : 'Expand navigation'}
              onClick={() => setLeftOpen((value) => !value)}
              sx={{
                position: 'absolute',
                insetInlineEnd: 2,
                top: 48,
                zIndex: 3,
                opacity: 0.08,
                '&:hover, &:focus-visible': { opacity: 1, bgcolor: '#fff' },
              }}
            >
              {leftOpen ? <ChevronLeft /> : <ChevronRight />}
            </IconButton>
          </Tooltip>
          {leftOpen && (
            <>
              <Tabs
                value={s.leftTab}
                onChange={(_, v: number) => s.setLeftTab(v)}
                variant="fullWidth"
                sx={{
                  minHeight: 46,
                  '& .MuiTab-root': {
                    minHeight: 46,
                    fontSize: 9,
                    fontWeight: 500,
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
                px: 2.25,
                minWidth: 'auto',
                fontSize: 9,
                fontWeight: 500,
                color: tokens.textMuted,
                textTransform: 'uppercase',
                whiteSpace: 'nowrap',
              },
              '& .MuiSvgIcon-root': { fontSize: 16 },
              '& .Mui-selected': { color: `${tokens.accent} !important` },
              '& .MuiTabs-indicator': { bgcolor: tokens.accent, height: 2 },
            }}
          >
            {tabDefinitions.map((x) => (
              <Tab key={x.label} label={x.label} icon={x.icon} iconPosition="start" />
            ))}
          </Tabs>
          <Box role="tabpanel" sx={{ p: '16px' }}>
            {tabs[s.centerTab]}
          </Box>
        </Box>
        <Paper
          square
          elevation={0}
          sx={{
            position: 'relative',
            minWidth: 0,
            overflowY: 'auto',
            overflowX: 'hidden',
            borderInlineStart: `1px solid ${tokens.dividerStrong}`,
          }}
        >
          <Tooltip title={rightOpen ? 'Collapse settings' : 'Expand settings'}>
            <IconButton
              size="small"
              aria-label={rightOpen ? 'Collapse settings' : 'Expand settings'}
              onClick={() => setRightOpen((value) => !value)}
              sx={{
                position: 'absolute',
                insetInlineStart: 2,
                top: 4,
                zIndex: 3,
                opacity: 0.08,
                '&:hover, &:focus-visible': { opacity: 1, bgcolor: '#fff' },
              }}
            >
              {rightOpen ? <ChevronRight /> : <ChevronLeft />}
            </IconButton>
          </Tooltip>
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
