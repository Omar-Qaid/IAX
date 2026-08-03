import React from 'react';
import { AppTextField, type AppTextFieldProps } from './AppTextField';
export interface AppDateTimeFieldProps extends AppTextFieldProps { includeTime?: boolean }
export const AppDateTimeField: React.FC<AppDateTimeFieldProps> = ({ includeTime = true, ...props }) => <AppTextField {...props} type={includeTime ? 'datetime-local' : 'date'} slotProps={{ inputLabel: { shrink: true }, ...props.slotProps }} />;

