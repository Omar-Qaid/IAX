import type { Components, Theme } from '@mui/material/styles';
import { uiDensity } from '@shared/constants/uiDensity';
import { APP_FONT_FAMILY_CSS_VARIABLE } from '@shared/constants/fontFamilies';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export const getComponentOverrides = (theme: Theme): Components => ({
  MuiTypography: {
    styleOverrides: {
      overline: { fontWeight: 700 },
    },
  },
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
      'html, body, #root': {
        width: '100%',
        minWidth: 0,
        minHeight: '100%',
        [APP_FONT_FAMILY_CSS_VARIABLE]: theme.typography.fontFamily,
      },
      'html, body': {
        margin: 0,
        overflowX: 'hidden',
      },
      '*': {
        boxSizing: 'border-box',
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
    styleOverrides: {
      root: {
        minWidth: 0,
        '& > .MuiInputLabel-root': {
          position: 'static',
          transform: 'none',
          maxWidth: '100%',
          marginBottom: 6,
          padding: 0,
          fontFamily: theme.typography.fontFamily,
          fontSize: d365.labelFontSize,
          lineHeight: 1.2,
          color: theme.palette.text.primary,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        },
        '& .MuiOutlinedInput-root:not(.MuiInputBase-multiline)': {
          height: d365.controlHeight,
          minHeight: d365.controlHeight,
          borderRadius: d365.radius,
          fontSize: d365.fontSize,
        },
        '& .MuiOutlinedInput-root.MuiInputBase-multiline': {
          borderRadius: d365.radius,
          fontSize: d365.fontSize,
          padding: 0,
        },
        '& .MuiOutlinedInput-input': { padding: '4px 7px', boxSizing: 'border-box' },
        '& .MuiOutlinedInput-root textarea': { padding: '5px 7px' },
        '& .MuiOutlinedInput-notchedOutline legend': { display: 'none' },
        '& .MuiInputBase-input::placeholder': { color: theme.palette.text.secondary, opacity: 0.6 },
      },
    },
  },
  MuiFormControl: {
    styleOverrides: {
      root: {
        minWidth: 0,
        '& > .MuiInputLabel-root': {
          position: 'static',
          transform: 'none',
          maxWidth: '100%',
          marginBottom: 6,
          padding: 0,
          fontFamily: theme.typography.fontFamily,
          fontSize: d365.labelFontSize,
          lineHeight: 1.2,
          color: theme.palette.text.primary,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        },
      },
    },
  },
  MuiAutocomplete: {
    styleOverrides: {
      root: {
        minWidth: 0,
        '& .MuiOutlinedInput-root': { minHeight: d365.controlHeight, paddingBlock: 0 },
        '& .MuiOutlinedInput-root .MuiAutocomplete-input': { height: d365.controlHeight, boxSizing: 'border-box', padding: '4px 7px' },
        '& .MuiAutocomplete-endAdornment': { insetInlineEnd: 0, top: '50%', transform: 'translateY(-50%)' },
      },
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
    styleOverrides: {
      select: { minHeight: 0, paddingInlineEnd: 28 },
      icon: { color: theme.palette.text.secondary },
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
        paddingInline: '12px !important',
      },
    },
  },
  MuiDrawer: {
    styleOverrides: {
      paper: ({ ownerState }) => ({
        borderRadius: 0,
        borderLeft: ownerState.anchor === 'right' ? `1px solid ${theme.palette.divider}` : 0,
        borderRight: ownerState.anchor === 'left' ? `1px solid ${theme.palette.divider}` : 0,
      }),
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
        margin: theme.spacing(1),
        maxWidth: 'calc(100vw - 16px)',
        maxHeight: 'calc(100dvh - 16px)',
        overflowX: 'hidden',
        direction: theme.direction,
        textAlign: 'start',
        [theme.breakpoints.up('sm')]: {
          margin: theme.spacing(2),
          maxWidth: 'calc(100vw - 32px)',
          maxHeight: 'calc(100dvh - 32px)',
        },
      },
    },
  },
  MuiDialogTitle: {
    styleOverrides: { root: { padding: `${uiDensity.dialogPadding}px`, fontSize: '0.9375rem', textAlign: 'start' } },
  },
  MuiDialogContent: {
    styleOverrides: { root: { padding: `${uiDensity.dialogPadding}px`, textAlign: 'start' } },
  },
  MuiDialogActions: {
    styleOverrides: { root: { padding: '8px 12px', gap: 4, flexWrap: 'wrap', justifyContent: 'flex-end' } },
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
    styleOverrides: { root: { height: 22 }, label: { paddingInline: 7 } },
  },
  MuiAlert: {
    styleOverrides: { root: { padding: '4px 8px' }, message: { padding: '3px 0' } },
  },
  MuiCheckbox: { styleOverrides: { root: { padding: 4 } } },
  MuiRadio: { styleOverrides: { root: { padding: 4 } } },
  MuiFormControlLabel: {
    styleOverrides: { root: { marginInlineStart: -4, marginInlineEnd: 8, minHeight: 30 } },
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
