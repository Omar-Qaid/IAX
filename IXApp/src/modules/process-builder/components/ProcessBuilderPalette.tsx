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
import { Button, Stack, Typography } from '@mui/material';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import type { BuilderActivityType, BuilderControlType } from '../types/processBuilderTypes';

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
  { type: 'url', label: 'URL', icon: <Search fontSize="small" /> },
  { type: 'dropdown-db', label: 'Database dropdown', icon: <ArrowDropDownCircle fontSize="small" /> },
  { type: 'dropdown-manual', label: 'Manual dropdown', icon: <ArrowDropDownCircle fontSize="small" /> },
  { type: 'checkbox', label: 'Check box', icon: <CheckBox fontSize="small" /> },
  { type: 'checkboxlist', label: 'Checkbox list', icon: <CheckBox fontSize="small" /> },
  { type: 'radiobuttonlist', label: 'Radio button list', icon: <CheckBox fontSize="small" /> },
  { type: 'table', label: 'Table', icon: <GridOn fontSize="small" /> },
  { type: 'label', label: 'Label', icon: <TextFields fontSize="small" /> },
  { type: 'employeesearch', label: 'Employee search', icon: <HowToReg fontSize="small" /> },
  { type: 'employeeid', label: 'Employee ID', icon: <HowToReg fontSize="small" /> },
  { type: 'file', label: 'File', icon: <CloudUpload fontSize="small" /> },
  { type: 'showroom', label: 'Showroom', icon: <Search fontSize="small" /> },
  { type: 'signature', label: 'Signature', icon: <Draw fontSize="small" /> },
  { type: 'location', label: 'Location', icon: <Search fontSize="small" /> },
  { type: 'advertiser', label: 'Advertiser', icon: <Search fontSize="small" /> },
];

export function ProcessBuilderPalette() {
  const store = useProcessBuilderStore();
  const selectedStep = store.selected.kind === 'step' ? store.selected.id : store.selected.kind === 'activity' ? store.selected.stepId : store.document.steps[0]?.id;
  const selectedActivity = store.selected.kind === 'activity' ? store.selected : null;
  const titleSx = { pt: 1, fontSize: '0.6875rem', fontWeight: 700, color: 'text.secondary' };
  return <Stack spacing={1} sx={{ p: 1.5 }}>
    <Typography sx={titleSx}>PROCESS ELEMENTS</Typography>
    <Button variant="outlined" onClick={store.addStep}>Step</Button><Button variant="outlined" onClick={store.addVariable}>Variable</Button>
    <Typography sx={titleSx}>ACTIVITIES</Typography>
    {activityPalette.map((item) => <Button key={item.type} startIcon={item.icon} disabled={!selectedStep} onClick={() => selectedStep && store.addActivity(selectedStep, item.type)} sx={{ justifyContent: 'flex-start' }}>{item.label}</Button>)}
    <Typography sx={titleSx}>REQUEST CONTROLS</Typography>
    {controlPalette.map((item) => <Button key={`request-${item.type}`} startIcon={item.icon} onClick={() => store.addRequestControl(item.type)} sx={{ justifyContent: 'flex-start' }}>{item.label}</Button>)}
    <Typography sx={titleSx}>ACTIVITY CONTROLS</Typography>
    {controlPalette.map((item) => <Button key={`activity-${item.type}`} startIcon={item.icon} disabled={!selectedActivity} onClick={() => selectedActivity && store.addActivityControl(selectedActivity.stepId, selectedActivity.id, item.type)} sx={{ justifyContent: 'flex-start' }}>{item.label}</Button>)}
  </Stack>;
}
