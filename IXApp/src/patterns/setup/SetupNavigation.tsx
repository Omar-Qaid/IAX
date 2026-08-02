import React from 'react';
import { Box, Button } from '@mui/material';
import type { SetupNavigationItem } from './types';

interface SetupNavigationProps {
  items: SetupNavigationItem[];
  activeId: string;
  onSelect: (id: string) => void;
}

export function SetupNavigation({ items, activeId, onSelect }: SetupNavigationProps): React.ReactElement {
  return (
    <Box component="nav" aria-label="Setup sections" sx={{ width: { xs: '100%', md: 238 }, flexShrink: 0, bgcolor: '#f5f4f3', overflowY: 'auto', py: 0.5 }}>
      {items.map((item) => (
        <Button
          key={item.id}
          fullWidth
          onClick={() => onSelect(item.id)}
          sx={{
            position: 'relative', justifyContent: 'flex-start', minHeight: 41, px: 2, py: 0.75,
            borderRadius: 0, color: 'text.primary', fontSize: '0.75rem', fontWeight: activeId === item.id ? 500 : 400,
            bgcolor: activeId === item.id ? 'background.paper' : 'transparent',
            '&::before': activeId === item.id ? { content: '""', position: 'absolute', insetInlineStart: 6, top: 7, bottom: 7, width: 4, borderRadius: 2, bgcolor: 'primary.main' } : {},
            '&:hover': { bgcolor: activeId === item.id ? 'background.paper' : 'action.hover' },
          }}
        >
          {item.label}
        </Button>
      ))}
    </Box>
  );
}
