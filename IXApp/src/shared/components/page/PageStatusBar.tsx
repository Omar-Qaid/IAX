import React from 'react';
import { Box, type BoxProps } from '@mui/material';
export const PageStatusBar: React.FC<BoxProps> = ({ children, sx, ...props }) => <Box role="status" sx={[{ minHeight: 32, px: 1.5, display: 'flex', alignItems: 'center', gap: 2, borderTop: 1, borderColor: 'divider', bgcolor: 'background.paper' }, ...(Array.isArray(sx) ? sx : [sx])]} {...props}>{children}</Box>;

