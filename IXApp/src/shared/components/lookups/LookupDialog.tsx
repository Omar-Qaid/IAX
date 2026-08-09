import React, { useState, useMemo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  List,
  ListItemButton,
  ListItemText,
  Button,
  InputAdornment,
  Box,
  Typography,
  IconButton,
  CircularProgress,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { LookupDialogProps } from './types';

export const LookupDialog: React.FC<LookupDialogProps> = ({
  open,
  onClose,
  title,
  options,
  selectedId,
  onSelect,
  loading = false,
}) => {
  const { t } = useAppTranslation();
  const [searchTerm, setSearchTerm] = useState('');

  const filteredOptions = useMemo(() => {
    if (!searchTerm.trim()) return options;
    const term = searchTerm.toLowerCase();
    return options.filter(
      (opt) =>
        opt.code.toLowerCase().includes(term) ||
        opt.name.toLowerCase().includes(term) ||
        (opt.description && opt.description.toLowerCase().includes(term))
    );
  }, [options, searchTerm]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          m: 0,
          p: 1.5,
          px: 2,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
        }}
      >
        <Typography component="span" variant="h6" sx={{ fontWeight: 700 }}>
          {title ?? t('lookups.selectOption')}
        </Typography>
        <IconButton size="small" aria-label={t('actions.close')} onClick={onClose}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers sx={{ p: 2 }}>
        <TextField
          autoFocus
          fullWidth
          size="small"
          placeholder={t('lookups.searchOptions')}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              ),
            },
          }}
          sx={{ mb: 2 }}
        />

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress size={32} />
          </Box>
        ) : filteredOptions.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="body2" color="text.secondary">
              {t('lookups.noMatchingRecords')}
            </Typography>
          </Box>
        ) : (
          <List disablePadding sx={{ maxHeight: 300, overflowY: 'auto' }}>
            {filteredOptions.map((opt) => (
              <ListItemButton
                key={opt.id}
                selected={opt.id === selectedId}
                onClick={() => {
                  onSelect(opt);
                  onClose();
                }}
                sx={{ borderRadius: 1, mb: 0.5 }}
              >
                <ListItemText
                  primary={`${opt.code} - ${opt.name}`}
                  secondary={opt.description}
                  slotProps={{
                    primary: { sx: { fontSize: '0.875rem', fontWeight: 600 } },
                    secondary: { sx: { fontSize: '0.75rem' } },
                  }}
                />
              </ListItemButton>
            ))}
          </List>
        )}
      </DialogContent>

      <DialogActions sx={{ p: 1.5, px: 2 }}>
        <Button onClick={onClose} size="small">
          {t('actions.cancel')}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
