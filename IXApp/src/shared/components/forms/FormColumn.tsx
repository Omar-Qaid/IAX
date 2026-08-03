import React from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';
export interface FormColumnProps { children: React.ReactNode; span?: number; sx?: SxProps<Theme> }
export const FormColumn: React.FC<FormColumnProps> = ({ children, span = 1, sx }) => <Box sx={[{ minWidth: 0, gridColumn: `span ${span}` }, ...(Array.isArray(sx) ? sx : [sx])]}>{children}</Box>;

