import type { SxProps, Theme } from '@mui/material/styles';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export const actionPaneControlSx: SxProps<Theme> = {
  minWidth: 0,
  minHeight: 31,
  height: 31,
  px: 1,
  py: 0,
  color: 'text.primary',
  fontFamily: d365.fontFamily,
  fontSize: 14,
  fontWeight: 400,
  lineHeight: 1,
  textTransform: 'none',
  border: '1px solid transparent',
  borderRadius: d365.radius,
  '&:hover': {
    bgcolor: (theme) => (theme.palette.mode === 'light' ? '#e8edf4' : '#333333'),
    borderColor: 'divider',
  },
  '& .MuiButton-startIcon': {
    marginInlineStart: 0,
    marginInlineEnd: '6px',
    color: 'primary.main',
    '& .MuiSvgIcon-root': { fontSize: 17 },
  },
  '& .MuiButton-endIcon': {
    marginInlineStart: '6px',
    marginInlineEnd: 0,
    color: 'primary.main',
    '& .MuiSvgIcon-root': { fontSize: 17 },
  },
  '&.Mui-disabled': { color: 'text.disabled' },
};
