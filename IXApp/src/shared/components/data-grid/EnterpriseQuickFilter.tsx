import React from 'react';
import { InputAdornment, TextField } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';

export interface EnterpriseQuickFilterProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
}

export const EnterpriseQuickFilter: React.FC<EnterpriseQuickFilterProps> = ({ label, value, onChange }) => (
  <TextField
    value={value}
    onChange={(event) => onChange(event.target.value)}
    placeholder={label}
    aria-label={label}
    sx={{ width: { xs: '100%', sm: 208 }, px: { xs: 0.5, sm: 1 }, pb: 1, '& .MuiOutlinedInput-root': { height: 29, borderRadius: 0.5, bgcolor: 'background.paper', fontSize: '0.75rem' }, '& .MuiOutlinedInput-notchedOutline': { borderColor: 'text.secondary' } }}
    slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 15, color: 'text.secondary' }} /></InputAdornment> } }}
  />
);
