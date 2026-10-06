import React, { createContext, useContext } from 'react';
import { Box } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

const FieldViewModeContext = createContext(false);

export function FieldViewModeProvider({ viewMode, children }: { viewMode: boolean; children: React.ReactNode }): React.ReactElement {
  return <FieldViewModeContext.Provider value={viewMode}>
    {viewMode ? <Box sx={{
      '& .MuiOutlinedInput-root': { borderRadius: 0, bgcolor: 'transparent', boxShadow: 'none' },
      '& .MuiOutlinedInput-notchedOutline': { border: 0, borderBottom: `1px solid ${d365.darkBorder}`, borderRadius: 0 },
      '& .MuiInputBase-input.Mui-disabled': { WebkitTextFillColor: d365.text },
      '& .MuiInputBase-input::placeholder': { opacity: 0 },
      '& .MuiSelect-icon, & .MuiAutocomplete-endAdornment, & .MuiInputAdornment-root': { display: 'none' },
      '& input, & textarea, & .MuiSelect-select': { pointerEvents: 'none' },
    }}>{children}</Box> : children}
  </FieldViewModeContext.Provider>;
}

export const useFieldViewMode = (): boolean => useContext(FieldViewModeContext);
