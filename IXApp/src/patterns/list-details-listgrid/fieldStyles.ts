export const listGridLabelSx = {
  display: 'block', fontSize: 11, lineHeight: '16px', mb: '4px',
} as const;

export const listGridControlSx = {
  '& .MuiInputBase-root': { height: 28, fontSize: 12, borderRadius: '3px', boxSizing: 'border-box' },
  '& .MuiInputBase-input': { px: '6px', py: '4px' },
  '& .MuiOutlinedInput-notchedOutline': { top: 0 },
  '& .MuiOutlinedInput-notchedOutline legend': { display: 'none' },
  '& .MuiAutocomplete-inputRoot.MuiOutlinedInput-root': { height: 28, minHeight: 28, padding: '0 28px 0 0' },
  '& .MuiAutocomplete-inputRoot .MuiAutocomplete-input': { padding: '4px 6px', minWidth: 0 },
  '& .MuiSelect-select': { paddingRight: '28px' },
  '& .MuiAutocomplete-endAdornment': { right: '4px', top: '50%', transform: 'translateY(-50%)' },
  '& .MuiAutocomplete-popupIndicator': { padding: 0 },
  '& .MuiSelect-icon': { right: '4px' },
} as const;
