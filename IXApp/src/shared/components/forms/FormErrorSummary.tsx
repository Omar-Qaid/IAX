import React from 'react';
import { Alert, AlertTitle, Typography } from '@mui/material';
export interface FormErrorSummaryProps { errors: Record<string, string> | string[]; title?: string }
export const FormErrorSummary: React.FC<FormErrorSummaryProps> = ({ errors, title }) => { const messages = Array.isArray(errors) ? errors : Object.values(errors); if (!messages.length) return null; return <Alert severity="error" role="alert">{title && <AlertTitle>{title}</AlertTitle>}{messages.map((message, index) => <Typography key={`${message}-${index}`} component="div" variant="body2">{message}</Typography>)}</Alert>; };

