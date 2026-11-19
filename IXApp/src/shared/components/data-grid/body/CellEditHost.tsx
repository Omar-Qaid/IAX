import React from 'react';
import { Box, IconButton } from '@mui/material';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';

/** View cell with a pencil that appears on hover/focus (D365 "edit from view"). */
export function CellEditHost({
  children,
  label,
  onEdit,
}: {
  children: React.ReactNode;
  label: string;
  onEdit: () => void;
}): React.ReactElement {
  return (
    <Box
      sx={{
        position: 'relative',
        width: '100%',
        minWidth: 0,
        display: 'flex',
        alignItems: 'center',
        '& .grid-cell-edit': { opacity: 0 },
        '&:hover .grid-cell-edit, & .grid-cell-edit:focus-visible': { opacity: 1 },
        '&:hover > .grid-cell-value, &:focus-within > .grid-cell-value': {
          paddingRight: '22px',
        },
      }}
    >
      <Box
        className="grid-cell-value"
        sx={{
          flex: 1,
          width: '100%',
          minWidth: 0,
          boxSizing: 'border-box',
          paddingRight: 'var(--grid-cell-edit-space, 0px)',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
      >
        {children}
      </Box>
      <IconButton
        className="grid-cell-edit"
        size="small"
        aria-label={label}
        onClick={(event) => {
          event.stopPropagation();
          onEdit();
        }}
        sx={{
          p: 0.25,
          position: 'absolute',
          right: 0,
          top: '50%',
          transform: 'translateY(-50%)',
          color: 'text.secondary',
          bgcolor: 'transparent',
          '&:hover, &:focus-visible, &.Mui-focusVisible': { bgcolor: 'transparent' },
        }}
      >
        <EditOutlinedIcon sx={{ fontSize: 14 }} />
      </IconButton>
    </Box>
  );
}
