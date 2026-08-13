import type { Components, Theme } from '@mui/material/styles';
import { uiDensity } from '@shared/constants/uiDensity';

export const getComponentOverrides = (theme: Theme): Components => ({
  MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 2,
        fontWeight: 600,
        textTransform: 'none',
        padding: '4px 12px',
        minHeight: uiDensity.buttonHeight,
        boxShadow: 'none',
        '&:hover': {
          boxShadow: 'none',
        },
      },
      sizeSmall: {
        padding: '2px 8px',
        fontSize: '0.75rem',
      },
    },
  },
  MuiIconButton: {
    styleOverrides: {
      root: {
        padding: 6,
        borderRadius: 2,
      },
    },
  },
  MuiCssBaseline: {
    styleOverrides: {
      '*': {
        scrollbarWidth: 'thin',
        scrollbarColor: `${theme.palette.mode === 'light' ? '#a8a8a8' : '#667085'} transparent`,
      },
      '*::-webkit-scrollbar': {
        width: uiDensity.scrollbarSize,
        height: uiDensity.scrollbarSize,
      },
      '*::-webkit-scrollbar-track': { backgroundColor: 'transparent' },
      '*::-webkit-scrollbar-thumb': {
        backgroundColor: theme.palette.mode === 'light' ? '#a8a8a8' : '#667085',
        borderRadius: 999,
      },
    },
  },
  MuiTextField: {
    defaultProps: {
      size: 'small',
      variant: 'outlined',
    },
  },
  MuiInputBase: {
    styleOverrides: {
      root: {
        fontSize: '0.8125rem',
      },
      input: {
        padding: '6px 8px',
      },
    },
  },
  MuiOutlinedInput: {
    styleOverrides: {
      root: {
        borderRadius: 2,
        minHeight: uiDensity.controlHeight,
      },
      input: {
        padding: '6px 8px',
      },
    },
  },
  MuiSelect: {
    defaultProps: {
      size: 'small',
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        boxShadow: 'none',
        borderBottom: `1px solid ${theme.palette.divider}`,
      },
    },
  },
  MuiToolbar: {
    styleOverrides: {
      root: {
        minHeight: `${uiDensity.toolbarHeight}px !important`,
        paddingLeft: '12px !important',
        paddingRight: '12px !important',
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: {
        borderRadius: 0,
        borderRight: `1px solid ${theme.palette.divider}`,
      },
    },
  },
  MuiAccordion: {
    styleOverrides: {
      root: {
        borderRadius: 2,
        boxShadow: 'none',
        border: `1px solid ${theme.palette.divider}`,
        marginBottom: 8,
        '&:before': {
          display: 'none',
        },
        '&.Mui-expanded': {
          margin: '0 0 8px 0',
        },
      },
    },
  },
  MuiAccordionSummary: {
    styleOverrides: {
      root: {
        minHeight: '36px !important',
        padding: '0 12px',
        backgroundColor: theme.palette.mode === 'light' ? '#fafafa' : '#2d2d2d',
        '&.Mui-expanded': {
          minHeight: '36px !important',
          borderBottom: `1px solid ${theme.palette.divider}`,
        },
      },
      content: {
        margin: '6px 0 !important',
      },
    },
  },
  MuiAccordionDetails: {
    styleOverrides: {
      root: {
        padding: uiDensity.sectionPadding,
      },
    },
  },
  MuiTabs: {
    styleOverrides: {
      root: {
        minHeight: uiDensity.tabHeight,
      },
    },
  },
  MuiTab: {
    styleOverrides: {
      root: {
        minHeight: uiDensity.tabHeight,
        padding: '4px 12px',
        fontWeight: 600,
        fontSize: '0.8125rem',
        textTransform: 'none',
      },
    },
  },
  MuiDialog: {
    styleOverrides: {
      paper: {
        borderRadius: 4,
        boxShadow: theme.shadows[8],
      },
    },
  },
  MuiDialogTitle: {
    styleOverrides: { root: { padding: `${uiDensity.dialogPadding}px`, fontSize: '0.9375rem' } },
  },
  MuiDialogContent: {
    styleOverrides: { root: { padding: `${uiDensity.dialogPadding}px` } },
  },
  MuiDialogActions: {
    styleOverrides: { root: { padding: '8px 12px', gap: 4 } },
  },
  MuiCardContent: {
    styleOverrides: {
      root: {
        padding: uiDensity.sectionPadding,
        '&:last-child': { paddingBottom: uiDensity.sectionPadding },
      },
    },
  },
  MuiListItemButton: {
    styleOverrides: { root: { minHeight: 32, padding: '4px 8px' } },
  },
  MuiMenuItem: {
    styleOverrides: { root: { minHeight: '32px !important', padding: '4px 8px', fontSize: '0.8125rem' } },
  },
  MuiTableCell: {
    styleOverrides: { root: { padding: '5px 8px' }, sizeSmall: { padding: '3px 6px' } },
  },
  MuiChip: {
    defaultProps: { size: 'small' },
    styleOverrides: { root: { height: 22 }, label: { paddingLeft: 7, paddingRight: 7 } },
  },
  MuiAlert: {
    styleOverrides: { root: { padding: '4px 8px' }, message: { padding: '3px 0' } },
  },
  MuiCheckbox: { styleOverrides: { root: { padding: 4 } } },
  MuiRadio: { styleOverrides: { root: { padding: 4 } } },
  MuiFormControlLabel: {
    styleOverrides: { root: { marginLeft: -4, marginRight: 8, minHeight: 30 } },
  },
  MuiPaginationItem: {
    styleOverrides: { root: { minWidth: 28, height: 28, margin: '0 1px' } },
  },
  MuiMenu: {
    styleOverrides: {
      paper: {
        borderRadius: 2,
        boxShadow: theme.shadows[4],
        border: `1px solid ${theme.palette.divider}`,
      },
    },
  },
  MuiTooltip: {
    styleOverrides: {
      tooltip: {
        borderRadius: 2,
        fontSize: '0.6875rem',
      },
    },
  },
});
