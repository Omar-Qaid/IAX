import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, InputAdornment } from '@mui/material';
import {
  Search as SearchIcon,
  DragIndicator as DragHandleIcon,
  TableRows as TableRowsIcon,
  Functions as FunctionsIcon,
} from '@mui/icons-material';
import type { ColumnDef } from '../Types';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';

interface ColumnsPanelProps<T> {
  columns: ColumnDef<T>[];
  setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
  searchTerm: string;
  setSearchTerm: (val: string) => void;
  pivotMode: boolean;
  setPivotMode: (val: boolean) => void;
}

export function ColumnsPanel<T>({
  columns, setColumns, searchTerm, setSearchTerm, pivotMode, setPivotMode
}: ColumnsPanelProps<T>) {
  const { t } = useTranslation();

  const handleToggleColumn = (field: string) => {
    setColumns(cols => cols.map(c => c.field === field ? { ...c, hidden: !c.hidden } : c));
  };

  const filteredColumns = columns.filter(c => t(c.headerName).toLowerCase().includes(searchTerm.toLowerCase()));

  return (
    <>
      <Box sx={{ p: 1, display: 'flex', alignItems: 'center', borderBottom: '1px solid #e0e0e0' }}>
        <AppBooleanField 
          value={pivotMode} 
          onChange={(v) => setPivotMode(v)} 
          sx={{ '& .MuiSwitch-root': { mr: 1 } }} 
        />
        <Typography variant="body2" sx={{ ml: 1, fontSize: '0.8rem', color: 'text.secondary' }}>{t('grid.pivot_mode')}</Typography>
      </Box>

      <Box sx={{ p: 1 }}>
        <AppTextField
          fullWidth
          placeholder={t('common.search')}
          value={searchTerm}
          onChange={(val: any) => setSearchTerm(val)}
          slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>, sx: { fontSize: '0.8rem', height: 30 } } }}
        />
      </Box>

      <Box sx={{ flexGrow: 1, minHeight: 0, overflow: 'auto', p: 1 }}>
        {filteredColumns.map(col => (
          <Box key={col.field as string} sx={{ display: 'flex', alignItems: 'center', mb: 0.5 }}>
            <DragHandleIcon fontSize="small" sx={{ color: 'action.disabled', mr: 0.5, cursor: 'grab', fontSize: 16 }} />
            <AppBooleanField
              value={!col.hidden}
              onChange={() => handleToggleColumn(col.field as string)}
              sx={{ p: 0.5, '& .MuiSvgIcon-root': { fontSize: 18 } }}
            />
            <Typography variant="body2" sx={{ fontSize: '0.8rem', ml: 0.5 }}>{t(col.headerName)}</Typography>
          </Box>
        ))}
      </Box>

      <Box sx={{ p: 1, borderTop: '1px solid #e0e0e0', bgcolor: '#fafafa' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <TableRowsIcon fontSize="small" sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
          <Typography variant="caption" sx={{ fontWeight: 600 }}>{t('grid.row_groups')}</Typography>
        </Box>
        <Box sx={{ border: '1px dashed #ccc', p: 1, mb: 2, textAlign: 'center', bgcolor: 'white', borderRadius: 1 }}>
          <Typography variant="caption" color="text.disabled">{t('grid.drag_here')}</Typography>
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
          <FunctionsIcon fontSize="small" sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
          <Typography variant="caption" sx={{ fontWeight: 600 }}>{t('grid.values')}</Typography>
        </Box>
        <Box sx={{ border: '1px dashed #ccc', p: 1, textAlign: 'center', bgcolor: 'white', borderRadius: 1 }}>
          <Typography variant="caption" color="text.disabled">{t('grid.drag_here_aggregate')}</Typography>
        </Box>
      </Box>
    </>
  );
}
