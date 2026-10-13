export const hasLookupValue = (value: unknown): boolean =>
  value !== null && value !== undefined && value !== '' &&
  (!Array.isArray(value) || value.length > 0);

import { d365 } from '@shared/constants/enterpriseUiTokens';

export const lookupValueSx = (value: unknown) => ({
  '& .MuiInputBase-input, & .MuiSelect-select': hasLookupValue(value)
    ? { color: d365.primary, WebkitTextFillColor: d365.primary }
    : {},
});

