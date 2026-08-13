import React from 'react';
import { Box, IconButton, Paper, Typography } from '@mui/material';
import DeleteOutlined from '@mui/icons-material/DeleteOutlined';
import { processBuilderTokens as tokens } from './processBuilderTokens';

export function BuilderItemCard({
  title,
  subtitle,
  selected,
  onSelect,
  onDelete,
  children,
}: {
  title: string;
  subtitle?: string;
  selected?: boolean;
  onSelect?: () => void;
  onDelete?: () => void;
  children?: React.ReactNode;
}) {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 1.5,
        borderRadius: `${tokens.radius}px`,
        borderColor: selected ? tokens.warning : tokens.border,
        bgcolor: '#fff',
        boxShadow: 'none',
        transition: 'border-color 120ms ease',
        '&:hover': { borderColor: selected ? tokens.warning : tokens.borderStrong },
      }}
    >
      <Box
        sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
        onClick={onSelect}
        role={onSelect ? 'button' : undefined}
        tabIndex={onSelect ? 0 : undefined}
      >
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography sx={{ fontSize: 10, fontWeight: 500 }}>{title}</Typography>
          {subtitle && (
            <Typography color="text.secondary" sx={{ fontSize: 9 }}>
              {subtitle}
            </Typography>
          )}
        </Box>
        {onDelete && (
          <IconButton
            size="small"
            color="error"
            aria-label={`Delete ${title}`}
            onClick={(event) => {
              event.stopPropagation();
              onDelete();
            }}
          >
            <DeleteOutlined fontSize="small" />
          </IconButton>
        )}
      </Box>
      {children}
    </Paper>
  );
}
