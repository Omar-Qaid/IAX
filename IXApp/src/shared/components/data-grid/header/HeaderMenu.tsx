import React, { useState } from 'react';
import { Menu, MenuItem, ListItemIcon, ListItemText, Divider, Box } from '@mui/material';
import ArrowUpward from '@mui/icons-material/ArrowUpward';
import ArrowDownward from '@mui/icons-material/ArrowDownward';
import PushPinIcon from '@mui/icons-material/PushPin';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import ViewColumnIcon from '@mui/icons-material/ViewColumn';
import CheckIcon from '@mui/icons-material/Check';
import { useTranslation } from 'react-i18next';
import type { ColumnDef } from '../types';

interface HeaderMenuProps<T> {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  activeColumn: ColumnDef<T> | null;
  initialColumns: ColumnDef<T>[];
  setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
  onResetColumns: () => void;
  onOpenChooseColumns: () => void;
}

export function HeaderMenu<T>({
  anchorEl,
  onClose,
  activeColumn,
  initialColumns,
  setColumns,
  onSort,
  onResetColumns,
  onOpenChooseColumns,
}: HeaderMenuProps<T>) {
  const { t } = useTranslation();
  const [pinMenuAnchor, setPinMenuAnchor] = useState<HTMLElement | null>(null);

  const handlePin = (position: 'left' | 'right' | null) => {
    if (activeColumn) {
      setColumns((prev) =>
        prev.map((c) => (c.field === activeColumn.field ? { ...c, pinned: position } : c))
      );
    }
    onClose();
    setPinMenuAnchor(null);
  };

  const handleAutosize = (all: boolean) => {
    if (all) {
      setColumns((prev) =>
        prev.map((c) => {
          const initial = initialColumns.find((i) => i.field === c.field);
          return { ...c, width: initial?.width, flex: initial?.flex };
        })
      );
    } else if (activeColumn) {
      const initial = initialColumns.find((c) => c.field === activeColumn.field);
      setColumns((prev) =>
        prev.map((c) =>
          c.field === activeColumn.field ? { ...c, width: initial?.width, flex: initial?.flex } : c
        )
      );
    }
    onClose();
  };

  return (
    <>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => {
          onClose();
          setPinMenuAnchor(null);
        }}
        slotProps={{ paper: { sx: { width: 240, p: 0 } } }}
      >
        {activeColumn?.sortable !== false && (
          <MenuItem
            onClick={() => {
              onSort(activeColumn?.field as string, 'asc');
              onClose();
            }}
          >
            <ListItemIcon>
              <ArrowUpward fontSize="small" />
            </ListItemIcon>
            <ListItemText
              primary={t('grid.sort_asc')}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </MenuItem>
        )}
        {activeColumn?.sortable !== false && (
          <MenuItem
            onClick={() => {
              onSort(activeColumn?.field as string, 'desc');
              onClose();
            }}
          >
            <ListItemIcon>
              <ArrowDownward fontSize="small" />
            </ListItemIcon>
            <ListItemText
              primary={t('grid.sort_desc')}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </MenuItem>
        )}

        <Divider />

        <MenuItem
          onClick={(e) => setPinMenuAnchor((prev) => (prev ? null : e.currentTarget))}
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            bgcolor: pinMenuAnchor ? 'action.hover' : 'transparent',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <ListItemIcon>
              <PushPinIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText
              primary={t('grid.pin_column')}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </Box>
          <ChevronRightIcon fontSize="small" />
        </MenuItem>

        <Divider />

        <MenuItem onClick={() => handleAutosize(false)}>
          <ListItemText
            inset
            primary={t('grid.autosize_this')}
            slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
          />
        </MenuItem>
        <MenuItem onClick={() => handleAutosize(true)}>
          <ListItemText
            inset
            primary={t('grid.autosize_all')}
            slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
          />
        </MenuItem>

        <Divider />

        <MenuItem
          onClick={() => {
            onOpenChooseColumns();
            onClose();
          }}
        >
          <ListItemIcon>
            <ViewColumnIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText
            primary={t('grid.choose_columns')}
            slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
          />
        </MenuItem>
        <MenuItem
          onClick={() => {
            onResetColumns();
            onClose();
          }}
        >
          <ListItemText
            inset
            primary={t('grid.reset_columns')}
            slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
          />
        </MenuItem>
      </Menu>

      {/* Pin sub-menu */}
      <Menu
        anchorEl={pinMenuAnchor}
        open={Boolean(pinMenuAnchor)}
        onClose={() => setPinMenuAnchor(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'left' }}
        slotProps={{ paper: { sx: { width: 140, marginInlineStart: 0.5 } } }}
      >
        {(
          [
            { label: t('grid.no_pin'), value: null },
            { label: t('grid.pin_left'), value: 'left' },
            { label: t('grid.pin_right'), value: 'right' },
          ] as const
        ).map(({ label, value }) => (
          <MenuItem key={value || 'none'} onClick={() => handlePin(value)}>
            {activeColumn?.pinned === value && (
              <ListItemIcon>
                <CheckIcon fontSize="small" />
              </ListItemIcon>
            )}
            <ListItemText
              inset={activeColumn?.pinned !== value}
              primary={label}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
