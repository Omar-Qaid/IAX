import React from 'react';
import { Box, type BoxProps } from '@mui/material';
export type FormActionsProps = BoxProps;
export const FormActions: React.FC<FormActionsProps> = ({ children, sx, ...props }) => <Box sx={[{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: 1 }, ...(Array.isArray(sx) ? sx : [sx])]} {...props}>{children}</Box>;

