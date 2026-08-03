import React from 'react';
import { Alert, type AlertProps } from '@mui/material';
export type AppAlertProps = AlertProps;
export const AppAlert: React.FC<AppAlertProps> = (props) => <Alert role="alert" variant="outlined" {...props} />;

