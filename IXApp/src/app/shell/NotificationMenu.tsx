import React, { useState } from 'react';
import { IconButton, Badge, Menu, MenuItem, ListItemText, Typography, Divider } from '@mui/material';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const NotificationMenu: React.FC = () => {
  const { t } = useAppTranslation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleOpen = (e: React.MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);

  return (
    <>
      <IconButton size="small" color="inherit" aria-label={t('common.notifications')} onClick={handleOpen}>
        <Badge badgeContent={2} color="error" variant="dot">
          <NotificationsOutlinedIcon fontSize="small" />
        </Badge>
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        <MenuItem disabled sx={{ opacity: '1 !important' }}>
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            {t('notifications.center')}
          </Typography>
        </MenuItem>
        <Divider />
        <MenuItem onClick={handleClose}>
          <ListItemText
            primary={<Typography variant="body2" sx={{ fontWeight: 600 }}>{t('notifications.salesOrderConfirmed')}</Typography>}
            secondary={<Typography variant="caption">{t('notifications.salesOrderConfirmedBy', { order: 'SO-00104' })}</Typography>}
          />
        </MenuItem>
        <MenuItem onClick={handleClose}>
          <ListItemText
            primary={<Typography variant="body2" sx={{ fontWeight: 600 }}>{t('notifications.creditLimitExceeded')}</Typography>}
            secondary={<Typography variant="caption">{t('notifications.customerExceededCredit', { customer: 'US-001' })}</Typography>}
          />
        </MenuItem>
      </Menu>
    </>
  );
};
