import React from 'react';
import ArrowDropDownCircle from '@mui/icons-material/ArrowDropDownCircle';
import CalendarMonth from '@mui/icons-material/CalendarMonth';
import CheckBox from '@mui/icons-material/CheckBox';
import CloudUpload from '@mui/icons-material/CloudUpload';
import Draw from '@mui/icons-material/Draw';
import GridOn from '@mui/icons-material/GridOn';
import HowToReg from '@mui/icons-material/HowToReg';
import Search from '@mui/icons-material/Search';
import TextFields from '@mui/icons-material/TextFields';
import { Box, Button, Chip, Stack, Typography } from '@mui/material';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import type { BuilderActivityType, BuilderControlType } from '../types/processBuilderTypes';
import { processBuilderTokens as tokens } from './processBuilderTokens';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { TFunction } from 'i18next';

// Kept as a compatibility export for already-loaded Vite modules. Activity types are
// configured in Activity Settings and are no longer rendered in the Palette.
export const activityPalette: ReadonlyArray<{
  type: BuilderActivityType;
  label: string;
  icon: React.ReactNode;
}> = [
  { type: 'approval', label: 'Approval', icon: null },
  { type: 'review', label: 'Review', icon: null },
  { type: 'data-entry', label: 'Data Entry', icon: null },
  { type: 'api', label: 'API Action', icon: null },
  { type: 'notification', label: 'Notification', icon: null },
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

const controlTypeTranslationKeys: Record<BuilderControlType, string> = {
  digits: 'wfProcessBuilder.controlTypes.digits', text: 'wfProcessBuilder.controlTypes.text', longtext: 'wfProcessBuilder.controlTypes.longtext',
  date: 'wfProcessBuilder.controlTypes.date', time: 'wfProcessBuilder.controlTypes.time', url: 'wfProcessBuilder.controlTypes.url',
  'dropdown-db': 'wfProcessBuilder.controlTypes.dropdownDb', 'dropdown-manual': 'wfProcessBuilder.controlTypes.dropdownManual',
  checkbox: 'wfProcessBuilder.controlTypes.checkbox', checkboxlist: 'wfProcessBuilder.controlTypes.checkboxlist', radiobuttonlist: 'wfProcessBuilder.controlTypes.radiobuttonlist',
  table: 'wfProcessBuilder.controlTypes.table', label: 'wfProcessBuilder.controlTypes.label', employeesearch: 'wfProcessBuilder.controlTypes.employeesearch',
  employeeid: 'wfProcessBuilder.controlTypes.employeeid', file: 'wfProcessBuilder.controlTypes.file', showroom: 'wfProcessBuilder.controlTypes.showroom',
  signature: 'wfProcessBuilder.controlTypes.signature', location: 'wfProcessBuilder.controlTypes.location', advertiser: 'wfProcessBuilder.controlTypes.advertiser',
};

export const getControlTypeLabel = (t: TFunction, type: BuilderControlType): string => t(controlTypeTranslationKeys[type]);

export function ProcessBuilderPalette() {
  const { t } = useAppTranslation();
  const store = useProcessBuilderStore();
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
    '& .MuiButton-startIcon': { color: '#111827', marginInlineEnd: '10px', marginInlineStart: 0 },
    '&:hover': { bgcolor: tokens.accentSoft, borderColor: `${tokens.accentLight} !important` },
    '&:focus-visible': { boxShadow: tokens.focusRing },
  };
  const addLabel = (
    <Typography component="span" sx={{ marginInlineStart: 'auto', paddingInlineStart: 1, fontSize: tokens.fontSize.caption, color: tokens.accent }}>
      {t('wfProcessBuilder.actions.add')}
    </Typography>
  );
  return (
    <Stack spacing="8px" sx={{ p: '8px', pb: '20px' }}>
      <Stack direction="row" sx={{ alignItems: 'center', minHeight: 24 }}>
        <Typography sx={{ ...titleSx, flex: 1 }}>{t('wfProcessBuilder.palette.controls')}</Typography>
        <Chip
          variant="outlined"
          size="small"
          label={activityControlMode ? t('wfProcessBuilder.palette.activityForm') : t('wfProcessBuilder.palette.requestForm')}
          sx={{ height: 24, maxWidth: 190, fontSize: tokens.fontSize.caption, bgcolor: '#fff' }}
        />
      </Stack>
      <Box role="status" sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption, lineHeight: 1.55, pb: '2px' }}>
        {activityControlMode
          ? t('wfProcessBuilder.palette.activityHelp')
          : t('wfProcessBuilder.palette.requestHelp')}
      </Box>
      {store.centerTab === 5 && !selectedActivity && (
        <Box sx={{ p: '8px', color: '#9a6700', bgcolor: '#fff7db', border: '1px solid #f4d06f', fontSize: tokens.fontSize.caption }}>
          {t('wfProcessBuilder.palette.selectActivity')}
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
          {getControlTypeLabel(t, item.type)}
          {addLabel}
        </Button>
      ))}
    </Stack>
  );
}
