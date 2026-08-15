import React, { useEffect, useLayoutEffect, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Drawer,
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
import MenuOpen from '@mui/icons-material/MenuOpen';
import Tune from '@mui/icons-material/Tune';
import Save from '@mui/icons-material/Save';
import { useNavigate, useParams } from 'react-router-dom';
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
import {
  getProcessCodeMetadata,
  getVariableCodeMetadata,
  getStepCodeMetadata,
  getActivityCodeMetadata,
  getRequestControlCodeMetadata,
  loadProcessBuilder,
  saveProcessBuilder,
  saveProcessVariables,
  saveProcessActivities,
  saveProcessRequestControls,
  saveProcessTransitions,
} from '../api/processBuilderApi';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useNotifications } from '@shared/hooks/useNotifications';

const slimScrollbarSx = {
  '&, & *': {
    scrollbarWidth: 'thin',
    scrollbarColor: '#a8a8a8 transparent',
  },
  '&::-webkit-scrollbar, & *::-webkit-scrollbar': { width: 6, height: 6 },
  '&::-webkit-scrollbar-track, & *::-webkit-scrollbar-track': { backgroundColor: 'transparent' },
  '&::-webkit-scrollbar-thumb, & *::-webkit-scrollbar-thumb': {
    backgroundColor: '#a8a8a8',
    borderRadius: 999,
  },
  '&::-webkit-scrollbar-thumb:hover, & *::-webkit-scrollbar-thumb:hover': {
    backgroundColor: '#7d7d7d',
  },
} as const;

function ProcessBuilderNavigationPanel() {
  const s = useProcessBuilderStore();
  return (
    <>
      <Tabs
        value={s.leftTab}
        onChange={(_, value: number) => s.setLeftTab(value)}
        variant="fullWidth"
        aria-label="Process Builder navigation"
        sx={{
          minHeight: 40,
          '& .MuiTab-root': {
            minHeight: 40,
            fontSize: tokens.fontSize.secondary,
            fontWeight: 600,
            color: tokens.textMuted,
          },
          '& .Mui-selected': { color: `${tokens.accent} !important` },
          '& .MuiTabs-indicator': { bgcolor: tokens.accent, height: 2 },
        }}
      >
        <Tab label="Tree" />
        <Tab label="Palette" />
      </Tabs>
      {s.leftTab === 0 ? <ProcessBuilderTreePanel /> : <ProcessBuilderPalette />}
    </>
  );
}

