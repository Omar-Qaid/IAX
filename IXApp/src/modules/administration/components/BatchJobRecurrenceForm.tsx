import { useState } from 'react';
import {
  Box,
  FormControlLabel,
  MenuItem,
  Radio,
  RadioGroup,
  TextField,
  Typography,
} from '@mui/material';
import type { SysBackgroundJobRecord as Job } from '../api/sysBackgroundJobApi';

const units = [
  { label: 'Minutes', seconds: 60 },
  { label: 'Hours', seconds: 3600 },
  { label: 'Days', seconds: 86400 },
  { label: 'Weeks', seconds: 604800 },
];

export function BatchJobRecurrenceForm({
  job,
  disabled,
  onChange,
}: {
  job: Job;
  disabled: boolean;
  onChange: (job: Job) => void;
}) {
  const [unit, setUnit] = useState(
    () =>
      [...units]
        .reverse()
        .find((item) => job.intervalSeconds && job.intervalSeconds % item.seconds === 0)?.seconds ??
      60
  );
  const recurring = job.scheduleType === 2;
  const start = job.runAt ? new Date(job.runAt).toISOString() : '';
  const label = (text: string) => <Typography sx={{ fontSize: 11, mb: '3px' }}>{text}</Typography>;
  const changeStart = (date: string, time: string) =>
    onChange({ ...job, runAt: date && time ? new Date(`${date}T${time}Z`).toISOString() : null });
  return (
    <Box
      sx={{
        '& .MuiInputBase-root': { fontSize: 12 },
        '& .MuiFormControlLabel-label': { fontSize: 12 },
        '& .MuiRadio-root': { p: '5px' },
      }}
    >
      <Typography sx={{ fontSize: 12, color: 'text.secondary', mb: 2 }}>Standard view</Typography>
      <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1, maxWidth: 300 }}>
        <Box>
          {label('Start date (UTC)')}
          <TextField
            fullWidth
            type="date"
            size="small"
            disabled={disabled || job.scheduleType >= 2}
            value={start.slice(0, 10)}
            slotProps={{ htmlInput: { 'aria-label': 'Start date (UTC)' } }}
            onChange={(e) => changeStart(e.target.value, start.slice(11, 19) || '00:00:00')}
          />
        </Box>
        <Box>
          {label('Start time (UTC)')}
          <TextField
            fullWidth
            type="time"
            size="small"
            disabled={disabled || job.scheduleType >= 2}
            value={start.slice(11, 19)}
            slotProps={{ htmlInput: { step: 1, 'aria-label': 'Start time (UTC)' } }}
            onChange={(e) => changeStart(start.slice(0, 10), e.target.value)}
          />
        </Box>
      </Box>
      <Box sx={{ mt: 1.5, maxWidth: 300 }}>
        {label('Time zone')}
        <TextField select fullWidth size="small" value="UTC" disabled>
          <MenuItem value="UTC">UTC</MenuItem>
        </TextField>
      </Box>
      <RadioGroup value="none" aria-label="Recurrence end" sx={{ my: 1 }}>
        <FormControlLabel
          value="none"
          control={<Radio size="small" />}
          label="NO END DATE"
          disabled={disabled}
        />
        <FormControlLabel
          value="count"
          control={<Radio size="small" />}
          label="END AFTER"
          disabled
        />
        <TextField
          size="small"
          type="number"
          disabled
          value=""
          sx={{ ml: 2, width: 88 }}
          slotProps={{ htmlInput: { 'aria-label': 'End after count' } }}
        />
        <FormControlLabel value="date" control={<Radio size="small" />} label="END BY" disabled />
        <TextField
          size="small"
          type="date"
          disabled
          value=""
          sx={{ ml: 2, width: 150 }}
          slotProps={{ htmlInput: { 'aria-label': 'End by date' } }}
        />
      </RadioGroup>
      <Typography sx={{ fontSize: 11, fontWeight: 600, mt: 2 }}>RECURRENCE PATTERN</Typography>
      <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mt: 1 }}>
        <RadioGroup
          aria-label="Recurrence pattern"
          value={recurring ? String(unit) : String(job.scheduleType)}
          onChange={(e) => {
            const value = Number(e.target.value);
            if (value < 4) onChange({ ...job, scheduleType: value as Job['scheduleType'] });
            else {
              setUnit(value);
              onChange({ ...job, scheduleType: 2, intervalSeconds: value, cronExpression: null });
            }
          }}
        >
          {units.map((item) => (
            <FormControlLabel
              key={item.seconds}
              value={String(item.seconds)}
              control={<Radio size="small" />}
              label={item.label}
              disabled={disabled}
            />
          ))}
          {['Months', 'Years'].map((text) => (
            <FormControlLabel
              key={text}
              value={text}
              control={<Radio size="small" />}
              label={text}
              disabled
            />
          ))}
          <FormControlLabel
            value="0"
            control={<Radio size="small" />}
            label="One time"
            disabled={disabled}
          />
          <FormControlLabel
            value="3"
            control={<Radio size="small" />}
            label="Advanced (CRON)"
            disabled={disabled}
          />
        </RadioGroup>
        <Box>
          {recurring && (
            <>
              {label(
                `Repeat after specified number of ${units.find((item) => item.seconds === unit)?.label.toLowerCase()}`
              )}
              <Box sx={{ mt: 1.5 }}>
                {label('Count')}
                <TextField
                  type="number"
                  size="small"
                  value={(job.intervalSeconds ?? unit) / unit}
                  disabled={disabled}
                  sx={{ width: 88 }}
                  slotProps={{ htmlInput: { min: 1, step: 1, 'aria-label': 'Recurrence count' } }}
                  onChange={(e) =>
                    onChange({ ...job, intervalSeconds: Number(e.target.value) * unit })
                  }
                />
              </Box>
            </>
          )}
          {job.scheduleType === 3 && (
            <TextField
              label="CRON (UTC)"
              value={job.cronExpression ?? ''}
              disabled={disabled}
              onChange={(e) => onChange({ ...job, cronExpression: e.target.value })}
            />
          )}
        </Box>
      </Box>
      <Typography sx={{ mt: 1.5, fontSize: 11, color: 'text.secondary' }}>
        The scheduler uses UTC. Calendar-month/year intervals and end conditions are not supported
        yet. Repeating schedules start from the scheduling time.
      </Typography>
    </Box>
  );
}
