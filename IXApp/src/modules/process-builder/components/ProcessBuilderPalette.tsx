import React from 'react';
import Api from '@mui/icons-material/Api';
import ArrowDropDownCircle from '@mui/icons-material/ArrowDropDownCircle';
import CalendarMonth from '@mui/icons-material/CalendarMonth';
import CheckBox from '@mui/icons-material/CheckBox';
import CloudUpload from '@mui/icons-material/CloudUpload';
import Draw from '@mui/icons-material/Draw';
import Edit from '@mui/icons-material/Edit';
import GridOn from '@mui/icons-material/GridOn';
import HowToReg from '@mui/icons-material/HowToReg';
import Notifications from '@mui/icons-material/Notifications';
import RateReview from '@mui/icons-material/RateReview';
import Search from '@mui/icons-material/Search';
import TextFields from '@mui/icons-material/TextFields';
import { Box, Button, Chip, Divider, Stack, Typography } from '@mui/material';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import type { BuilderActivityType, BuilderControlType } from '../types/processBuilderTypes';
import { processBuilderTokens as tokens } from './processBuilderTokens';

export const activityPalette: ReadonlyArray<{ type: BuilderActivityType; label: string; icon: React.ReactNode }> = [
  { type: 'approval', label: 'Approval', icon: <HowToReg fontSize="small" /> },
  { type: 'review', label: 'Review', icon: <RateReview fontSize="small" /> },
  { type: 'data-entry', label: 'Data Entry', icon: <Edit fontSize="small" /> },
  { type: 'api', label: 'API Action', icon: <Api fontSize="small" /> },
  { type: 'notification', label: 'Notification', icon: <Notifications fontSize="small" /> },
];

export const controlPalette: ReadonlyArray<{ type: BuilderControlType; label: string; icon: React.ReactNode }> = [
  { type: 'digits', label: 'Digits', icon: <TextFields fontSize="small" /> },
  { type: 'text', label: 'Text', icon: <TextFields fontSize="small" /> },
  { type: 'longtext', label: 'Long text', icon: <TextFields fontSize="small" /> },
  { type: 'date', label: 'Date', icon: <CalendarMonth fontSize="small" /> },
  { type: 'time', label: 'Time', icon: <CalendarMonth fontSize="small" /> },
  { type: 'url', label: 'Url', icon: <Search fontSize="small" /> },
  { type: 'dropdown-db', label: 'Drop Down List (Fill From DataBase)', icon: <ArrowDropDownCircle fontSize="small" /> },
  { type: 'dropdown-manual', label: 'Drop Down List (Fill Manually)', icon: <ArrowDropDownCircle fontSize="small" /> },
  { type: 'checkbox', label: 'Check box', icon: <CheckBox fontSize="small" /> },
  { type: 'checkboxlist', label: 'Check Box List', icon: <CheckBox fontSize="small" /> },
  { type: 'radiobuttonlist', label: 'Radio Button List', icon: <CheckBox fontSize="small" /> },
  { type: 'table', label: 'Table', icon: <GridOn fontSize="small" /> },
  { type: 'label', label: 'Label', icon: <TextFields fontSize="small" /> },
  { type: 'employeesearch', label: 'EmployeeSearch', icon: <HowToReg fontSize="small" /> },
  { type: 'employeeid', label: 'Employee ID', icon: <HowToReg fontSize="small" /> },
  { type: 'file', label: 'File', icon: <CloudUpload fontSize="small" /> },
  { type: 'showroom', label: 'ShowRoom', icon: <Search fontSize="small" /> },
  { type: 'signature', label: 'Signature', icon: <Draw fontSize="small" /> },
  { type: 'location', label: 'Location', icon: <Search fontSize="small" /> },
  { type: 'advertiser', label: 'Advertiser', icon: <Search fontSize="small" /> },
];

export function ProcessBuilderPalette() {
  const store = useProcessBuilderStore();
  const selectedStep = store.selected.kind === 'step' ? store.selected.id : store.selected.kind === 'activity' ? store.selected.stepId : store.document.steps[0]?.id;
  const selectedActivity = store.selected.kind === 'activity' ? store.selected : null;
  const activityControlMode = store.centerTab === 5 && selectedActivity;
  const titleSx = { fontSize: tokens.fontSize.secondary, lineHeight: 1.2, fontWeight: 700, color: tokens.text };
  const paletteButtonSx = {
    minHeight: '44px !important',
    px: '12px',
    justifyContent: 'flex-start',
    borderColor: '#dfe4eb !important',
    bgcolor: '#fff',
    boxShadow: '0 1px 2px rgb(15 23 42 / 6%)',
    color: '#1f2937 !important',
    '& .MuiButton-startIcon': { color: '#111827', mr: '10px' },
    '&:hover': { bgcolor: tokens.accentSoft, borderColor: `${tokens.accentLight} !important` },
    '&:focus-visible': { boxShadow: tokens.focusRing },
  };
  const addLabel = (
    <Typography component="span" sx={{ ml: 'auto', pl: 1, fontSize: tokens.fontSize.caption, color: tokens.accent }}>
      Add
    </Typography>
  );
  return (
    <Stack spacing="8px" sx={{ p: '8px', pb: '20px' }}>
      <Typography sx={{ ...titleSx, py: '7px' }}>ACTIVITY TYPES (click a step, then add)</Typography>
      {activityPalette.map((item) => (
        <Button
          key={item.type}
          variant="outlined"
          startIcon={item.icon}
          disabled={!selectedStep}
          onClick={() => selectedStep && store.addActivity(selectedStep, item.type)}
          sx={paletteButtonSx}
        >
          {item.label}
          {addLabel}
        </Button>
      ))}
      <Divider sx={{ my: '8px !important' }} />
      <Stack direction="row" sx={{ alignItems: 'center', minHeight: 24 }}>
        <Typography sx={{ ...titleSx, flex: 1 }}>CONTROLS</Typography>
        <Chip
          variant="outlined"
          size="small"
          label={activityControlMode ? 'Activity Form' : 'Request Form (process-level)'}
          sx={{ height: 24, maxWidth: 190, fontSize: tokens.fontSize.caption, bgcolor: '#fff' }}
        />
      </Stack>
      <Box role="status" sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption, lineHeight: 1.55, pb: '2px' }}>
        {activityControlMode
          ? 'Adds a control to the selected activity form.'
          : 'Adds as a process-level Request Control. Select an activity to add there instead.'}
      </Box>
      {store.centerTab === 5 && !selectedActivity && (
        <Box sx={{ p: '8px', color: '#9a6700', bgcolor: '#fff7db', border: '1px solid #f4d06f', fontSize: tokens.fontSize.caption }}>
          Select an activity from the Tree or Activities workspace to enable activity-form controls.
        </Box>
      )}
      {controlPalette.map((item) => (
        <Button
          key={item.type}
          variant="outlined"
          startIcon={item.icon}
          disabled={Boolean(store.centerTab === 5 && !selectedActivity)}
          onClick={() => {
            if (activityControlMode) {
              store.addActivityControl(selectedActivity.stepId, selectedActivity.id, item.type);
            } else {
              store.addRequestControl(item.type);
            }
          }}
          sx={paletteButtonSx}
        >
          {item.label}
          {addLabel}
        </Button>
      ))}
    </Stack>
  );
}
