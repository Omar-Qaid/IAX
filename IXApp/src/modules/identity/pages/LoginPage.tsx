import React, { useState } from 'react';
import { Alert, Button, Stack, TextField, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAuth } from '@core/auth/useAuth';
import { environment } from '@core/configuration/environment';

export interface LoginPageProps {
  onLoginSuccess?: () => void;
}

export function LoginPage({ onLoginSuccess }: LoginPageProps): React.ReactElement {
  const { t } = useAppTranslation();
  const { login, isLoading } = useAuth();
  const [username, setUsername] = useState(environment.enableMockApi ? 'admin' : '');
  const [password, setPassword] = useState(environment.enableMockApi ? 'admin' : '');
  const [error, setError] = useState('');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!username.trim() || !password) {
      setError(t('validation.required', { field: t('fields.username') }));
      return;
    }
    setError('');
    try {
      await login(username.trim(), password);
      onLoginSuccess?.();
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : t('errors.generic'));
    }
  };

  return (
    <Stack component="form" spacing={2} onSubmit={handleSubmit} noValidate>
      <Typography variant="h6" component="h1">
        {t('pages.login.title')}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {t('pages.login.subtitle')}
      </Typography>
      {error && <Alert severity="error">{error}</Alert>}
      <TextField
        label={t('fields.username')}
        value={username}
        onChange={(event) => setUsername(event.target.value)}
        required
        autoComplete="username"
        autoFocus
      />
      <TextField
        label={t('fields.password')}
        type="password"
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        required
        autoComplete="current-password"
      />
      <Button type="submit" variant="contained" disabled={isLoading}>
        {isLoading ? t('messages.signingIn') : t('actions.signIn')}
      </Button>
    </Stack>
  );
}
