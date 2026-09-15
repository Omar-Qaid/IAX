import { Box, FormControlLabel, MenuItem, Radio, RadioGroup, TextField, Typography } from '@mui/material';
import type { SysBackgroundJobRecord as Job } from '../api/sysBackgroundJobApi';

type Unit = 'minutes' | 'hours' | 'days' | 'weeks' | 'months' | 'years';
type EndMode = 'none' | 'count' | 'date';
interface RecurrenceSpec { unit: Unit; interval: number; endAfter?: number; endBy?: string }
const units: { label: string; unit: Unit; seconds?: number }[] = [
  { label: 'Minutes', unit: 'minutes', seconds: 60 },
  { label: 'Hours', unit: 'hours', seconds: 3_600 },
  { label: 'Days', unit: 'days', seconds: 86_400 },
  { label: 'Weeks', unit: 'weeks', seconds: 604_800 },
  { label: 'Months', unit: 'months' },
  { label: 'Years', unit: 'years' },
];
const timeZones = [
  { id: 0, label: '(GMT+00:00) UTC' },
  { id: 1, label: '(GMT+03:00) Kuwait, Riyadh' },
  { id: 2, label: '(GMT+04:00) Abu Dhabi, Muscat' },
  { id: 3, label: '(GMT+02:00) Cairo' },
  { id: 4, label: '(GMT+00:00) London' },
  { id: 5, label: '(GMT-05:00) Eastern Time' },
];

const readSpec = (value: string | null): RecurrenceSpec => {
  try {
    const parsed = JSON.parse(value ?? '') as Partial<RecurrenceSpec>;
    if (units.some((item) => item.unit === parsed.unit) && Number(parsed.interval) > 0)
      return { unit: parsed.unit!, interval: Number(parsed.interval),
        ...(parsed.endAfter ? { endAfter: Number(parsed.endAfter) } : {}),
        ...(parsed.endBy ? { endBy: parsed.endBy } : {}) };
  } catch { /* Legacy interval in seconds. */ }
  const seconds = Math.max(1, Number(value) || 60);
  const selected = [...units].reverse().find((item) => item.seconds && seconds % item.seconds === 0)
    ?? units[0];
  return { unit: selected.unit, interval: seconds / (selected.seconds ?? 1) };
};
const writeSpec = (spec: RecurrenceSpec) => {
  const unit = units.find((item) => item.unit === spec.unit);
  // Keep the established numeric format for the common interval-only schedules.
  // This remains compatible with existing workers while richer schedules use JSON.
  if (unit?.seconds && !spec.endAfter && !spec.endBy)
    return String(spec.interval * unit.seconds);
  return JSON.stringify(spec);
};

