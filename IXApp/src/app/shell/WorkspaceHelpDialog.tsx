import { useId } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from '@mui/material';
import { useTranslation } from 'react-i18next';

export function WorkspaceHelpDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation();
  const titleId = useId();
  return (
    <Dialog open={open} onClose={onClose} aria-labelledby={titleId} fullWidth maxWidth="sm">
      <DialogTitle id={titleId}>{t('workspaceHelp.title')}</DialogTitle>
      <DialogContent>
        <Stack spacing={2}>
          {(['navigation', 'editing', 'grid'] as const).map((section) => (
            <section key={section}>
              <Typography component="h2" variant="subtitle1">
                {t(`workspaceHelp.${section}Title`)}
              </Typography>
              <Typography variant="body2">{t(`workspaceHelp.${section}`)}</Typography>
            </section>
          ))}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('actions.close', 'Close')}</Button>
      </DialogActions>
    </Dialog>
  );
}
