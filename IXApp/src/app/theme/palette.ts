import type { PaletteOptions } from '@mui/material/styles';

export const lightPalette: PaletteOptions = {
  mode: 'light',
  primary: {
    main: '#005a9e',
    light: '#106ebe',
    dark: '#004578',
    contrastText: '#ffffff',
  },
  secondary: {
    main: '#486885',
    light: '#6c88a3',
    dark: '#2c4964',
    contrastText: '#ffffff',
  },
  background: {
    default: '#f3f2f1',
    paper: '#ffffff',
  },
  text: {
    primary: '#201f1e',
    secondary: '#605e5c',
    disabled: '#a19f9d',
  },
  divider: '#e1dfdd',
  success: {
    main: '#107c41',
    light: '#dff6dd',
    dark: '#0b5a2f',
  },
  warning: {
    main: '#d83b01',
    light: '#fff4ce',
    dark: '#a82e00',
  },
  error: {
    main: '#a80000',
    light: '#fde7e9',
    dark: '#750000',
  },
  info: {
    main: '#0078d4',
    light: '#eff6fc',
    dark: '#004e8c',
  },
};

export const darkPalette: PaletteOptions = {
  mode: 'dark',
  primary: {
    main: '#2899f5',
    light: '#6cb8f8',
    dark: '#0078d4',
    contrastText: '#ffffff',
  },
  secondary: {
    main: '#8aacc8',
    light: '#b6cee2',
    dark: '#5d83a4',
    contrastText: '#000000',
  },
  background: {
    default: '#1b1a19',
    paper: '#252423',
  },
  text: {
    primary: '#f3f2f1',
    secondary: '#c8c6c4',
    disabled: '#797775',
  },
  divider: '#3b3a39',
  success: {
    main: '#54b054',
    light: '#1e381e',
    dark: '#388e3c',
  },
  warning: {
    main: '#fce100',
    light: '#3a3500',
    dark: '#c7b200',
  },
  error: {
    main: '#f1707b',
    light: '#441d20',
    dark: '#d32f2f',
  },
  info: {
    main: '#4f96d8',
    light: '#1b3247',
    dark: '#0288d1',
  },
};
