import React from 'react';
import { Box, Typography } from '@mui/material';
export interface FastTabSummaryItem { label?: string; value: React.ReactNode }
export interface FastTabSummaryProps { items: FastTabSummaryItem[] }
export const FastTabSummary: React.FC<FastTabSummaryProps> = ({ items }) => <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>{items.map((item, index) => <Typography key={index} component="span" variant="caption" color="text.secondary">{item.label ? `${item.label}: ` : ''}{item.value}</Typography>)}</Box>;

