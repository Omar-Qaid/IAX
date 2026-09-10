import { useEffect, useState } from 'react';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAuth } from '@core/auth/useAuth';
import { processBuilderSchedules } from '../api/processBuilderSchedules';
import { Box, Button, Chip, FormControlLabel, MenuItem, Stack, Switch, TextField, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useLocalStorage } from '@shared/hooks/useLocalStorage';
import { processBuilderTokens as tokens } from './processBuilderTokens';
import { processScheduleDraftKey } from '../processScheduleDraft';
import type { BuilderControl } from '../types/processBuilderTypes';

const frequencies = ['daily', 'weekly', 'monthly', 'yearly'] as const;
interface ScheduleDraft {
  version?: string | null;
  enabled: boolean;
  frequency: typeof frequencies[number];
  startsAt: string;
  timeZone: string;
  ownerType: 'systemAdmin' | 'processOwner' | 'employee' | 'showroom';
  ownerReference: string;
  sourceType: 'employee' | 'showroom' | 'table';
  sourceTable: string;
  sourceRecord: string;
  mappings: { targetControlId: string; sourceField: string }[];
}
const createDraft = (): ScheduleDraft => ({
  enabled: false,
  frequency: 'daily',
  startsAt: '',
  timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
  ownerType: 'processOwner', ownerReference: '', sourceType: 'employee', sourceTable: '', sourceRecord: '', mappings: [],
});
const readDraft = (raw: string): ScheduleDraft => {
  const value = JSON.parse(raw) as Partial<ScheduleDraft> | null;
  const defaults = createDraft();
  return {
    version: typeof value?.version === 'string' ? value.version : null,
    enabled: value?.enabled === true,
    frequency: frequencies.includes(value?.frequency as ScheduleDraft['frequency']) ? value!.frequency! : defaults.frequency,
    startsAt: typeof value?.startsAt === 'string' ? value.startsAt : '',
    timeZone: typeof value?.timeZone === 'string' ? value.timeZone : defaults.timeZone,
    ownerType: ['systemAdmin', 'processOwner', 'employee', 'showroom'].includes(value?.ownerType ?? '') ? value!.ownerType! : defaults.ownerType,
    ownerReference: typeof value?.ownerReference === 'string' ? value.ownerReference : '',
    sourceType: ['employee', 'showroom', 'table'].includes(value?.sourceType ?? '') ? value!.sourceType! : defaults.sourceType,
    sourceTable: typeof value?.sourceTable === 'string' ? value.sourceTable : '',
    sourceRecord: typeof value?.sourceRecord === 'string' ? value.sourceRecord : '',
    mappings: Array.isArray(value?.mappings) ? value.mappings.filter((item) => item && typeof item.targetControlId === 'string' && typeof item.sourceField === 'string') : [],
  };
};

