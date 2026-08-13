import React from 'react';
import { Box, Paper, Typography, type SxProps, type Theme } from '@mui/material';
export interface FormSectionProps { title?: string; actions?: React.ReactNode; children: React.ReactNode; sx?: SxProps<Theme> }
export const FormSection: React.FC<FormSectionProps> = ({ title, actions, children, sx }) => <Paper variant="outlined" sx={[{ borderRadius: 1, overflow: 'hidden' }, ...(Array.isArray(sx) ? sx : [sx])]}>{(title || actions) && <Box sx={{ minHeight: 36, px: 1, display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: 1, borderColor: 'divider' }}>{title && <Typography component="h2" sx={{ fontSize: '0.8125rem', fontWeight: 600 }}>{title}</Typography>}{actions}</Box>}<Box sx={{ p: 1 }}>{children}</Box></Paper>;

