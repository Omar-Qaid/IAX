import React, { createContext, useContext } from 'react';
import { Box } from '@mui/material';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';

interface FieldPresentation {
  viewMode: boolean;
  masterRoute?: string;
}

const FieldPresentationContext = createContext<FieldPresentation>({ viewMode: false });

export function FieldPresentationProvider({ viewMode, masterRoute, children }: FieldPresentation & { children: React.ReactNode }): React.ReactElement {
  return (
    <FieldPresentationContext.Provider value={{ viewMode, masterRoute }}>
      {viewMode ? (
        <Box sx={{
          '& .MuiOutlinedInput-root': { borderRadius: 0, bgcolor: 'transparent', boxShadow: 'none' },
          '& .MuiOutlinedInput-notchedOutline': (theme) => ({ border: 0, borderBottom: `1px solid ${enterpriseSurfaces(theme).strongBorder}`, borderRadius: 0 }),
          '& .MuiInputBase-input.Mui-disabled': (theme) => ({ WebkitTextFillColor: theme.palette.text.primary }),
          '& .MuiInputBase-input::placeholder': { opacity: 0 },
          '& .MuiSelect-icon, & .MuiAutocomplete-endAdornment, & .MuiInputAdornment-root': { display: 'none' },
          '& input, & textarea, & .MuiSelect-select': { pointerEvents: 'none' },
        }}>{children}</Box>
      ) : children}
    </FieldPresentationContext.Provider>
  );
}

export const useFieldViewMode = (): boolean => useContext(FieldPresentationContext).viewMode;
export const useFieldMasterRoute = (): string | undefined => useContext(FieldPresentationContext).masterRoute;
