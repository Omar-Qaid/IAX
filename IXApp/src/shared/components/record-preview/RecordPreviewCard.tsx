import React from 'react';
import { Box, CircularProgress, Paper, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import type { RecordPreviewRow } from './types';

export interface RecordPreviewCardProps {
  rows: RecordPreviewRow[];
  loading?: boolean;
  error?: string | null;
  emptyText?: string;
  onMouseEnter?: () => void;
  onMouseLeave?: () => void;
}

/**
 * D365-style record preview: small grey labels over underlined values, linked
 * (lookup) values in the link colour, and a pointer toward the field.
 */
export function RecordPreviewCard({
  rows,
  loading = false,
  error,
  emptyText,
  onMouseEnter,
  onMouseLeave,
}: RecordPreviewCardProps) {
  return (
    <Paper
      elevation={6}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      sx={{
        position: 'relative',
        px: 1.5,
        pt: 1.25,
        pb: 1.5,
        minWidth: 220,
        maxWidth: 300,
        borderRadius: 1,
        display: 'grid',
        gap: 1.25,
        // Pointer toward the field, below the card (popper is placed on top).
        '&::after': {
          content: '""',
          position: 'absolute',
          bottom: -6,
          insetInlineStart: 18,
          width: 12,
          height: 12,
          bgcolor: 'background.paper',
          transform: 'rotate(45deg)',
          boxShadow: '2px 2px 3px rgba(0,0,0,0.08)',
        },
      }}
    >
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 1 }}>
          <CircularProgress size={18} />
        </Box>
      ) : error ? (
        <Typography sx={{ fontSize: 12, color: 'error.main' }}>{error}</Typography>
      ) : rows.length === 0 ? (
        <Typography sx={{ fontSize: 12, color: 'text.secondary' }}>{emptyText}</Typography>
      ) : (
        rows.map((row) => {
          const text = row.value == null ? '' : String(row.value);
          return (
            <Box key={row.label} sx={{ minWidth: 0 }}>
              <Typography sx={{ fontSize: 11, color: 'text.secondary', lineHeight: 1.4, mb: 0.25 }}>
                {row.label}
              </Typography>
              <Typography
                title={text}
                sx={{
                  fontSize: 13,
                  lineHeight: 1.5,
                  minHeight: 20,
                  px: 0.5,
                  borderBottom: '1px solid',
                  borderColor: 'text.primary',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                  color: row.lookup && text ? d365.primary : 'text.primary',
                }}
              >
                {text}
              </Typography>
            </Box>
          );
        })
      )}
    </Paper>
  );
}
