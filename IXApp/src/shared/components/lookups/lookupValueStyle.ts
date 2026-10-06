export const hasLookupValue = (value: unknown): boolean =>
  value !== null && value !== undefined && value !== '' &&
  (!Array.isArray(value) || value.length > 0);

export const lookupValueSx = (value: unknown) => hasLookupValue(value)
  ? {
      '& .MuiInputBase-input, & .MuiSelect-select': {
        color: d365.primary,
        WebkitTextFillColor: d365.primary,
      },
    }
  : {};
import { d365 } from '@shared/constants/enterpriseUiTokens';

