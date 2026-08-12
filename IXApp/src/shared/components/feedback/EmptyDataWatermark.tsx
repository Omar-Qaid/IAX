import React from 'react';
import { Box, Typography } from '@mui/material';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function EmptyDataWatermark(): React.ReactElement {
  const { t } = useAppTranslation();

  return (
    <Box
      aria-hidden="true"
      sx={{
        width: '100%',
        height: '100%',
        minHeight: 180,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'text.primary',
        opacity: 0.13,
        pointerEvents: 'none',
        userSelect: 'none',
        textAlign: 'center',
      }}
    >
      <InboxOutlinedIcon sx={{ fontSize: 58, mb: 1 }} />
      <Typography sx={{ fontSize: 17, lineHeight: 1.3, fontWeight: 700 }}>
        {t('common.noData')}
      </Typography>
      <Typography sx={{ mt: 0.5, fontSize: 13, lineHeight: 1.4 }}>
        {t('grid.no_records_msg')}
      </Typography>
    </Box>
  );
}
