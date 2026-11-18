export const hasLookupValue = (value: unknown): boolean =>
  value !== null && value !== undefined && value !== '' &&
  (!Array.isArray(value) || value.length > 0);

import type { Theme } from '@mui/material/styles';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';

export const lookupValueSx = (value: unknown) => ({
  '& .MuiInputBase-input, & .MuiSelect-select': hasLookupValue(value)
    ? (theme: Theme) => ({
        color: enterpriseSurfaces(theme).accent,
        WebkitTextFillColor: enterpriseSurfaces(theme).accent,
      })
    : {},
});

