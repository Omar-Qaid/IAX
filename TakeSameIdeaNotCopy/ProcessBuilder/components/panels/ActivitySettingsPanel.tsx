import React, { useState, useMemo, useEffect } from 'react';
import {
    Stack, Box, Typography, Chip, TextField, FormControlLabel, Switch,
    FormControl, InputLabel, Select, MenuItem, Button, Divider, Paper, IconButton, Accordion, AccordionSummary, AccordionDetails, CircularProgress, FormHelperText
} from '@mui/material';
import {
    ExpandMore, Settings, Tune, Notifications, AdminPanelSettings,
    AssignmentInd, Bolt, PlaylistAddCheck, Add, Delete, Save
} from '@mui/icons-material';
import { useParams } from 'react-router-dom';
import type { ActivityAction, ValidationRule, ActivityType, ActionType } from '../../types';
import { ACTIVITY_TYPES, ACTION_TYPES } from '../../utils/palette';
import { uid } from '../../utils/uid';
import { DynamicField } from '../../../../../../components/common/DynamicField';
import { useTranslation } from 'react-i18next';
import { ConditionBuilder } from '../shared/ConditionBuilder';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { useProcessBuilderData } from '../../hooks/useProcessBuilderData';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { BilingualField } from '../../../../../../components/common/BilingualField';
import { notificationTemplateService } from '../../../../../notifications/api';
import type { SysNotificationTemplateDto } from '../../../../../../types/notification';

const SectionSaveBar: React.FC<{ sectionKey: string; onSave: () => void; label?: string }> = () => null;

