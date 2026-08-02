import React from 'react';
import { Box, FormControl, InputAdornment, InputLabel, MenuItem, Select, TextField } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';

export interface EnterpriseFilterOption {
  value: string;
  label: string;
}

export interface EnterpriseListFilterBarProps {
  filterLabel: string;
  searchByLabel: string;
  query: string;
  field: string;
  options: EnterpriseFilterOption[];
  onQueryChange: (query: string) => void;
  onFieldChange: (field: string) => void;
}

const controlSx = {
  '& .MuiOutlinedInput-root': { height: 29, borderRadius: 0.5, bgcolor: 'background.paper', fontSize: '0.75rem' },
  '& .MuiOutlinedInput-notchedOutline': { borderColor: 'text.secondary' },
};

export const EnterpriseListFilterBar: React.FC<EnterpriseListFilterBarProps> = ({
  filterLabel, searchByLabel, query, field, options, onQueryChange, onFieldChange,
}) => (
  <Box
    component="section"
    aria-label={filterLabel}
    sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'minmax(180px, 208px) 154px minmax(150px, 1fr)' }, gap: 1, alignItems: 'end', px: { xs: 0.5, sm: 1 }, pb: 1, maxWidth: 540 }}
  >
    <TextField
      value={query}
      onChange={(event) => onQueryChange(event.target.value)}
      placeholder={filterLabel}
      aria-label={filterLabel}
      sx={controlSx}
      slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 15, color: 'text.secondary' }} /></InputAdornment> } }}
    />
    <FormControl size="small" sx={controlSx}>
      <InputLabel shrink sx={{ fontSize: '0.6875rem', transform: 'translate(0, -18px) scale(1)', color: 'text.primary' }}>{searchByLabel}</InputLabel>
      <Select value={field} onChange={(event) => onFieldChange(event.target.value)} inputProps={{ 'aria-label': searchByLabel }}>
        {options.map((option) => <MenuItem key={option.value} value={option.value} sx={{ fontSize: '0.75rem' }}>{option.label}</MenuItem>)}
      </Select>
    </FormControl>
    <TextField value={query} onChange={(event) => onQueryChange(event.target.value)} aria-label={searchByLabel} sx={{ ...controlSx, display: { xs: 'none', sm: 'block' } }} />
  </Box>
);