export function ProcessBuilderPage() {
  const { builderId = 'new' } = useParams();
  const navigate = useNavigate();
  const { notifyError, notifySuccess } = useNotifications();
  const s = useProcessBuilderStore();
  const initialize = s.initialize;
  const [exportOpen, setExportOpen] = useState(false);
  const [leftOpen, setLeftOpen] = useState(() => sessionStorage.getItem('ixapp.processBuilder.leftOpen') !== 'false');
  const [rightOpen, setRightOpen] = useState(() => sessionStorage.getItem('ixapp.processBuilder.rightOpen') !== 'false');
  const [mobileNavigationOpen, setMobileNavigationOpen] = useState(false);
  const [mobileSettingsOpen, setMobileSettingsOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [savingVariables, setSavingVariables] = useState(false);
  const [manualVariableCode, setManualVariableCode] = useState(false);
  const [manualStepCode, setManualStepCode] = useState(false);
  const [manualActivityCode, setManualActivityCode] = useState(false);
  const [savingActivities, setSavingActivities] = useState(false);
  const [manualRequestControlCode, setManualRequestControlCode] = useState(false);
  const [savingRequestControls, setSavingRequestControls] = useState(false);
  const [savingTransitions, setSavingTransitions] = useState(false);
  const draft = useProcessBuilderDraft(s.document, s.dirty, s.markDraftSaved);
  useLayoutEffect(() => {
    let active = true;
    const load = async () => {
      setLoading(true);
      try {
        const [variableMetadata, stepMetadata, activityMetadata, requestControlMetadata] = await Promise.all([
          getVariableCodeMetadata(),
          getStepCodeMetadata(),
          getActivityCodeMetadata(),
          getRequestControlCodeMetadata(),
        ]);
        if (active) {
          setManualVariableCode(variableMetadata.manual);
          setManualStepCode(stepMetadata.manual);
          setManualActivityCode(activityMetadata.manual);
          setManualRequestControlCode(requestControlMetadata.manual);
        }
        if (builderId === 'new') {
          const fallback = createProcessBuilderDocument('new');
          const recovered = loadProcessBuilderDraft(builderId, fallback);
          if (active) initialize(recovered);
          const metadata = await getProcessCodeMetadata();
          if (active && !metadata.manual)
            useProcessBuilderStore.getState().setGeneratedCode(metadata.previewCode ?? '');
        } else {
          const fallback = await loadProcessBuilder(Number(builderId));
          if (active) initialize(loadProcessBuilderDraft(builderId, fallback));
        }
      } catch (error) {
        if (active) {
          // A preview failure must not discard edits already made in a new draft.
          if (builderId !== 'new') initialize(createProcessBuilderDocument(builderId));
          notifyError(error instanceof Error ? error.message : 'Failed to load process builder.');
        }
      } finally {
        if (active) setLoading(false);
      }
    };
    void load();
    return () => { active = false; };
  }, [builderId, initialize, notifyError]);
  useEffect(() => {
    sessionStorage.setItem('ixapp.processBuilder.leftOpen', String(leftOpen));
  }, [leftOpen]);
  useEffect(() => {
    sessionStorage.setItem('ixapp.processBuilder.rightOpen', String(rightOpen));
  }, [rightOpen]);
  const activities = s.document.steps.reduce((n, x) => n + x.activities.length, 0);
  const controls =
    s.document.requestControls.length +
    s.document.steps.reduce(
      (n, x) => n + x.activities.reduce((m, a) => m + a.controls.length, 0),
      0
    );
  const tabs = [
    <DesignerWorkspace />,
    <VariablesWorkspace
      onSave={() => void saveVariables()}
      saving={savingVariables}
      manualCode={manualVariableCode}
    />,
    <StepsWorkspace manualCode={manualStepCode} />,
    <ActivitiesWorkspace
      onSave={() => void saveActivities()}
      saving={savingActivities}
      manualCode={manualActivityCode}
    />,
    <RequestFormWorkspace
      onSave={() => void saveRequestControls()}
      saving={savingRequestControls}
      manualCode={manualRequestControlCode}
    />,
    <ActivityFormWorkspace />,
    <DiagramWorkspace />,
    <TransitionsWorkspace onSave={() => void saveTransitions()} saving={savingTransitions} />,
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
  const save = async () => {
    setSaving(true);
    try {
      const previousId = s.document.id;
      const persisted = await saveProcessBuilder(s.document);
      localStorage.removeItem(`ixapp.process-builder.${previousId}`);
      useProcessBuilderStore.getState().applyPersistedDocument(persisted);
      notifySuccess('Process saved successfully.');
      if (previousId === 'new') navigate(ROUTE_PATHS.processBuilder(persisted.id), { replace: true });
    } catch (error) {
      notifyError(error instanceof Error ? error.message : 'Failed to save process.');
    } finally {
      setSaving(false);
    }
  };
  const saveVariables = async () => {
    setSavingVariables(true);
    try {
      const variables = await saveProcessVariables(useProcessBuilderStore.getState().document);
      useProcessBuilderStore.getState().setPersistedVariables(variables);
      notifySuccess('Variables saved successfully.');
    } catch (error) {
      notifyError(error instanceof Error ? error.message : 'Failed to save variables.');
    } finally {
      setSavingVariables(false);
    }
  };
  const saveActivities = async () => {
    setSavingActivities(true);
    try {
      const persisted = await saveProcessActivities(useProcessBuilderStore.getState().document);
      useProcessBuilderStore.getState().applyPersistedDocument(persisted);
      notifySuccess('Activities saved successfully.');
    } catch (error) {
      notifyError(error instanceof Error ? error.message : 'Failed to save activities.');
    } finally {
      setSavingActivities(false);
    }
  };
  const saveRequestControls = async () => {
    setSavingRequestControls(true);
    try {
      const controls = await saveProcessRequestControls(useProcessBuilderStore.getState().document);
      useProcessBuilderStore.getState().setPersistedRequestControls(controls);
      notifySuccess('Request controls saved successfully.');
    } catch (error) {
      notifyError(error instanceof Error ? error.message : 'Failed to save request controls.');
    } finally {
      setSavingRequestControls(false);
    }
  };
  const saveTransitions = async () => {
    setSavingTransitions(true);
    try {
      const persisted = await saveProcessTransitions(useProcessBuilderStore.getState().document);
      useProcessBuilderStore.getState().applyPersistedDocument(persisted);
      notifySuccess('Transitions saved successfully.');
    } catch (error) {
      notifyError(error instanceof Error ? error.message : 'Failed to save transitions.');
    } finally {
      setSavingTransitions(false);
    }
  };
  return (
    <Box
      sx={{
        ...slimScrollbarSx,
        height: { xs: 'calc(100dvh - 58px)', md: 'calc(100vh - 108px)' },
        minHeight: { xs: 0, md: 620 },
        bgcolor: '#fff',
        borderTop: `2px solid ${tokens.accent}`,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        color: tokens.text,
        fontFamily: 'Roboto, Inter, "Segoe UI", Arial, sans-serif',
        fontSize: tokens.fontSize.body,
        '& .MuiButton-root': {
          minHeight: 28,
          px: 1,
          py: 0.375,
          borderRadius: `${tokens.radius}px`,
          fontSize: tokens.fontSize.secondary,
          fontWeight: 500,
          textTransform: 'none',
        },
        '& .MuiButton-sizeSmall': { minHeight: 26 },
        '& .MuiButton-outlined': { borderColor: tokens.border, color: tokens.textMuted },
        '& .MuiButton-text:not(.MuiButton-colorError)': { color: tokens.accent },
        '& .MuiButton-contained.Mui-disabled': {
          bgcolor: '#dedede',
          color: '#a3a3a3',
          opacity: 1,
        },
        '& .MuiIconButton-root': { borderRadius: `${tokens.radius}px` },
        '& .MuiAccordionSummary-root': { px: 1.25 },
        '& .MuiAccordionSummary-content': { my: 0.75 },
        '& .MuiAccordionDetails-root': { p: 1.25 },
        '& .MuiFormHelperText-root': { mt: 0.25, mx: 0.5, fontSize: tokens.fontSize.caption },
        '& .MuiOutlinedInput-root': {
          minHeight: tokens.controlHeight,
          borderRadius: `${tokens.radius}px`,
          fontSize: tokens.fontSize.body,
          bgcolor: '#fff',
        },
        '& .MuiInputLabel-root': { fontSize: tokens.fontSize.secondary, fontWeight: 500 },
        '& .MuiFormControlLabel-label': { fontSize: tokens.fontSize.secondary },
        '& .MuiChip-root': { fontSize: tokens.fontSize.caption },
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
        '& :focus-visible': { outline: 'none', boxShadow: tokens.focusRing },
        '@media (prefers-reduced-motion: reduce)': {
          '&, & *': { scrollBehavior: 'auto !important', transitionDuration: '0.01ms !important' },
        },
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
          px: 0.75,
          display: 'flex',
          alignItems: 'center',
          gap: 0.5,
          borderBottom: `1px solid ${tokens.border}`,
          flexWrap: 'nowrap',
          overflowX: 'auto',
        }}
      >
        <AccountTree sx={{ color: tokens.accent, fontSize: 16 }} />
        <Typography sx={{ fontSize: 12, fontWeight: 600, color: '#1f2937', whiteSpace: 'nowrap' }}>
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
        <Chip size="small" variant="outlined" label={`#${s.document.id}`} sx={{ display: { xs: 'none', sm: 'flex' }, height: 22 }} />
        <Chip size="small" variant="outlined" label={s.document.code} sx={{ display: { xs: 'none', sm: 'flex' }, height: 22 }} />
        <Tooltip title={leftOpen ? 'Collapse navigation' : 'Expand navigation'}>
          <IconButton
            size="small"
            aria-label={leftOpen ? 'Collapse navigation' : 'Expand navigation'}
            onClick={() => setLeftOpen((value) => !value)}
            sx={{
              display: { xs: 'none', lg: 'inline-flex' },
              color: tokens.textMuted,
              border: `1px solid ${tokens.border}`,
              borderRadius: `${tokens.radius}px`,
              '&:hover, &:focus-visible': { color: tokens.accent, bgcolor: tokens.accentSoft },
            }}
          >
            {leftOpen ? <ChevronLeft /> : <ChevronRight />}
          </IconButton>
        </Tooltip>
        <Tooltip title={rightOpen ? 'Collapse settings' : 'Expand settings'}>
          <IconButton
            size="small"
            aria-label={rightOpen ? 'Collapse settings' : 'Expand settings'}
            onClick={() => setRightOpen((value) => !value)}
            sx={{
              display: { xs: 'none', lg: 'inline-flex' },
              color: tokens.textMuted,
              border: `1px solid ${tokens.border}`,
              borderRadius: `${tokens.radius}px`,
              '&:hover, &:focus-visible': { color: tokens.accent, bgcolor: tokens.accentSoft },
            }}
          >
            {rightOpen ? <ChevronRight /> : <ChevronLeft />}
          </IconButton>
        </Tooltip>
        <Tooltip title="Open process structure">
          <IconButton
            size="small"
            aria-label="Open process structure"
            onClick={() => setMobileNavigationOpen(true)}
            sx={{ display: { xs: 'inline-flex', lg: 'none' }, color: tokens.accent }}
          >
            <MenuOpen />
          </IconButton>
        </Tooltip>
        <Tooltip title="Open settings">
          <IconButton
            size="small"
            aria-label="Open settings"
            onClick={() => setMobileSettingsOpen(true)}
            sx={{ display: { xs: 'inline-flex', lg: 'none' }, color: tokens.accent }}
          >
            <Tune />
          </IconButton>
        </Tooltip>
        <Box sx={{ flex: 1, minWidth: 12 }} />
        {loading && <Chip size="small" label="Loading from server…" />}
        {s.dirty ? (
          <Chip size="small" label="Local changes" sx={{ bgcolor: '#f59e0b' }} />
        ) : (
          draft.savedAt && (
            <Typography sx={{ fontSize: tokens.fontSize.secondary, color: tokens.textMuted }}>
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
            sx={{ display: { xs: 'none', sm: 'flex' }, height: 24, borderRadius: 12, bgcolor: '#eeeeee' }}
          />
        </Tooltip>
        <Button size="small" sx={{ display: { xs: 'none', sm: 'inline-flex' }, color: '#d97706' }} startIcon={<RestartAlt />} onClick={reset}>
          Reset
        </Button>
        <Button
          variant="contained"
          startIcon={<Save />}
          disabled={loading || saving}
          onClick={() => void save()}
          sx={{ bgcolor: tokens.success, '&:hover': { bgcolor: '#047857' } }}
        >
          {saving ? 'Saving…' : builderId === 'new' ? 'Create' : 'Save'}
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
            lg: `${leftOpen ? tokens.leftWidth : 42}px minmax(0, 1fr) ${rightOpen ? tokens.rightWidth : 42}px`,
          },
          gridTemplateRows: '1fr',
          transition: 'grid-template-columns 160ms ease',
        }}
      >
        <Paper
          square
          elevation={0}
          sx={{
            position: 'relative',
            display: { xs: 'none', lg: 'block' },
            minWidth: 0,
            overflow: 'auto',
            borderInlineEnd: `1px solid ${tokens.dividerStrong}`,
          }}
        >
          {leftOpen && (
            <ProcessBuilderNavigationPanel />
          )}
        </Paper>
        <Box sx={{ minWidth: 0, overflow: 'auto', bgcolor: tokens.canvas }}>
          <Tabs
            value={s.centerTab}
            onChange={(_, v: number) => s.setCenterTab(v)}
            aria-label="Process Builder workspaces"
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
                px: { xs: 1.5, lg: 2.25 },
                minWidth: 'auto',
                fontSize: tokens.fontSize.secondary,
                fontWeight: 600,
                color: tokens.textMuted,
                textTransform: 'uppercase',
                whiteSpace: 'nowrap',
              },
              '& .MuiSvgIcon-root': { fontSize: 16 },
              '& .Mui-selected': { color: `${tokens.accent} !important` },
              '& .MuiTabs-indicator': { bgcolor: tokens.accent, height: 2 },
              '& .MuiTab-root:nth-of-type(4), & .MuiTab-root:nth-of-type(7)': {
                borderInlineStart: `1px solid ${tokens.border}`,
              },
            }}
          >
            {tabDefinitions.map((x) => (
              <Tab key={x.label} label={x.label} icon={x.icon} iconPosition="start" />
            ))}
          </Tabs>
          <Box role="tabpanel" aria-label={tabDefinitions[s.centerTab]?.label} sx={{ p: { xs: '8px', sm: '10px' } }}>
            {tabs[s.centerTab]}
          </Box>
        </Box>
        <Paper
          square
          elevation={0}
          sx={{
            position: 'relative',
            display: { xs: 'none', lg: 'block' },
            minWidth: 0,
            overflowY: 'auto',
            overflowX: 'hidden',
            borderInlineStart: `1px solid ${tokens.dividerStrong}`,
          }}
        >
          {rightOpen && <ProcessBuilderSettingsPanel />}
        </Paper>
      </Box>
      <Drawer
        open={mobileNavigationOpen}
        onClose={() => setMobileNavigationOpen(false)}
        aria-label="Process structure and palette"
        slotProps={{ paper: { sx: { ...slimScrollbarSx, top: 58, height: 'calc(100dvh - 58px)', width: 'min(88vw, 340px)' } } }}
      >
        <Box sx={{ minHeight: 48, px: '16px', display: 'flex', alignItems: 'center', borderBottom: `1px solid ${tokens.border}` }}>
          <Typography component="h2" sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}>Process structure</Typography>
          <IconButton aria-label="Close process structure" onClick={() => setMobileNavigationOpen(false)}><ChevronLeft /></IconButton>
        </Box>
        <Box sx={{ overflowY: 'auto' }}><ProcessBuilderNavigationPanel /></Box>
      </Drawer>
      <Drawer
        anchor="right"
        open={mobileSettingsOpen}
        onClose={() => setMobileSettingsOpen(false)}
        aria-label="Process Builder settings"
        slotProps={{ paper: { sx: { ...slimScrollbarSx, top: 58, height: 'calc(100dvh - 58px)', width: 'min(92vw, 380px)' } } }}
      >
        <Box sx={{ minHeight: 48, px: '16px', display: 'flex', alignItems: 'center', borderBottom: `1px solid ${tokens.border}` }}>
          <Typography component="h2" sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}>Settings</Typography>
          <IconButton aria-label="Close settings" onClick={() => setMobileSettingsOpen(false)}><ChevronRight /></IconButton>
        </Box>
        <Box sx={{ overflowY: 'auto' }}><ProcessBuilderSettingsPanel /></Box>
      </Drawer>
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