export const ActivitySettingsPanel: React.FC = React.memo(() => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';
    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const {
        processInfo, steps, updateActivity, selectedNode,
        variables, requestControls
    } = useProcessBuilderContext();

    const { performers, activityTypes, saveActivityToBackend } = useProcessBuilderData(processInfo.id, isEditMode, true);

    const [saving, setSaving] = useState(false);

    // Notification templates sourced from the central Notification Center.
    const [notificationTemplates, setNotificationTemplates] = useState<SysNotificationTemplateDto[]>([]);
    useEffect(() => {
        let active = true;
        notificationTemplateService
            .getAll()
            .then((rows) => { if (active) setNotificationTemplates(rows); })
            .catch(() => { /* non-blocking: picker simply stays empty */ });
        return () => { active = false; };
    }, []);

    const allFieldNames = useMemo(() => {
        const names: string[] = variables.map((v) => v.name);
        requestControls.forEach((c) => names.push(c.name));
        steps.forEach((s) => s.activities.forEach((a) => a.controls.forEach((c) => names.push(c.name))));
        return names;
    }, [variables, requestControls, steps]);

    const [showErrors, setShowErrors] = useState(false);

    if (selectedNode.kind !== 'activity') return null;
    const stepId = selectedNode.stepId;
    const step = steps.find(s => s.id === stepId);
    const activity = step?.activities.find(a => a.id === selectedNode.id);
    if (!step || !activity) return null;

    const errors = showErrors ? {
        name: !(isRtl ? activity.nameAR : activity.name)?.trim() ? t('common.required', 'Required') : undefined,
        activityTypeId: !activity.activityTypeId ? t('common.required', 'Required') : undefined,
    } : { name: undefined, activityTypeId: undefined };

    const handleSave = async () => {
        setShowErrors(true);
        const nameOk = !!(isRtl ? activity.nameAR : activity.name)?.trim();
        const typeOk = !!activity.activityTypeId;
        if (!nameOk || !typeOk) return;
        setSaving(true);
        try {
            await saveActivityToBackend(stepId, activity);
        } finally {
            setSaving(false);
        }
    };

    const addAction = () => {
        updateActivity(stepId, activity.id, {
            actions: [...activity.actions, { id: uid(), type: 'approve', label: t('Approve') }],
        });
    };
    const updAction = (id: string, patch: Partial<ActivityAction>) =>
        updateActivity(stepId, activity.id, {
            actions: activity.actions.map((a) => (a.id === id ? { ...a, ...patch } : a)),
        });
    const delAction = (id: string) =>
        updateActivity(stepId, activity.id, {
            actions: activity.actions.filter((a) => a.id !== id),
        });

    const addActivityVal = () =>
        updateActivity(stepId, activity.id, {
            validations: [...activity.validations, { id: uid(), type: 'required', message: t('Required') }],
        });
    const updActivityVal = (id: string, patch: Partial<ValidationRule>) =>
        updateActivity(stepId, activity.id, {
            validations: activity.validations.map((v) => (v.id === id ? { ...v, ...patch } : v)),
        });
    const delActivityVal = (id: string) =>
        updateActivity(stepId, activity.id, {
            validations: activity.validations.filter((v) => v.id !== id),
        });

    return (
        <Stack spacing={2.5} sx={{ p: 2.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Typography variant="h6" sx={{ fontWeight: 700, color: 'text.primary' }}>{t('Activity Settings')}</Typography>
                <Box sx={{ display: 'flex', gap: 1 }}>
                    {activity.serverId ? (
                        <Chip size="small" color="success" label={activity.code || `ID: ${activity.serverId}`} sx={{ borderRadius: 1.5, fontWeight: 600 }} />
                    ) : (
                        <Chip size="small" label={t("New")} sx={{ borderRadius: 1.5, fontWeight: 600 }} />
                    )}
                    {activity.dirty && <Chip size="small" color="warning" label={t("unsaved")} sx={{ borderRadius: 1.5, fontWeight: 600 }} />}
                    <Button
                        variant="contained"
                        size="small"
                        startIcon={saving ? <CircularProgress size={16} color="inherit" /> : <Save fontSize="small" />}
                        onClick={handleSave}
                        disabled={saving}
                        sx={{ ml: 1 }}
                    >
                        {t('Save')}
                    </Button>
                </Box>
            </Box>

            <GeneratableTextField
                label={t("Activity Code")}
                value={activity.code ?? ''}
                onChange={(val) => updateActivity(stepId, activity.id, { code: val })}
                sequenceType="WfActivity"
                required
                disabled={!!activity.serverId}
            />

            <BilingualField
                labelEN={t('Activity Name (EN)')}
                labelAR={t('Activity Name (AR)')}
                nameValue={activity.name}
                nameARValue={activity.nameAR}
                onChange={(name, nameAR) => updateActivity(stepId, activity.id, { name, nameAR })}
                required
                error={!!errors.name}
                errorMessage={errors.name}
            />

            {/* Section 1: General Info */}
            <Accordion variant="outlined" defaultExpanded sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Settings sx={{ mr: 1, fontSize: 20, color: '#6366f1' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('General Information')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ display: 'flex', flexDirection: 'column', gap: 2, p: 2 }}>
                    <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                        <InputLabel>{t('Type')}</InputLabel>
                        <Select label={t("Type")} value={activity.type}
                            onChange={(e) => updateActivity(stepId, activity.id, { type: e.target.value as ActivityType })}>
                            {ACTIVITY_TYPES.map((t) => <MenuItem key={t.type} value={t.type}>{t.label}</MenuItem>)}
                        </Select>
                    </FormControl>
                </AccordionDetails>
            </Accordion>

            {/* Section 2: Performance & SLA */}
            <Accordion variant="outlined" sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Tune sx={{ mr: 1, fontSize: 20, color: '#10b981' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('Performance & SLA')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 2 }}>
                    <Stack spacing={1.5} sx={{ width: '100%' }}>
                        <Stack direction="row" spacing={2} sx={{ width: '100%' }}>
                            <DynamicField
                                config={{ name: 'score', label: t('Score'), type: 'number' }}
                                value={activity.score ?? 0}
                                onChange={(val) => updateActivity(stepId, activity.id, { score: Number(val) || 0 })}
                            />
                            <DynamicField
                                config={{ name: 'autoPassingHrs', label: t('Auto Passing (hrs)'), type: 'number' }}
                                value={activity.autoPassingHrs ?? 0}
                                onChange={(val) => updateActivity(stepId, activity.id, { autoPassingHrs: Number(val) || 0 })}
                            />
                        </Stack>
                        <FormControlLabel
                            sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                            labelPlacement="start"
                            control={<Switch
                                checked={!!activity.autoPassEnabled}
                                onChange={(e) => updateActivity(stepId, activity.id, { autoPassEnabled: e.target.checked })}
                                sx={{
                                    '& .MuiSwitch-switchBase.Mui-checked': { color: '#10b981' },
                                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#10b981' }
                                }} />}
                            label={<Typography variant="body2" sx={{ fontWeight: 500 }}>
                                {t('Auto-pass when SLA hours elapse')}
                            </Typography>} />
                    </Stack>
                </AccordionDetails>
            </Accordion>

            {/* Section 3: Notification & Alerts */}
            <Accordion variant="outlined" sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Notifications sx={{ mr: 1, fontSize: 20, color: '#f59e0b' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('Notification & Alerts')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ display: 'flex', flexDirection: 'column', gap: 2, p: 2 }}>
                    {/* Alert content is now driven by a central Notification Center template */}
                    <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                        <InputLabel>{t('Notification Template')}</InputLabel>
                        <Select
                            label={t('Notification Template')}
                            value={activity.sysNotificationTemplateId != null && activity.sysNotificationTemplateId !== '' ? String(activity.sysNotificationTemplateId) : ''}
                            onChange={(e) => updateActivity(stepId, activity.id, {
                                sysNotificationTemplateId: e.target.value === '' ? '' : Number(e.target.value),
                            })}
                        >
                            <MenuItem value=""><em>({t('none', 'None')})</em></MenuItem>
                            {notificationTemplates.map((tpl) => (
                                <MenuItem key={tpl.id} value={String(tpl.id)}>
                                    {isRtl ? tpl.nameAR || tpl.name : tpl.name} ({tpl.code})
                                </MenuItem>
                            ))}
                        </Select>
                        <FormHelperText>
                            {t('Linked to the Notification Center. Manage templates under Notifications → Templates.')}
                        </FormHelperText>
                    </FormControl>

                    <Box sx={{
                        border: '1px solid',
                        borderColor: 'divider',
                        borderRadius: 2,
                        p: 1.5,
                        bgcolor: 'background.paper',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 1,
                    }}>
                        {[
                            { field: 'alertingBySystem', label: 'Alert by System' },
                            { field: 'alertingByEmail', label: 'Alert by Email' },
                            { field: 'alertingBySms', label: 'Alert by SMS' },
                            { field: 'alertingByWhatsApp', label: 'Alert by WhatsApp' },
                        ].map(({ field, label }) => (
                            <FormControlLabel
                                key={field}
                                sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                                labelPlacement="start"
                                control={<Switch checked={!!(activity as any)[field]}
                                    onChange={(e) => updateActivity(stepId, activity.id, { [field]: e.target.checked } as any)}
                                    sx={{
                                        '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                        '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                    }} />}
                                label={<Typography variant="body2" sx={{ fontWeight: 500 }}>{t(label)}</Typography>} />
                        ))}
                    </Box>
                </AccordionDetails>
            </Accordion>

            {/* Section 4: Behavioral Rules */}
            <Accordion variant="outlined" sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <AdminPanelSettings sx={{ mr: 1, fontSize: 20, color: '#ec4899' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('Behavioral Rules')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 2 }}>
                    <Box sx={{
                        border: '1px solid',
                        borderColor: 'divider',
                        borderRadius: 2,
                        p: 1.5,
                        bgcolor: 'background.paper',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 1,
                    }}>
                        {[
                            { field: 'showPreviousSteps', label: 'Show Previous Steps' },
                            { field: 'showPreviousDocs', label: 'Show Previous Docs' },
                            { field: 'mandatoryDocs', label: 'Mandatory Docs' },
                            { field: 'isActive', label: 'Active' },
                        ].map(({ field, label }) => (
                            <FormControlLabel
                                key={field}
                                sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                                labelPlacement="start"
                                control={<Switch checked={field === 'isActive' ? activity.isActive !== false : !!(activity as any)[field]}
                                    onChange={(e) => updateActivity(stepId, activity.id, { [field]: e.target.checked } as any)}
                                    sx={{
                                        '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                        '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                    }} />}
                                label={<Typography variant="body2" sx={{ fontWeight: 500 }}>{t(label)}</Typography>} />
                        ))}
                    </Box>
                </AccordionDetails>
            </Accordion>

            <Accordion variant="outlined" defaultExpanded sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <AssignmentInd sx={{ mr: 1, fontSize: 20, color: '#3b82f6' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('Assignment Configuration')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 2 }}>
                    <Stack spacing={1.5}>
                        <FormControl size="small" error={!!errors.activityTypeId} sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                            <InputLabel>{t('Activity Type *')}</InputLabel>
                            <Select label={t("Activity Type *")} value={activity.activityTypeId ?? ''}
                                onChange={(e) => updateActivity(stepId, activity.id, { activityTypeId: e.target.value as number })}>
                                <MenuItem value=""><em>({t('select')})</em></MenuItem>
                                {activityTypes.map((type) => (
                                    <MenuItem key={type.id} value={type.id}>{isRtl ? type.nameAR || type.name : type.name} ({type.code})</MenuItem>
                                ))}
                            </Select>
                            {errors.activityTypeId && <FormHelperText>{errors.activityTypeId}</FormHelperText>}
                        </FormControl>
                        <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                            <InputLabel>{t('Performer *')}</InputLabel>
                            <Select label={t("Performer *")} value={activity.performerId ?? ''}
                                onChange={(e) => updateActivity(stepId, activity.id, { performerId: e.target.value as number })}>
                                <MenuItem value=""><em>({t('select')})</em></MenuItem>
                                {performers.map((p) => (
                                    <MenuItem key={p.id} value={p.id}>{isRtl ? p.nameAR || p.name : p.name} ({p.code})</MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                        <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                            <InputLabel>{t('Assignment Mode')}</InputLabel>
                            <Select label={t("Assignment Mode")} value={activity.assignmentMode}
                                onChange={(e) => updateActivity(stepId, activity.id, {
                                    assignmentMode: e.target.value as 'any' | 'all' | 'round-robin'
                                })}>
                                <MenuItem value="any">{t('Any (first to act)')}</MenuItem>
                                <MenuItem value="all">{t('All (everyone must act)')}</MenuItem>
                                <MenuItem value="round-robin">{t('Round-robin')}</MenuItem>
                            </Select>
                        </FormControl>
                    </Stack>
                </AccordionDetails>
            </Accordion>

            <Button variant="contained" startIcon={<PlaylistAddCheck />}
                disabled={saving}
                onClick={handleSave}
                sx={{
                    borderRadius: 2,
                    py: 1.25,
                    textTransform: 'none',
                    fontWeight: 600,
                    boxShadow: 'none',
                    '&:hover': { boxShadow: 'none' }
                }}>
                {saving ? t('Saving Changes…') : t('Save Activity to DB')}
            </Button>

            {activity.type === 'api' && (
                <>
                    <FormControl size="small">
                        <InputLabel>{t('Method')}</InputLabel>
                        <Select label={t("Method")} value={activity.config.apiMethod ?? 'GET'}
                            onChange={(e) => updateActivity(stepId, activity.id, {
                                config: { ...activity.config, apiMethod: e.target.value as any }
                            })}>
                            {['GET', 'POST', 'PUT', 'DELETE'].map((m) => <MenuItem key={m} value={m}>{m}</MenuItem>)}
                        </Select>
                    </FormControl>
                    <TextField label={t("API URL")} size="small" value={activity.config.apiUrl ?? ''}
                        onChange={(e) => updateActivity(stepId, activity.id, {
                            config: { ...activity.config, apiUrl: e.target.value }
                        })} />
                </>
            )}
            {activity.type === 'notification' && (
                <TextField label={t("Notify (emails)")} size="small" value={activity.config.notifyEmails ?? ''}
                    onChange={(e) => updateActivity(stepId, activity.id, {
                        config: { ...activity.config, notifyEmails: e.target.value }
                    })} />
            )}

            <Accordion variant="outlined" defaultExpanded sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Bolt sx={{ mr: 1, fontSize: 20, color: '#8b5cf6' }} />
                        <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('Activity Condition')}</Typography>
                    </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 2 }}>
                    <Stack spacing={1}>
                        <ConditionBuilder group={activity.condition}
                            onChange={(g) => updateActivity(stepId, activity.id, { condition: g })} fields={allFieldNames} />
                        <SectionSaveBar sectionKey={`condition:activity:${activity.id}`}
                            onSave={handleSave} label={t("Save Condition")} />
                    </Stack>
                </AccordionDetails>
            </Accordion>

            <Accordion variant="outlined" sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        {t('Validation Rules')} ({activity.validations.length})
                    </Typography>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 2 }}>
                    <Stack spacing={1}>
                        <Stack direction="row" alignItems="center" spacing={1}>
                            <Button size="small" startIcon={<Add />} onClick={addActivityVal} sx={{ textTransform: 'none' }}>{t('Add Rule')}</Button>
                        </Stack>
                        {activity.validations.map((v) => (
                            <Paper key={v.id} variant="outlined" sx={{ p: 1.5, borderRadius: 1.5 }}>
                                <Stack direction="row" spacing={0.5} alignItems="center">
                                    <FormControl size="small" sx={{ flex: 1 }}>
                                        <Select value={v.type}
                                            onChange={(e) => updActivityVal(v.id, { type: e.target.value as any })}>
                                            {['required', 'min', 'max', 'regex', 'email'].map((t) =>
                                                <MenuItem key={t} value={t}>{t}</MenuItem>)}
                                        </Select>
                                    </FormControl>
                                    <TextField size="small" placeholder={t("Value")} value={v.value ?? ''}
                                        onChange={(e) => updActivityVal(v.id, { value: e.target.value })} sx={{ flex: 1 }} />
                                    <TextField size="small" placeholder={t("Message")} value={v.message}
                                        onChange={(e) => updActivityVal(v.id, { message: e.target.value })} sx={{ flex: 2 }} />
                                    <IconButton size="small" color="error" onClick={() => delActivityVal(v.id)}>
                                        <Delete fontSize="small" />
                                    </IconButton>
                                </Stack>
                            </Paper>
                        ))}
                    </Stack>
                </AccordionDetails>
            </Accordion>

            <Divider />
            <Stack direction="row" alignItems="center">
                <Typography variant="subtitle2" sx={{ flexGrow: 1, fontWeight: 700 }}>{t('Actions')}</Typography>
                <Button size="small" startIcon={<Add />} onClick={addAction} sx={{ textTransform: 'none' }}>{t('Add')}</Button>
            </Stack>
            {activity.actions.map((a) => (
                <Paper key={a.id} variant="outlined" sx={{ p: 1.5, borderRadius: 1.5 }}>
                    <Stack direction="row" spacing={0.5} alignItems="center">
                        <FormControl size="small" sx={{ flex: 1 }}>
                            <Select value={a.type} onChange={(e) => updAction(a.id, { type: e.target.value as ActionType })}>
                                {ACTION_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                            </Select>
                        </FormControl>
                        <TextField size="small" placeholder={t("Label")} value={a.label}
                            onChange={(e) => updAction(a.id, { label: e.target.value })} sx={{ flex: 1 }} />
                        <FormControl size="small" sx={{ flex: 1 }}>
                            <Select displayEmpty value={a.nextStepId ?? ''}
                                onChange={(e) => updAction(a.id, { nextStepId: e.target.value || undefined })}>
                                <MenuItem value=""><em>({t('end')})</em></MenuItem>
                                {steps.filter((s) => s.id !== stepId).map((s) =>
                                    <MenuItem key={s.id} value={s.id}>→ {s.name}</MenuItem>)}
                            </Select>
                        </FormControl>
                        <IconButton size="small" color="error" onClick={() => delAction(a.id)}>
                            <Delete fontSize="small" />
                        </IconButton>
                    </Stack>
                    <Accordion variant="outlined" sx={{ mt: 1, borderRadius: 1 }}>
                        <AccordionSummary expandIcon={<ExpandMore />}>
                            <Typography variant="caption">{t('Action Condition')}</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <ConditionBuilder group={a.condition} onChange={(g) => updAction(a.id, { condition: g })} fields={allFieldNames} />
                        </AccordionDetails>
                    </Accordion>
                </Paper>
            ))}
        </Stack>
    );
});

ActivitySettingsPanel.displayName = 'ActivitySettingsPanel';