/** Local edits are activated only after a successful server save. */
export function ProcessScheduleSettings({ processId, controls = [], onOpen }: { processId: string; controls?: BuilderControl[]; onOpen?: () => void }) {
  const { t } = useAppTranslation();
  const [schedule, setSchedule] = useLocalStorage(processScheduleDraftKey(processId), createDraft, { deserialize: readDraft });
  const update = (patch: Partial<ScheduleDraft>) => { setSaved(false); setSchedule((current) => ({ ...current, ...patch })); };
  const company = useCompanyStore((state) => state.currentCompany);
  const { user } = useAuth();
  const scope = [processId, company, user?.id].join(':');
  const [loadedScope, setLoadedScope] = useState('');
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const persistedProcess = /^\d+$/.test(processId) && Number(processId) > 0;
  useEffect(() => {
    if (!persistedProcess) return;
    const controller = new AbortController();
    processBuilderSchedules.load(processId, controller.signal).then((value) => {
      if (controller.signal.aborted) return;
      if (value) setSchedule({ ...createDraft(), ...value, startsAt: value.startsAt.startsWith('0001-') ? '' : value.startsAt, ownerType: 'processOwner' });
      setLoadedScope(scope);
    }).catch((reason: unknown) => {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'Unable to load the schedule.');
    });
    return () => controller.abort();
  }, [persistedProcess, processId, scope, setSchedule]);
  const save = async () => {
    setSaving(true); setError(''); setSaved(false);
    try {
      const { enabled, frequency, startsAt, timeZone, sourceType, sourceTable, sourceRecord, mappings, version } = schedule;
      const result = await processBuilderSchedules.save(processId, { enabled, frequency, startsAt, timeZone, sourceType, sourceTable, sourceRecord, mappings, version });
      setSchedule((current) => ({ ...current, version: result.version }));
      setSaved(true);
    } catch (reason) { setError(reason instanceof Error ? reason.message : 'Unable to save the schedule.'); }
    finally { setSaving(false); }
  };

  if (onOpen) {
    const summary = [
      t('wfProcessBuilder.settings.schedule.draft'),
      t(`wfProcessBuilder.settings.schedule.${schedule.enabled ? schedule.frequency : 'notEnabled'}`),
      ...(schedule.enabled ? [
        t('wfProcessBuilder.settings.schedule.executionAccountHelp'),
        `${t('wfProcessBuilder.settings.schedule.source')}: ${t(`wfProcessBuilder.settings.schedule.${schedule.sourceType}`)}`,
      ] : []),
    ];
    return (
      <Box sx={{ border: `1px solid ${tokens.border}` }}>
        <Button fullWidth onClick={onOpen} sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}>
          <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>{t('wfProcessBuilder.settings.schedule.title')}</Typography>
        </Button>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, px: 1, pb: 0.75 }}>
          {summary.map((label) => <Chip key={label} size="small" variant="outlined" label={label} title={label} sx={{ height: 22 }} />)}
        </Box>
      </Box>
    );
  }
  return (
    <Box component="fieldset" disabled={saving || persistedProcess && loadedScope !== scope} sx={{ border: `1px solid ${tokens.border}`, p: 1, m: 0, minWidth: 0 }}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <FormControlLabel
          sx={{ m: 0, '& .MuiFormControlLabel-label': { fontSize: tokens.fontSize.body, fontWeight: 700 } }}
          control={<Switch size="small" checked={schedule.enabled} onChange={(_, enabled) => update({ enabled })} />}
          label={t('wfProcessBuilder.settings.schedule.title')}
        />
        <Chip size="small" variant="outlined" label={t('wfProcessBuilder.settings.schedule.draft')} sx={{ height: 22 }} />
      </Stack>
      {schedule.enabled && (
        <Stack spacing="8px" sx={{ mt: 1 }}>
          <TextField fullWidth select size="small" label={t('wfProcessBuilder.settings.schedule.frequency')}
            value={schedule.frequency} onChange={(event) => update({ frequency: event.target.value as ScheduleDraft['frequency'] })}>
            {frequencies.map((frequency) => <MenuItem key={frequency} value={frequency}>{t(`wfProcessBuilder.settings.schedule.${frequency}`)}</MenuItem>)}
          </TextField>
          <TextField fullWidth size="small" type="datetime-local" label={t('wfProcessBuilder.settings.schedule.startsAt')}
            value={schedule.startsAt} onChange={(event) => update({ startsAt: event.target.value })} slotProps={{ inputLabel: { shrink: true } }} />
          <TextField fullWidth size="small" label={t('wfProcessBuilder.settings.schedule.timeZone')}
            value={schedule.timeZone} onChange={(event) => update({ timeZone: event.target.value })} placeholder="Asia/Riyadh" />
          <Typography sx={{ fontSize: tokens.fontSize.caption }}>{t('wfProcessBuilder.settings.schedule.executionAccountHelp')}</Typography>
          <TextField fullWidth select size="small" label={t('wfProcessBuilder.settings.schedule.source')}
            value={schedule.sourceType} onChange={(event) => update({ sourceType: event.target.value as ScheduleDraft['sourceType'], sourceTable: '', sourceRecord: '', mappings: [] })}>
            {(['employee', 'showroom', 'table'] as const).map((type) => <MenuItem key={type} value={type}>{t(`wfProcessBuilder.settings.schedule.${type}`)}</MenuItem>)}
          </TextField>
          {schedule.sourceType === 'table' && (
            <TextField fullWidth size="small" label={t('wfProcessBuilder.settings.schedule.sourceTable')}
              value={schedule.sourceTable} onChange={(event) => update({ sourceTable: event.target.value, mappings: [] })} />
          )}
          <TextField fullWidth size="small" label={t('wfProcessBuilder.settings.schedule.sourceRecord')}
            value={schedule.sourceRecord} onChange={(event) => update({ sourceRecord: event.target.value })} />
          <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>{t('wfProcessBuilder.settings.schedule.mappings')}</Typography>
          {schedule.mappings.map((mapping, index) => (
            <Stack key={index} spacing="6px" sx={{ border: `1px solid ${tokens.border}`, p: 0.75 }}>
              <TextField fullWidth select size="small" label={t('wfProcessBuilder.settings.schedule.targetField')}
                value={mapping.targetControlId} onChange={(event) => update({ mappings: schedule.mappings.map((item, position) => position === index ? { ...item, targetControlId: event.target.value } : item) })}>
                <MenuItem value="">{t('common.none')}</MenuItem>
                {mapping.targetControlId && !controls.some((control) => control.id === mapping.targetControlId) && <MenuItem value={mapping.targetControlId}>{t('wfProcessBuilder.settings.schedule.unavailableField')}</MenuItem>}
                {controls.map((control) => <MenuItem key={control.id} value={control.id} disabled={schedule.mappings.some((item, position) => position !== index && item.targetControlId === control.id)}>{control.label}{control.required ? ' *' : ''}</MenuItem>)}
              </TextField>
              <TextField fullWidth size="small" label={t('wfProcessBuilder.settings.schedule.sourceField')}
                value={mapping.sourceField} onChange={(event) => update({ mappings: schedule.mappings.map((item, position) => position === index ? { ...item, sourceField: event.target.value } : item) })} />
              <Button size="small" color="error" onClick={() => update({ mappings: schedule.mappings.filter((_, position) => position !== index) })}>{t('common.remove')}</Button>
            </Stack>
          ))}
          <Button size="small" variant="outlined" disabled={controls.length === 0 || schedule.mappings.length >= controls.length}
            onClick={() => update({ mappings: [...schedule.mappings, { targetControlId: '', sourceField: '' }] })}>{t('wfProcessBuilder.settings.schedule.addMapping')}</Button>
        </Stack>
      )}
      <Button size="small" variant="contained" onClick={() => void save()} disabled={!persistedProcess || loadedScope !== scope || saving} sx={{ mt: 1 }}>{t('common.save')}</Button>
      {error && <Typography color="error" role="alert">{error}</Typography>}
      {saved && <Typography role="status">{t('wfProcessBuilder.settings.schedule.saved')}</Typography>}
      <Typography sx={{ mt: 0.75, color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
        {t('wfProcessBuilder.settings.schedule.draftHelp')}
      </Typography>
    </Box>
  );
}
