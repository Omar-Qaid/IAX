import React from 'react';
import { Box, Typography } from '@mui/material';
export interface FastTabHeaderProps { title: string; actions?: React.ReactNode; summary?: React.ReactNode }
export const FastTabHeader: React.FC<FastTabHeaderProps> = ({ title, actions, summary }) => <Box sx={{ minHeight: 42, px: 1.25, display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1, borderBottom: 1, borderColor: 'divider' }}><Typography component="h2" sx={{ minWidth: 0, fontSize: '0.875rem', fontWeight: 600, overflowWrap: 'anywhere' }}>{title}</Typography>{summary && <Box sx={{ marginInlineStart: 'auto' }}>{summary}</Box>}{actions && <Box sx={{ maxWidth: '100%', overflowX: 'auto' }}>{actions}</Box>}</Box>;

