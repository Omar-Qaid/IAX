import React, { useState } from 'react';
import { IconButton, Badge, Menu, MenuItem, ListItemText, Typography, Divider } from '@mui/material';
import NotificationsOutlinedIcon from '@mui/icons-material/NotificationsOutlined';

export const NotificationMenu: React.FC = () => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleOpen = (e: React.MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);

  return (
    <>
      <IconButton size="small" color="inherit" onClick={handleOpen}>
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
            Notifications Center
          </Typography>
        </MenuItem>
        <Divider />
        <MenuItem onClick={handleClose}>
          <ListItemText
            primary={<Typography variant="body2" sx={{ fontWeight: 600 }}>New Sales Order Confirmed</Typography>}
            secondary={<Typography variant="caption">SO-00104 confirmed by user</Typography>}
          />
        </MenuItem>
        <MenuItem onClick={handleClose}>
          <ListItemText
            primary={<Typography variant="body2" sx={{ fontWeight: 600 }}>Customer Credit Limit Exceeded</Typography>}
            secondary={<Typography variant="caption">US-001 exceeded credit limit</Typography>}
          />
        </MenuItem>
      </Menu>
    </>
  );
};
