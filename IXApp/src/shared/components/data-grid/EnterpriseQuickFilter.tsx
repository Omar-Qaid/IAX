import React from 'react';
import { InputAdornment, TextField } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

export interface EnterpriseQuickFilterProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
}

export const EnterpriseQuickFilter: React.FC<EnterpriseQuickFilterProps> = ({
  label,
  value,
  onChange,
}) => (
  <TextField
    value={value}
    onChange={(event) => onChange(event.target.value)}
    placeholder={label}
    aria-label={label}
    sx={{
      width: { xs: '100%', sm: 315 },
      px: { xs: 1, sm: 2.5 },
      pb: 2.25,
      '& .MuiOutlinedInput-root': {
        width: 275,
        height: 37,
        borderRadius: '4px',
        bgcolor: 'background.paper',
        fontFamily: APP_FONT_FAMILY,
        fontSize: 16,
      },
      '& .MuiOutlinedInput-notchedOutline': { borderColor: 'text.secondary' },
      '& input::placeholder': { color: 'text.secondary', opacity: 1 },
    }}
    slotProps={{
      input: {
        startAdornment: (
          <InputAdornment position="start">
            <SearchIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
          </InputAdornment>
        ),
      },
    }}
  />
);
