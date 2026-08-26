import React from 'react';
import { Box, Paper, Typography, type SxProps, type Theme } from '@mui/material';
export interface FormSectionProps { title?: string; actions?: React.ReactNode; children: React.ReactNode; sx?: SxProps<Theme> }
export const FormSection: React.FC<FormSectionProps> = ({ title, actions, children, sx }) => <Paper variant="outlined" sx={[{ minWidth: 0, maxWidth: '100%', borderRadius: 1, overflow: 'hidden' }, ...(Array.isArray(sx) ? sx : [sx])]}>{(title || actions) && <Box sx={{ minHeight: 36, px: 1, display: 'flex', flexWrap: 'wrap', gap: 0.5, alignItems: 'center', justifyContent: 'space-between', borderBottom: 1, borderColor: 'divider' }}>{title && <Typography component="h2" sx={{ minWidth: 0, fontSize: '0.8125rem', fontWeight: 600, overflowWrap: 'anywhere' }}>{title}</Typography>}{actions && <Box sx={{ maxWidth: '100%', overflowX: 'auto' }}>{actions}</Box>}</Box>}<Box sx={{ p: { xs: 0.75, sm: 1 }, minWidth: 0 }}>{children}</Box></Paper>;

