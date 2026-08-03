import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined';
import i18n from '@core/localization/i18n';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  public override state: State = {
    hasError: false,
    error: null,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public override componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('Uncaught error in ErrorBoundary:', error, errorInfo);
  }

  private handleReset = (): void => {
    this.setState({ hasError: false, error: null });
  };

  public override render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '300px',
            p: 3,
          }}
        >
          <Paper
            elevation={1}
            sx={{
              p: 4,
              maxWidth: 500,
              textAlign: 'center',
              borderRadius: 1,
              borderInlineStart: '4px solid',
              borderInlineStartColor: 'error.main',
            }}
          >
            <WarningAmberOutlinedIcon color="error" sx={{ fontSize: 48, mb: 1 }} />
            <Typography variant="h6" color="error" sx={{ mb: 1, fontWeight: 700 }}>
              {i18n.t('errors.boundaryTitle', 'Application error')}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              {this.state.error?.message || i18n.t('errors.generic')}
            </Typography>
            <Button variant="contained" color="primary" onClick={this.handleReset} size="small">
              {i18n.t('actions.retry')}
            </Button>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}
