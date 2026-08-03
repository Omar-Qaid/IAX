import React from 'react';
import { Box } from '@mui/material';
import { AppTextField } from './AppTextField';
export interface AppBilingualFieldProps { primaryLabel: string; secondaryLabel: string; primaryValue: string; secondaryValue: string; onPrimaryChange?: (value: string) => void; onSecondaryChange?: (value: string) => void; disabled?: boolean }
export const AppBilingualField: React.FC<AppBilingualFieldProps> = ({ primaryLabel, secondaryLabel, primaryValue, secondaryValue, onPrimaryChange, onSecondaryChange, disabled }) => <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 1 }}><AppTextField label={primaryLabel} value={primaryValue} disabled={disabled} onChange={onPrimaryChange} /><AppTextField label={secondaryLabel} value={secondaryValue} disabled={disabled} onChange={onSecondaryChange} slotProps={{ htmlInput: { dir: 'rtl' } }} /></Box>;

