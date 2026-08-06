import { useState, memo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box, IconButton, Tooltip, InputAdornment,
  Typography, Collapse, Button,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
import AddRowIcon from '@mui/icons-material/AddCircleOutlined';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { DataGridSelectionSummary } from './DataGridSelectionSummary';

interface GridToolbarProps {
  globalSearch: string;
  setGlobalSearch: (val: string) => void;
  loadedRows: number;
  totalRowCount: number;
  filteredRows: number;
  serverSide?: boolean;
  masterForm?: boolean;
  onAddRow?: () => void;
  isEditing?: boolean;
  hideAddRowButton?: boolean;
  searchInputRef?: React.RefObject<HTMLInputElement | null>;
  onSaveRow?: () => void;
  onCancelEdit?: () => void;
  saving?: boolean;
}

export const DataGridToolbar = memo(function DataGridToolbar({
  globalSearch,
  setGlobalSearch,
  loadedRows,
  totalRowCount,
  filteredRows,
  serverSide = false,
  masterForm = false,
  onAddRow,
  isEditing = false,
  hideAddRowButton = false,
  searchInputRef,
  onSaveRow: _onSaveRow,
  onCancelEdit: _onCancelEdit,
  saving: _saving = false,
}: GridToolbarProps) {
  const { t } = useTranslation();
  const [mobileSearchOpen, setMobileSearchOpen] = useState(false);

  // Client mode: show filtered vs total when a filter is active.
  // Server mode: show loaded vs total-from-server to reflect pagination state.
  const _rowLabel = (() => {
    if (serverSide) {
      return loadedRows < totalRowCount
        ? t('grid.rows_loaded', { loaded: loadedRows.toLocaleString(), total: totalRowCount.toLocaleString() })
        : t('grid.rows_count', { count: loadedRows.toLocaleString() });
    }
    return filteredRows < totalRowCount
      ? t('grid.rows_filtered', { filtered: filteredRows.toLocaleString(), total: totalRowCount.toLocaleString() })
      : t('grid.rows_count', { count: totalRowCount.toLocaleString() });
  })();

  const isFiltered = serverSide ? loadedRows < totalRowCount : filteredRows < totalRowCount;

  return (
    <Box sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'divider' }}>
      {/* Main toolbar row */}
      <Box sx={{ display: 'flex', alignItems: 'center', px: 1.5, py: 0.75, gap: 1, minHeight: 40 }}>

        {/* Row count â€” hidden on xs */}
        <Box sx={{ flexGrow: 1, display: { xs: 'none', sm: 'flex' }, alignItems: 'center', gap: 1 }}>


          <AppTextField
            inputRef={searchInputRef}
            placeholder={t('actions.filter')}
            value={globalSearch}
            onChange={(val) => setGlobalSearch(String(val ?? ''))}
            sx={{
              width: { sm: 220, md: 280 },
              transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
              '& .MuiOutlinedInput-root': {
                height: 32,
                borderRadius: '2px',
                bgcolor: '#ffffff',
                fontSize: '13px',
                color: '#323130',
                '& fieldset': { 
                  borderColor: '#a6a6a6',
                  borderWidth: '1px',
                  transition: 'all 0.2s',
                },
                '&:hover fieldset': {
                  borderColor: '#323130',
                },
                '&.Mui-focused fieldset': {
                  borderWidth: '1px',
                  borderColor: 'primary.main',
                },
                '& .MuiInputBase-input::placeholder': {
                  color: '#8A9BB2',
                  opacity: 1,
                }
              },
            }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start" sx={{ mr: 0.5 }}>
                    <SearchIcon 
                      sx={{ 
                        fontSize: 18, 
                        color: '#605E5C',
                      }} 
                    />
                  </InputAdornment>
                ),
                endAdornment: globalSearch ? (
                  <InputAdornment position="end">
                    <IconButton aria-label={t('common.clear')} size="small" edge="end" onClick={() => setGlobalSearch('')} sx={{ p: 0.5 }}>
                      <CloseIcon sx={{ fontSize: 16 }} />
                    </IconButton>
                  </InputAdornment>
                ) : null,
              },
            }}
          />
        </Box>

        <DataGridSelectionSummary 
          serverSide={serverSide}
          loadedRows={loadedRows}
          totalRowCount={totalRowCount}
          filteredRows={filteredRows}
        />

        {/* Add Row button (master-form mode only) */}
        {(masterForm && !hideAddRowButton) && (
          <Tooltip title={isEditing ? t('grid.add_row_disabled') : t('grid.add_row')}>
            <span>
              <Button
                size="small"
                variant="outlined"
                color="primary"
                startIcon={<AddRowIcon sx={{ fontSize: 16 }} />}
                disabled={isEditing}
                onClick={onAddRow}
                sx={{
                  fontSize: '0.75rem',
                  height: 28,
                  px: 1.25,
                  borderRadius: '2px',
                  textTransform: 'none',
                  whiteSpace: 'nowrap',
                }}
              >
                {t('grid.add_row')}
              </Button>
            </span>
          </Tooltip>
        )}



        {/* Mobile search toggle */}
        <Box sx={{ display: { xs: 'flex', sm: 'none' } }}>
          <Tooltip title={mobileSearchOpen ? t('common.close') : t('common.search')}>
            <IconButton
              aria-label={mobileSearchOpen ? t('common.close') : t('common.search')}
              size="small"
              color={globalSearch ? 'primary' : 'default'}
              onClick={() => {
                setMobileSearchOpen(o => !o);
                if (mobileSearchOpen) setGlobalSearch('');
              }}
            >
              {mobileSearchOpen ? <CloseIcon fontSize="small" /> : <SearchIcon fontSize="small" />}
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Mobile search row */}
      <Collapse in={mobileSearchOpen} timeout={200}>
        <Box sx={{ display: { xs: 'flex', sm: 'none' }, px: 1.5, pb: 1, gap: 1, alignItems: 'center' }}>
          <AppTextField
            fullWidth
            placeholder={`${t('common.search')}...`}
            value={globalSearch}
            onChange={(val) => setGlobalSearch(String(val ?? ''))}
            slotProps={{
              input: {
                autoFocus: true,
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
          />
          {isFiltered && (
            <Typography variant="caption" color="primary" sx={{ whiteSpace: 'nowrap', fontWeight: 600 }}>
              {serverSide ? `${loadedRows}/${totalRowCount}` : `${filteredRows}/${totalRowCount}`}
            </Typography>
          )}
        </Box>
      </Collapse>
    </Box>
  );
});
