import React, { useState, useEffect, Suspense, lazy } from 'react';
import {
    Box, Paper, Typography, Button, Chip, Dialog, DialogTitle, DialogContent, DialogActions, Tooltip, Stack, Tabs, Tab, CircularProgress
} from '@mui/material';
import {
    AccountTree, RestartAlt, Download, PlaylistAddCheck, Bolt, TextFields, Visibility, ContentCopy
} from '@mui/icons-material';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { numberSequenceService } from '../../../system/number-sequences/api/numberSequenceService';

import type { ActivityType, ControlType } from './types';
import { uid } from './utils/uid';
import { findControlIdByType } from './utils/ProcessBuilderMappers';
import { ProcessBuilderProvider, useProcessBuilderContext, defaultProcessInfo } from './context/ProcessBuilderContext';
import { useProcessBuilderData } from './hooks/useProcessBuilderData';
import { useProcessBuilderStorage } from './hooks/useProcessBuilderStorage';

// Lazy-load standalone tabs & subcomponents
const ProcessTree = lazy(() => import('./components/tabs/ProcessTree').then(m => ({ default: m.ProcessTree })));
const ActivitiesPalette = lazy(() => import('./components/tabs/ActivitiesPalette').then(m => ({ default: m.ActivitiesPalette })));
const DesignerTab = lazy(() => import('./components/tabs/DesignerTab').then(m => ({ default: m.DesignerTab })));
const VariablesTab = lazy(() => import('./components/tabs/VariablesTab').then(m => ({ default: m.VariablesTab })));
const StepsTab = lazy(() => import('./components/tabs/StepsTab').then(m => ({ default: m.StepsTab })));
const ActivitiesCenterTab = lazy(() => import('./components/tabs/ActivitiesCenterTab').then(m => ({ default: m.ActivitiesCenterTab })));
const RequestFormTab = lazy(() => import('./components/tabs/RequestFormTab').then(m => ({ default: m.RequestFormTab })));
const ActivityFormTab = lazy(() => import('./components/tabs/ActivityFormTab').then(m => ({ default: m.ActivityFormTab })));
const WorkflowDiagramTab = lazy(() => import('./components/tabs/WorkflowDiagramTab').then(m => ({ default: m.WorkflowDiagramTab })));
const TransitionsTab = lazy(() => import('./components/tabs/TransitionsTab').then(m => ({ default: m.TransitionsTab })));

// Lazy-load properties panels
const ProcessSettingsPanel = lazy(() => import('./components/panels/ProcessSettingsPanel').then(m => ({ default: m.ProcessSettingsPanel })));
const VariableSettingsPanel = lazy(() => import('./components/panels/VariableSettingsPanel').then(m => ({ default: m.VariableSettingsPanel })));
const ActivityControlSettingsPanel = lazy(() => import('./components/panels/ActivityControlSettingsPanel').then(m => ({ default: m.ActivityControlSettingsPanel })));
const RequestControlSettingsPanel = lazy(() => import('./components/panels/RequestControlSettingsPanel').then(m => ({ default: m.RequestControlSettingsPanel })));
const StepSettingsPanel = lazy(() => import('./components/panels/StepSettingsPanel').then(m => ({ default: m.StepSettingsPanel })));
const ActivitySettingsPanel = lazy(() => import('./components/panels/ActivitySettingsPanel').then(m => ({ default: m.ActivitySettingsPanel })));

