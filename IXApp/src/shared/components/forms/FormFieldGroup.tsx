import React from 'react';
import { Box, Typography, type SxProps, type Theme } from '@mui/material';
export interface FormFieldGroupProps { title?: string; children: React.ReactNode; columns?: number; sx?: SxProps<Theme> }
export const FormFieldGroup: React.FC<FormFieldGroupProps> = ({ title, children, columns = 1, sx }) => <Box component="fieldset" sx={[{ m: 0, p: 0, minWidth: 0, border: 0 }, ...(Array.isArray(sx) ? sx : [sx])]}>{title && <Typography component="legend" sx={{ mb: 1, p: 0, fontSize: '0.6875rem', fontWeight: 700, textTransform: 'uppercase' }}>{title}</Typography>}<Box sx={{ display: 'grid', gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))`, gap: 1.25 }}>{children}</Box></Box>;

