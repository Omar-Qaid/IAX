import React, { useState } from 'react';
import { Alert, Button, Stack, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAuth } from '@core/auth/useAuth';
import { ROUTE_PATHS } from '@app/routes/routePaths';

export function LoginPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const { login, isLoading } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('admin');
  const [error, setError] = useState('');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!username.trim()) {
      setError(t('validation.required', { field: t('fields.username') }));
      return;
    }
    setError('');
    await login(username.trim());
    navigate(ROUTE_PATHS.DASHBOARD, { replace: true });
  };

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit} noValidate>
      <Typography variant="h6" component="h1">{t('pages.login.title')}</Typography>
      <Typography variant="body2" color="text.secondary">{t('pages.login.subtitle')}</Typography>
      {error && <Alert severity="error">{error}</Alert>}
      <TextField
        label={t('fields.username')}
        value={username}
        onChange={(event) => setUsername(event.target.value)}
        required
        autoComplete="username"
        autoFocus
      />
      <Button type="submit" variant="contained" disabled={isLoading}>
        {isLoading ? t('messages.signingIn') : t('actions.signIn')}
      </Button>
    </Stack>
  );
}
