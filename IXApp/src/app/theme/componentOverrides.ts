import type { Components, Theme } from '@mui/material/styles';
import type {} from '@mui/x-data-grid/themeAugmentation';

export const getComponentOverrides = (theme: Theme): Components => ({
  MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 2,
        fontWeight: 600,
        textTransform: 'none',
        padding: '4px 12px',
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
        minHeight: '44px !important',
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
        padding: 12,
      },
    },
  },
  MuiTabs: {
    styleOverrides: {
      root: {
        minHeight: 36,
      },
    },
  },
  MuiTab: {
    styleOverrides: {
      root: {
        minHeight: 36,
        padding: '6px 16px',
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
  MuiDataGrid: {
    styleOverrides: {
      root: {
        borderRadius: 2,
        border: `1px solid ${theme.palette.divider}`,
        fontSize: '0.8125rem',
        fontFamily: theme.typography.fontFamily,
        '& .MuiDataGrid-columnHeaders': {
          backgroundColor: theme.palette.mode === 'light' ? '#f3f2f1' : '#2d2c2b',
          borderBottom: `1px solid ${theme.palette.divider}`,
          minHeight: '34px !important',
          maxHeight: '34px !important',
        },
        '& .MuiDataGrid-columnHeader': {
          padding: '0 8px',
        },
        '& .MuiDataGrid-cell': {
          padding: '0 8px',
          borderBottom: `1px solid ${theme.palette.divider}`,
        },
        '& .MuiDataGrid-row:hover': {
          backgroundColor: theme.palette.mode === 'light' ? '#edebe9' : '#323130',
        },
        '& .MuiDataGrid-row.Mui-selected': {
          backgroundColor: theme.palette.mode === 'light' ? '#c7e0f4' : '#004e8c',
          '&:hover': {
            backgroundColor: theme.palette.mode === 'light' ? '#b1d6f0' : '#003966',
          },
        },
      },
    },
  },
});
