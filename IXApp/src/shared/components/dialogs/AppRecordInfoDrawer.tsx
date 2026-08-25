import React, { useMemo } from 'react';
import { Box, Drawer, IconButton, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { RecordValueDisplay } from './RecordValueDisplay';

export interface AppRecordInfoDrawerProps {
  open: boolean;
  onClose: () => void;
  record: unknown;
  title?: string;
}

const humanize = (value: string) =>
  value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/^./, (character) => character.toUpperCase());

export function AppRecordInfoDrawer({
  open,
  onClose,
  record,
  title,
}: AppRecordInfoDrawerProps): React.ReactElement {
  const { t } = useAppTranslation();
  const fields = useMemo(
    () =>
      record && typeof record === 'object' && !Array.isArray(record)
        ? Object.entries(record as Record<string, unknown>)
        : [],
    [record]
  );

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: { xs: '100%', sm: 420 } } } }}
      sx={{ zIndex: (theme) => theme.zIndex.drawer + 2 }}
    >
      <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Box
          sx={{
            minHeight: 57,
            px: 2,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid',
            borderColor: 'divider',
            bgcolor: 'action.hover',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <InfoOutlinedIcon color="primary" />
            <Box>
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                {t('common.recordInfo', 'Record Info')}
              </Typography>
              {title && (
                <Typography variant="caption" color="text.secondary">
                  {title}
                </Typography>
              )}
            </Box>
          </Box>
          <IconButton onClick={onClose} size="small" aria-label={t('actions.close', 'Close')}>
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>

        <Box sx={{ flex: 1, overflowY: 'auto', p: 2 }}>
          {fields.length === 0 ? (
            <Typography color="text.secondary">
              {t('messages.selectRecord', 'Select a record.')}
            </Typography>
          ) : (
            fields.map(([field, value]) => (
              <Box key={field} sx={{ py: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
                <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                  {humanize(field)}
                </Typography>
                <Box sx={{ mt: 0.25, fontSize: 14, minWidth: 0 }}>
                  <RecordValueDisplay value={value} />
                </Box>
              </Box>
            ))
          )}
        </Box>
      </Box>
    </Drawer>
  );
}
