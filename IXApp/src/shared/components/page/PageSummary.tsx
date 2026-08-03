import React from 'react';
import { Box, Typography, type BoxProps } from '@mui/material';
export interface PageSummaryProps extends BoxProps { title?: string; description?: string }
export const PageSummary: React.FC<PageSummaryProps> = ({ title, description, children, sx, ...props }) => <Box sx={[{ p: 1.5, border: 1, borderColor: 'divider', borderRadius: 1 }, ...(Array.isArray(sx) ? sx : [sx])]} {...props}>{title && <Typography component="h2" variant="subtitle1">{title}</Typography>}{description && <Typography variant="body2" color="text.secondary">{description}</Typography>}{children}</Box>;

