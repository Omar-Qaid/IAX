import React from 'react';
import { Chip, type ChipProps } from '@mui/material';
export interface RecordStatusProps extends Omit<ChipProps, 'label' | 'color'> { label: React.ReactNode; status?: 'success' | 'warning' | 'error' | 'info' | 'default' }
export const RecordStatus: React.FC<RecordStatusProps> = ({ label, status = 'default', ...props }) => <Chip label={label} color={status} size="small" variant="outlined" {...props} />;