// =====================================================================
// Main component
// =====================================================================
const ProcessBuilderPageContent: React.FC = () => {
    const { t } = useTranslation();
    const {
        processInfo, setProcessInfo,
        variables, setVariables,
        requestControls, setRequestControls,
        steps, setSteps,
        selectedNode: selected, setSelectedNode: setSelected,
        leftTab, setLeftTab,
        centerTab, setCenterTab,
        expandedSteps, setExpandedSteps,
        addStep,
        addActivity: addActivityContext,
        addControl: addControlContext,
        addRequestControl: addRequestControlContext,
        transitions, setTransitions,
    } = useProcessBuilderContext();

    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const {
        dataTypes,
        activityTypes,
        performers,
        wfControls,
        operators,
        
        stepsSaving,
        variablesSaving,
        requestControlsSaving,
        transitionsSaving,
        
        saveVariablesToBackend,
        saveStepsToBackend,
        saveRequestControlsToBackend: saveRequestControlsToBackendRaw,
        saveActivityToBackend,
        saveTransitionsToBackend,
    } = useProcessBuilderData(processInfo.id, isEditMode);

    // Auto-fill Code with the next sequence preview
    useEffect(() => {
        if (isEditMode) return;
        numberSequenceService.peek('WfProcess')
            .then((result) => {
                setProcessInfo((p) => p.code ? p : { ...p, code: result.code });
            })
            .catch(() => { /* leave blank if no sequence is configured */ });
    }, [isEditMode, setProcessInfo]);

    // --- Control CRUD & Action Wrappers ---
    const addActivity = async (stepId: string, type: ActivityType) => {
        let code = '';
        try {
            const seq = await numberSequenceService.next('WfActivity');
            if (seq && seq.code) code = seq.code;
        } catch { /* ignore */ }
        addActivityContext(stepId, type, code);
    };

    const addControl = (stepId: string, activityId: string, type: ControlType) => {
        addControlContext(stepId, activityId, type);
    };

    const addRequestControl = (type: ControlType) => {
        const resolvedControlId = findControlIdByType(type, wfControls);
        addRequestControlContext(type, undefined, resolvedControlId);
    };

    const saveRequestControlsToBackend = () => saveRequestControlsToBackendRaw(wfControls);

    // =====================================================================
    // LocalStorage Sync
    // =====================================================================
    const [saveDialogOpen, setSaveDialogOpen] = useState(false);

    const storageData = {
        processInfo, variables, requestControls, steps,
        selectedNode: selected, leftTab, centerTab, expandedSteps, transitions
    };
    const storageSetters = {
        setProcessInfo, setVariables, setRequestControls, setSteps,
        setSelectedNode: setSelected, setLeftTab, setCenterTab, setExpandedSteps, setTransitions
    };
    
    const { savedAt, buildPayload, resetStorage } = useProcessBuilderStorage(
        routeId, isEditMode, storageData, storageSetters
    );

    const handleCopy = () => {
        navigator.clipboard.writeText(JSON.stringify(buildPayload(), null, 2));
    };

    const handleDownload = () => {
        const blob = new Blob([JSON.stringify(buildPayload(), null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${processInfo.code || 'process'}.json`;
        a.click();
        URL.revokeObjectURL(url);
    };

    const handleReset = () => {
        if (!confirm(t('workflow.confirm_reset', 'Discard all changes and reset the builder?'))) return;
        resetStorage();
        setProcessInfo(defaultProcessInfo);
        setVariables([]);
        setRequestControls([]);
        setSteps([{
            id: uid(), name: 'Step 1', order: 1, status: 'pending',
            assignedUsers: '', assignedRoles: '', activities: [],
        }]);
        setSelected({ kind: 'process' });
    };

    const totals = {
        steps: steps.length,
        activities: steps.reduce((n, s) => n + s.activities.length, 0),
        activityControls: steps.reduce((n, s) =>
            n + s.activities.reduce((m, a) => m + a.controls.length, 0), 0),
        variables: variables.length,
        requestControls: requestControls.length,
        transitions: transitions.length,
    };

    const renderRightPanel = () => {
        return (
            <Suspense fallback={<Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress size={24} /></Box>}>
                {(() => {
                    if (selected.kind === 'process') return <ProcessSettingsPanel />;
                    if (selected.kind === 'variable') return <VariableSettingsPanel />;
                    if (selected.kind === 'control') return <ActivityControlSettingsPanel />;
                    if (selected.kind === 'requestControl') return <RequestControlSettingsPanel />;
                    if (selected.kind === 'step') return <StepSettingsPanel />;
                    if (selected.kind === 'activity') return <ActivitySettingsPanel />;
                    return (
                        <Box sx={{ p: 4, textAlign: 'center', color: 'text.secondary' }}>
                            <Typography>{t('workflow.select_node_view_settings', 'Select a node to view settings')}</Typography>
                        </Box>
                    );
                })()}
            </Suspense>
        );
    };

    const renderLeftPanel = () => {
        return (
            <Suspense fallback={<Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}><CircularProgress size={24} /></Box>}>
                {leftTab === 0 ? (
                    <ProcessTree setCenterTab={setCenterTab} />
                ) : (
                    <ActivitiesPalette addActivity={addActivity} addControl={addControl} addRequestControl={addRequestControl} setCenterTab={setCenterTab} />
                )}
            </Suspense>
        );
    };

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
            {/* Top bar */}
            <Paper square sx={{ p: 1, display: 'flex', alignItems: 'center', gap: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
                <AccountTree color="primary" />
                <Typography variant="h6">{t('workflow.process_builder', 'Process Builder')}</Typography>
                <Chip size="small"
                    label={processInfo.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                    color={processInfo.isActive ? 'success' : 'default'} />
                {processInfo.id && <Chip size="small" variant="outlined" label={`#${processInfo.id}`} />}
                {processInfo.code && <Chip size="small" variant="outlined" label={processInfo.code} />}
                <Box sx={{ flexGrow: 1 }} />
                {(() => {
                    const ts = Object.values(savedAt).sort().pop();
                    return ts ? (
                        <Stack direction="row" alignItems="center" spacing={0.5}>
                            <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: 'success.main' }} />
                            <Typography variant="caption" color="text.secondary">
                                {t('workflow.auto_saved', 'Auto-saved')} {new Date(ts).toLocaleTimeString()}
                            </Typography>
                        </Stack>
                    ) : (
                        <Typography variant="caption" color="text.secondary">{t('workflow.auto_save_on', 'Auto-save on')}</Typography>
                    );
                })()}
                <Tooltip title={
                    `${t('workflow.steps', 'Steps')}: ${totals.steps} | ${t('workflow.activities', 'Activities')}: ${totals.activities} | ` +
                    `${t('workflow.activity_controls', 'Activity controls')}: ${totals.activityControls} | ` +
                    `${t('workflow.variables', 'Variables')}: ${totals.variables} | ${t('workflow.request_controls', 'Request controls')}: ${totals.requestControls} | ` +
                    `${t('workflow.transitions', 'Transitions')}: ${totals.transitions}`
                }>
                    <Chip size="small" icon={<PlaylistAddCheck />}
                        label={`${totals.steps}${t('workflow.step_short', 'S')} / ${totals.activities}${t('workflow.activity_short', 'A')} / ${totals.activityControls}${t('workflow.control_short', 'C')} / ${totals.transitions}${t('workflow.transition_short', 'T')}`} />
                </Tooltip>
                <Button startIcon={<RestartAlt />} size="small" color="warning" onClick={handleReset}>{t('common.reset', 'Reset')}</Button>
                <Button startIcon={<Download />} variant="contained" onClick={() => setSaveDialogOpen(true)}>{t('common.export', 'Export')}</Button>
            </Paper>

            <Dialog open={saveDialogOpen} onClose={() => setSaveDialogOpen(false)} maxWidth="md" fullWidth>
                <DialogTitle>
                    {t('workflow.export_process', 'Export Process')}
                    <Typography variant="caption" display="block" color="text.secondary">
                        {t('workflow.export_description', 'Snapshot of all sections currently in the builder. Each section is also saved independently to localStorage.')}
                    </Typography>
                </DialogTitle>
                <DialogContent dividers>
                    <Stack direction="row" spacing={1} sx={{ mb: 1, flexWrap: 'wrap' }}>
                        <Chip size="small" label={`${totals.steps} ${t('workflow.steps', 'steps')}`} />
                        <Chip size="small" label={`${totals.activities} ${t('workflow.activities', 'activities')}`} />
                        <Chip size="small" label={`${totals.activityControls} ${t('workflow.activity_controls', 'activity controls')}`} />
                        <Chip size="small" label={`${totals.variables} ${t('workflow.variables', 'variables')}`} />
                        <Chip size="small" label={`${totals.requestControls} ${t('workflow.request_controls', 'request controls')}`} />
                        <Chip size="small" label={`${totals.transitions} ${t('workflow.transitions', 'transitions')}`} />
                    </Stack>
                    <Box component="pre" sx={{
                        bgcolor: 'grey.100', p: 2, borderRadius: 1,
                        maxHeight: 400, overflow: 'auto',
                        fontSize: 12, fontFamily: 'monospace',
                    }}>
                        {JSON.stringify(buildPayload(), null, 2)}
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button startIcon={<ContentCopy />} onClick={handleCopy}>{t('workflow.copy_json', 'Copy JSON')}</Button>
                    <Button startIcon={<Download />} onClick={handleDownload}>{t('common.download', 'Download')}</Button>
                    <Button variant="contained" onClick={() => setSaveDialogOpen(false)}>{t('common.close', 'Close')}</Button>
                </DialogActions>
            </Dialog>

            {/* 3-panel layout */}
            <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, flexGrow: 1, minHeight: 0, overflow: { xs: 'auto', md: 'hidden' } }}>
                {/* Left panel */}
                <Paper square sx={{ width: { xs: '100%', md: 280 }, borderRight: { md: '1px solid' }, borderBottom: { xs: '1px solid', md: 'none' }, borderColor: 'divider', overflow: 'auto', flexShrink: 0 }}>
                    <Tabs value={leftTab} onChange={(_, v) => setLeftTab(v)} variant="fullWidth">
                        <Tab label={t('workflow.tree', 'Tree')} />
                        <Tab label={t('workflow.palette', 'Palette')} />
                    </Tabs>
                    {renderLeftPanel()}
                </Paper>

                {/* Center panel */}
                <Box sx={{ flexGrow: 1, overflow: 'auto', bgcolor: 'grey.50', minHeight: { xs: 500, md: 0 } }}>
                    <Tabs value={centerTab} onChange={(_, v) => setCenterTab(v)}
                        variant="scrollable" scrollButtons="auto"
                        sx={{ borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'background.paper' }}>
                        <Tab icon={<Bolt fontSize="small" />} iconPosition="start" label={t('workflow.designer_tab', 'Designer')} />
                        <Tab icon={<PlaylistAddCheck fontSize="small" />} iconPosition="start" label={t('workflow.variables_tab', 'Variables')} />
                        <Tab icon={<AccountTree fontSize="small" />} iconPosition="start" label={t('workflow.steps_tab', 'Steps')} />
                        <Tab icon={<Bolt fontSize="small" />} iconPosition="start" label={t('workflow.activities_tab', 'Activities')} />
                        <Tab icon={<TextFields fontSize="small" />} iconPosition="start" label={t('workflow.request_form_tab', 'Request Form')} />
                        <Tab icon={<Visibility fontSize="small" />} iconPosition="start" label={t('workflow.activity_form_tab', 'Activity Form')} />
                        <Tab icon={<AccountTree fontSize="small" />} iconPosition="start" label={t('workflow.diagram', 'Diagram')} />
                        <Tab icon={<Bolt fontSize="small" />} iconPosition="start" label={t('workflow.transitions_tab', 'Transitions')} />
                    </Tabs>
                    <Suspense fallback={<Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}><CircularProgress size={32} /></Box>}>
                        {centerTab === 0 && (
                            <DesignerTab
                                addStep={addStep}
                                addActivity={addActivity}
                                activityTypes={activityTypes}
                                performers={performers}
                            />
                        )}
                        {centerTab === 1 && (
                            <VariablesTab
                                dataTypes={dataTypes}
                                variablesSaving={variablesSaving}
                                saveVariablesToBackend={saveVariablesToBackend}
                            />
                        )}
                        {centerTab === 2 && (
                            <StepsTab
                                stepsSaving={stepsSaving}
                                saveStepsToBackend={saveStepsToBackend}
                                setCenterTab={setCenterTab}
                            />
                        )}
                        {centerTab === 3 && (
                            <ActivitiesCenterTab
                                activityTypes={activityTypes}
                                performers={performers}
                                addActivity={addActivity}
                                saveActivityToBackend={saveActivityToBackend}
                            />
                        )}
                        {centerTab === 4 && (
                            <RequestFormTab
                                requestControlsSaving={requestControlsSaving}
                                saveRequestControlsToBackend={saveRequestControlsToBackend}
                                addRequestControl={addRequestControl}
                            />
                        )}
                        {centerTab === 5 && (
                            <ActivityFormTab
                                addControl={addControl}
                                saveActivityToBackend={saveActivityToBackend}
                            />
                        )}
                        {centerTab === 6 && (
                            <WorkflowDiagramTab />
                        )}
                        {centerTab === 7 && (
                            <TransitionsTab
                                operators={operators}
                                transitionsSaving={transitionsSaving}
                                saveTransitionsToBackend={saveTransitionsToBackend}
                            />
                        )}
                    </Suspense>
                </Box>

                {/* Right panel */}
                <Paper square sx={{ width: { xs: '100%', md: 360 }, borderLeft: { md: '1px solid' }, borderTop: { xs: '1px solid', md: 'none' }, borderColor: 'divider', overflow: 'auto', flexShrink: 0 }}>
                    {renderRightPanel()}
                </Paper>
            </Box>
        </Box>
    );
};

const ProcessBuilderPage: React.FC = () => (
    <ProcessBuilderProvider>
        <ProcessBuilderPageContent />
    </ProcessBuilderProvider>
);
export default ProcessBuilderPage;
