import type { SxProps, Theme } from '@mui/material/styles';
import { d365 } from '@patterns/list-details/d365Tokens';

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
    marginLeft: 0,
    marginRight: '6px',
    color: 'primary.main',
    '& .MuiSvgIcon-root': { fontSize: 17 },
  },
  '& .MuiButton-endIcon': {
    marginLeft: '6px',
    marginRight: 0,
    color: 'primary.main',
    '& .MuiSvgIcon-root': { fontSize: 17 },
  },
  '&.Mui-disabled': { color: 'text.disabled' },
};
