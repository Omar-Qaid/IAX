import React from 'react';
import { Box, Button } from '@mui/material';
import type { SetupNavigationItem } from './types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface SetupNavigationProps {
  items: SetupNavigationItem[];
  activeId: string;
  onSelect: (id: string) => void;
}

export function SetupNavigation({
  items,
  activeId,
  onSelect,
}: SetupNavigationProps): React.ReactElement {
  const { t } = useAppTranslation();
  return (
    <Box
      component="nav"
      aria-label={t('accessibility.setupSections')}
      sx={{
        width: { xs: '100%', md: 238 },
        flexShrink: 0,
        bgcolor: '#f5f4f3',
        overflowY: 'auto',
        py: 0.5,
      }}
    >
      {items.map((item) => (
        <Button
          key={item.id}
          fullWidth
          aria-current={activeId === item.id ? 'location' : undefined}
          onClick={() => onSelect(item.id)}
          sx={{
            justifyContent: 'flex-start',
            width: 'calc(100% - 12px)',
            minHeight: 39,
            mx: 0.75,
            mb: 0.25,
            px: 1.5,
            py: 0.75,
            borderRadius: 1.25,
            color: activeId === item.id ? 'primary.main' : 'text.primary',
            fontSize: '0.75rem',
            fontWeight: activeId === item.id ? 700 : 400,
            bgcolor: activeId === item.id ? 'action.selected' : 'transparent',
            boxShadow: activeId === item.id ? 'inset 0 0 0 1px currentColor' : 'none',
            transition: 'background-color 120ms ease, color 120ms ease, box-shadow 120ms ease',
            '&:hover': {
              bgcolor: activeId === item.id ? 'action.selected' : 'action.hover',
            },
          }}
        >
          {item.label}
        </Button>
      ))}
    </Box>
  );
}