export function BatchJobRecurrenceForm({ job, disabled, onChange }: {
  job: Job; disabled: boolean; onChange: (job: Job) => void;
}) {
  const spec = readSpec(job.recurrenceData);
  const recurring = job.scheduleType === 2;
  const startValue = job.startDateTime ?? job.origStartDateTime;
  const start = startValue ? new Date(startValue).toISOString() : '';
  const endMode: EndMode = spec.endAfter ? 'count' : spec.endBy ? 'date' : 'none';
  const label = (text: string) => <Typography sx={{ fontSize: 11, mb: '3px' }}>{text}</Typography>;
  const updateSpec = (next: RecurrenceSpec) => onChange({ ...job, scheduleType: 2, recurrenceData: writeSpec(next) });
  const changeStart = (date: string, time: string) => {
    const value = date && time ? new Date(`${date}T${time}Z`).toISOString() : null;
    onChange({ ...job, startDateTime: value, origStartDateTime: value });
  };
  const changeEndMode = (mode: EndMode) => updateSpec({ unit: spec.unit, interval: spec.interval,
    ...(mode === 'count' ? { endAfter: spec.endAfter ?? 1 } : {}),
    ...(mode === 'date' ? { endBy: spec.endBy ?? new Date().toISOString().slice(0, 10) } : {}) });

  return <Box sx={{ '& .MuiInputBase-root': { fontSize: 12 },
    '& .MuiFormControlLabel-label': { fontSize: 12 }, '& .MuiRadio-root': { p: '5px' } }}>
    <Typography sx={{ fontSize: 12, color: 'text.secondary', mb: 2 }}>Standard view</Typography>
    <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1, maxWidth: 300 }}>
      <Box>{label('Start date (UTC)')}<TextField fullWidth type="date" size="small" disabled={disabled}
        value={start.slice(0, 10)} slotProps={{ htmlInput: { 'aria-label': 'Start date (UTC)' } }}
        onChange={(event) => changeStart(event.target.value, start.slice(11, 19) || '00:00:00')} /></Box>
      <Box>{label('Start time (UTC)')}<TextField fullWidth type="time" size="small" disabled={disabled}
        value={start.slice(11, 19)} slotProps={{ htmlInput: { step: 1, 'aria-label': 'Start time (UTC)' } }}
        onChange={(event) => changeStart(start.slice(0, 10), event.target.value)} /></Box>
    </Box>
    <Box sx={{ mt: 1.5, maxWidth: 300 }}>{label('Time zone')}
      <TextField select fullWidth size="small" disabled={disabled}
        value={job.startDateTimeTzId ?? 0}
        onChange={(event) => onChange({ ...job, startDateTimeTzId: Number(event.target.value) })}>
        {timeZones.map((zone) => <MenuItem key={zone.id} value={zone.id}>{zone.label}</MenuItem>)}
      </TextField>
    </Box>
    <RadioGroup value={endMode} aria-label="Recurrence end" sx={{ my: 1 }}
      onChange={(event) => changeEndMode(event.target.value as EndMode)}>
      <FormControlLabel value="none" control={<Radio size="small" />} label="NO END DATE" disabled={disabled || !recurring} />
      <FormControlLabel value="count" control={<Radio size="small" />} label="END AFTER" disabled={disabled || !recurring} />
      <TextField size="small" type="number" disabled={disabled || !recurring || endMode !== 'count'}
        value={spec.endAfter ?? 1} sx={{ ml: 2, width: 88 }}
        slotProps={{ htmlInput: { min: 1, step: 1, 'aria-label': 'End after count' } }}
        onChange={(event) => updateSpec({ unit: spec.unit, interval: spec.interval,
          endAfter: Math.max(1, Number(event.target.value) || 1) })} />
      <FormControlLabel value="date" control={<Radio size="small" />} label="END BY" disabled={disabled || !recurring} />
      <TextField size="small" type="date" disabled={disabled || !recurring || endMode !== 'date'}
        value={spec.endBy?.slice(0, 10) ?? ''} sx={{ ml: 2, width: 150 }}
        slotProps={{ htmlInput: { 'aria-label': 'End by date' } }}
        onChange={(event) => updateSpec({ unit: spec.unit, interval: spec.interval,
          endBy: event.target.value })} />
    </RadioGroup>
    <Typography sx={{ fontSize: 11, fontWeight: 600, mt: 2 }}>RECURRENCE PATTERN</Typography>
    <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mt: 1 }}>
      <RadioGroup aria-label="Recurrence pattern"
        value={recurring ? spec.unit : String(job.scheduleType)}
        onChange={(event) => {
          if (event.target.value === '0' || event.target.value === '3')
            onChange({ ...job, scheduleType: Number(event.target.value) as Job['scheduleType'],
              recurrenceData: event.target.value === '3' ? '* * * * *' : null });
          else updateSpec({ unit: event.target.value as Unit, interval: 1 });
        }}>
        {units.map((item) => <FormControlLabel key={item.unit} value={item.unit}
          control={<Radio size="small" />} label={item.label} disabled={disabled} />)}
        <FormControlLabel value="0" control={<Radio size="small" />} label="One time" disabled={disabled} />
        <FormControlLabel value="3" control={<Radio size="small" />} label="Advanced (CRON)" disabled={disabled} />
      </RadioGroup>
      <Box>
        {recurring && <><>{label(`Repeat after specified number of ${spec.unit}`)}</>
          <Box sx={{ mt: 1.5 }}>{label('Count')}<TextField type="number" size="small"
            value={spec.interval} disabled={disabled} sx={{ width: 88 }}
            slotProps={{ htmlInput: { min: 1, step: 1, 'aria-label': 'Recurrence count' } }}
            onChange={(event) => updateSpec({ ...spec, interval: Math.max(1, Number(event.target.value) || 1) })} /></Box></>}
        {job.scheduleType === 3 && <TextField label="CRON expression" value={job.recurrenceData ?? ''}
          disabled={disabled} onChange={(event) => onChange({ ...job, recurrenceData: event.target.value })} />}
      </Box>
    </Box>
    <Typography sx={{ mt: 1.5, fontSize: 11, color: 'text.secondary' }}>
      The scheduler uses UTC. End conditions are applied after successful or failed execution attempts.
    </Typography>
  </Box>;
}
