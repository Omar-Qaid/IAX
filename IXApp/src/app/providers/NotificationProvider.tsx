import React from 'react';
import { Snackbar, Alert } from '@mui/material';
import { useNotificationStore } from '@shared/services/notificationStore';

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const notifications = useNotificationStore((s) => s.notifications);
  const removeNotification = useNotificationStore((s) => s.removeNotification);

  const activeNotification = notifications[0];

  const handleClose = (_event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') return;
    if (activeNotification) {
      removeNotification(activeNotification.id);
    }
  };

  return (
    <>
      {children}
      {activeNotification && (
        <Snackbar
          open={true}
          autoHideDuration={activeNotification.autoHideDuration || 4000}
          onClose={handleClose}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert
            onClose={handleClose}
            severity={activeNotification.type}
            variant="filled"
            elevation={6}
            sx={{ width: '100%', borderRadius: 1 }}
          >
            {activeNotification.message}
          </Alert>
        </Snackbar>
      )}
    </>
  );
};
