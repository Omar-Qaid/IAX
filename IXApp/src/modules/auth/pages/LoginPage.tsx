import React, { useState } from 'react';
import { Box, Button, TextField, Typography, Alert } from '@mui/material';
import { useAuth } from '@core/auth/useAuth';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('password');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username) {
      setError('Please enter a username');
      return;
    }
    try {
      await login(username, password);
      navigate(ROUTE_PATHS.DASHBOARD);
    } catch {
      setError('Failed to sign in. Check your credentials.');
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 1 }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2, textAlign: 'center' }}>
        Microsoft Dynamics 365 F&O-Inspired Architecture Shell
      </Typography>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      <TextField
        fullWidth
        label="Username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
        margin="normal"
        size="small"
        required
      />
      <TextField
        fullWidth
        label="Password"
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        margin="normal"
        size="small"
        required
      />
      <Button type="submit" fullWidth variant="contained" color="primary" sx={{ mt: 2, py: 1 }}>
        Sign In
      </Button>
    </Box>
  );
};

export default LoginPage;
