import React, { useState } from 'react';
import { InputBase, Paper, IconButton } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const GlobalSearch: React.FC = () => {
  const { t } = useAppTranslation();
  const [query, setQuery] = useState('');

  return (
    <Paper
      elevation={0}
      sx={{
        p: '2px 8px',
        display: 'flex',
        alignItems: 'center',
        width: { xs: 140, sm: 220, md: 300 },
        bgcolor: 'action.hover',
        borderRadius: 1,
        border: '1px solid transparent',
        '&:focus-within': {
          bgcolor: 'background.paper',
          borderColor: 'primary.main',
        },
      }}
    >
      <SearchIcon fontSize="small" sx={{ color: 'text.secondary', mr: 1 }} />
      <InputBase
        sx={{ ml: 0.5, flex: 1, fontSize: '0.8125rem' }}
        placeholder={t('actions.search') || 'Search pages & actions...'}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
      />
      {query && (
        <IconButton size="small" onClick={() => setQuery('')} sx={{ p: '2px' }}>
          <ClearIcon fontSize="small" />
        </IconButton>
      )}
    </Paper>
  );
};
