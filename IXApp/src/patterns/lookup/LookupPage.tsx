import { useMemo, useState, type ReactNode } from 'react';
import { Box, InputAdornment, TextField } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

export interface LookupPageProps<T> { title: string; subtitle?: string; rows: T[]; columns: ColumnDef<T>[]; getRowId: (row: T) => string | number; getSearchText?: (row: T) => string; filterLabel?: string; loading?: boolean; actions?: ReactNode; onSelect?: (row: T) => void }
export function LookupPage<T>({ title, subtitle, rows, columns, getRowId, getSearchText = row => JSON.stringify(row), filterLabel = 'Filter', loading, actions, onSelect }: LookupPageProps<T>) {
  const [query, setQuery] = useState('');
  const filtered = useMemo(() => { const value = query.trim().toLocaleLowerCase(); return value ? rows.filter(row => getSearchText(row).toLocaleLowerCase().includes(value)) : rows; }, [getSearchText, query, rows]);
  return <PageContainer>
    <PageHeader title={title} subtitle={subtitle} actions={actions} />
    <Box sx={{ mb: 1, maxWidth: 320 }}><TextField fullWidth size="small" value={query} onChange={event => setQuery(event.target.value)} label={filterLabel} slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment> } }} /></Box>
    <DataGrid rows={filtered} columns={columns} getRowId={getRowId} loading={loading} onRowDoubleClick={onSelect} selectionMode="single" />
  </PageContainer>;
}
