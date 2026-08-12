import React from 'react';
import { Box, IconButton, Paper, Typography } from '@mui/material';
import DeleteOutlined from '@mui/icons-material/DeleteOutlined';
export function BuilderItemCard({ title, subtitle, selected, onSelect, onDelete, children }: { title: string; subtitle?: string; selected?: boolean; onSelect?: () => void; onDelete?: () => void; children?: React.ReactNode }) {
  return <Paper variant="outlined" sx={{ p: 1.25, borderRadius: 2, borderColor: selected ? '#f59e0b' : '#e5e7eb', bgcolor: '#fff', boxShadow: selected ? '0 2px 10px rgba(245,158,11,.08)' : 'none' }}>
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }} onClick={onSelect} role={onSelect ? 'button' : undefined} tabIndex={onSelect ? 0 : undefined}>
      <Box sx={{ flex: 1, minWidth: 0 }}><Typography sx={{ fontSize: '0.8125rem', fontWeight: 700 }}>{title}</Typography>{subtitle && <Typography color="text.secondary" sx={{ fontSize: '0.6875rem' }}>{subtitle}</Typography>}</Box>
      {onDelete && <IconButton size="small" aria-label={`Delete ${title}`} onClick={(e) => { e.stopPropagation(); onDelete(); }}><DeleteOutlined fontSize="small" /></IconButton>}
    </Box>{children}
  </Paper>;
}
