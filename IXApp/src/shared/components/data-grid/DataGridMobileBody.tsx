import React, { useMemo, useCallback, memo } from 'react';
import {
    Box, Typography, CircularProgress, useTheme, Card, IconButton, Divider, Chip, Checkbox
} from '@mui/material';
import {
    Inbox as InboxIcon,
    SearchOff as SearchOffIcon,
    Edit as EditIcon,
    Delete as DeleteIcon,
    History as HistoryIcon,
} from '@mui/icons-material';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useTranslation } from 'react-i18next';
import type { ColumnDef } from './types';
import { getNestedValue } from './DataGridUtils';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';

interface MobileGridBodyProps<T> {
    rows: T[];
    columns: ColumnDef<T>[];
    getRowId: (row: T) => string | number;
    scrollContainerRef: React.RefObject<HTMLDivElement | null>;
    loading?: boolean;
    hasMore?: boolean;
    hasActiveFilters?: boolean;
    onRowClick?: (row: T) => void;
    onRowDoubleClick?: (row: T) => void;
    onEdit?: (row: T) => void;
    onDelete?: (row: T) => void;
    onViewHistory?: (row: T) => void;
    selectionMode?: 'none' | 'single' | 'multiple';
    selectedIds?: (string | number)[];
    onSelectionChange?: (ids: (string | number)[]) => void;
}

export function DataGridMobileBodyInternal<T>({
    rows, columns, getRowId, scrollContainerRef,
    loading, hasMore, hasActiveFilters, onRowClick, onRowDoubleClick, onEdit, onDelete, onViewHistory,
    selectionMode = 'single', selectedIds = [], onSelectionChange,
}: MobileGridBodyProps<T>) {
    const { t } = useTranslation();
    const theme = useTheme();

    const selectedSet = useMemo(() => new Set(selectedIds), [selectedIds]);

    const onToggleRow = useCallback((rowId: string | number, event?: React.MouseEvent) => {
        if (event) {
            event.stopPropagation();
        }
        if (selectionMode === 'single') {
            onSelectionChange?.(selectedSet.has(rowId) ? [] : [rowId]);
        } else if (selectionMode === 'multiple') {
            const next = new Set(selectedSet);
            if (next.has(rowId)) next.delete(rowId); else next.add(rowId);
            onSelectionChange?.(Array.from(next));
        }
    }, [selectionMode, onSelectionChange, selectedSet]);

    // Use a dynamic virtualizer for cards which vary in height
    const rowVirtualizer = useVirtualizer({
        count: rows.length,
        getScrollElement: () => scrollContainerRef.current,
        estimateSize: () => 180, // rough estimate of a card height
        overscan: 5,
    });

    const visibleColumns = useMemo(() => columns.filter(c => !c.hidden), [columns]);

    const renderCell = useCallback((row: T, col: ColumnDef<T>, rowIndex: number) => {
        if (col.renderCell) {
            return col.renderCell({ row, value: getNestedValue(row, col.field as string), rowIndex });
        }
        const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);
        const isBool = col.type === 'boolean' || typeof val === 'boolean' || (typeof val === 'string' && (val === 'true' || val === 'false'));
        if (isBool) {
            const boolVal = val === true || String(val) === 'true' || val === 1;
            return (
                <Box sx={{ pointerEvents: 'none' }}>
                    <AppBooleanField
                        name={col.field as string}
                        value={boolVal}
                        readOnly
                    />
                </Box>
            );
        }
        return (
            <Typography variant="body2" sx={{ color: 'text.primary', fontWeight: 500, wordBreak: 'break-word' }}>
                {val != null ? String(val) : '-'}
            </Typography>
        );
    }, [t]);

    if (loading && rows.length === 0) {
        return (
            <Box sx={{ p: 2 }}>
                {[1, 2, 3].map(i => (
                    <Card key={i} sx={{ mb: 2, height: 160, bgcolor: theme.palette.action.hover, borderRadius: 2, boxShadow: 'none' }} />
                ))}
            </Box>
        );
    }

    if (!loading && rows.length === 0) {
        return (
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 10, px: 4, userSelect: 'none' }}>
                <Box sx={{ width: 72, height: 72, borderRadius: '50%', bgcolor: theme.palette.action.hover, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
                    {hasActiveFilters ? <SearchOffIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} /> : <InboxIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} />}
                </Box>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, color: 'text.secondary', mb: 0.5 }}>
                    {hasActiveFilters ? t('grid.no_records') : t('grid.no_data')}
                </Typography>
                <Typography variant="body2" sx={{ color: 'text.disabled', textAlign: 'center', maxWidth: 280, lineHeight: 1.6 }}>
                    {hasActiveFilters ? t('grid.no_results_msg') : t('grid.no_records_msg')}
                </Typography>
            </Box>
        );
    }

    return (
        <>
            <Box sx={{ height: `${rowVirtualizer.getTotalSize()}px`, width: '100%', position: 'relative' }}>
                {rowVirtualizer.getVirtualItems().map(virtualRow => {
                    const row = rows[virtualRow.index];
                    const rowId = getRowId(row);
                    const isSelected = selectedSet.has(rowId);
                    
                    // Pick the first column as the card title, second as status (if boolean or short text), rest as fields
                    const titleCol = visibleColumns.length > 0 ? visibleColumns[0] : null;
                    const statusCol = visibleColumns.find(c => c.type === 'boolean' || c.field === 'status' || c.field === 'isActive');
                    
                    // Filter out title and status columns from the general details list
                    const detailCols = visibleColumns.filter(c => c !== titleCol && c !== statusCol);

                    return (
                        <Box
                            key={rowId}
                            style={{
                                position: 'absolute',
                                top: 0,
                                left: 0,
                                width: '100%',
                                transform: `translateY(${virtualRow.start}px)`,
                                padding: '8px 12px',
                            }}
                            ref={rowVirtualizer.measureElement}
                            data-index={virtualRow.index}
                        >
                            <Card 
                                onClick={() => onRowClick?.(row)}
                                onDoubleClick={() => onRowDoubleClick?.(row)}
                                sx={{ 
                                    p: 2, 
                                    borderRadius: 3, 
                                    boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                                    border: '1px solid',
                                    borderColor: isSelected ? 'primary.main' : 'divider',
                                    bgcolor: isSelected ? 'primary.50' : 'background.paper',
                                    cursor: onRowClick ? 'pointer' : 'default',
                                    transition: 'all 0.2s',
                                    position: 'relative'
                                }}
                            >
                                {/* Selection Checkbox */}
                                {selectionMode !== 'none' && (
                                    <Box sx={{ position: 'absolute', top: 8, right: 8, zIndex: 2 }}>
                                        <Checkbox 
                                            checked={isSelected}
                                            onClick={(e) => onToggleRow(rowId, e)}
                                            size="small"
                                        />
                                    </Box>
                                )}

                                {/* Header */}
                                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2, pr: selectionMode !== 'none' ? 4 : 0 }}>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                                        {titleCol && (
                                            <>
                                                <Typography sx={{ fontWeight: 700, color: 'primary.main', fontSize: '0.95rem' }}>
                                                    {titleCol.headerName}:
                                                </Typography>
                                                <Box sx={{ 
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    '& .MuiTypography-root': { 
                                                        fontWeight: 600, 
                                                        color: 'text.primary', 
                                                        fontSize: '0.95rem',
                                                        margin: 0
                                                    } 
                                                }}>
                                                    {renderCell(row, titleCol, virtualRow.index)}
                                                </Box>
                                            </>
                                        )}
                                    </Box>
                                    {statusCol && (
                                        <Box sx={{ ml: 2, flexShrink: 0 }}>
                                            {renderCell(row, statusCol, virtualRow.index)}
                                        </Box>
                                    )}
                                </Box>

                                {/* Details */}
                                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 2 }}>
                                    {detailCols.map(col => (
                                        <Box key={col.field as string} sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
                                            <Typography variant="body2" sx={{ color: 'text.secondary', minWidth: '80px', flexShrink: 0, pt: '1px' }}>
                                                {col.headerName}:
                                            </Typography>
                                            <Box sx={{ flexGrow: 1, '& .MuiTypography-root': { color: 'text.primary', fontWeight: 500 } }}>
                                                {renderCell(row, col, virtualRow.index)}
                                            </Box>
                                        </Box>
                                    ))}
                                </Box>

                                {/* Actions */}
                                {(onEdit || onDelete || onViewHistory) && (
                                    <>
                                        <Divider sx={{ mx: -2, mb: 1 }} />
                                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5 }}>
                                            {onViewHistory && (
                                                <IconButton size="small" onClick={(e) => { e.stopPropagation(); onViewHistory(row); }} sx={{ color: 'text.secondary', border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
                                                    <HistoryIcon sx={{ fontSize: 18 }} />
                                                </IconButton>
                                            )}
                                            {onEdit && (
                                                <IconButton size="small" onClick={(e) => { e.stopPropagation(); onEdit(row); }} sx={{ color: 'primary.main', border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
                                                    <EditIcon sx={{ fontSize: 18 }} />
                                                </IconButton>
                                            )}
                                            {onDelete && (
                                                <IconButton size="small" onClick={(e) => { e.stopPropagation(); onDelete(row); }} sx={{ color: 'error.main', border: '1px solid', borderColor: 'error.light', borderRadius: 2 }}>
                                                    <DeleteIcon sx={{ fontSize: 18 }} />
                                                </IconButton>
                                            )}
                                        </Box>
                                    </>
                                )}
                            </Card>
                        </Box>
                    );
                })}
            </Box>

            {hasMore && loading && (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 1, py: 2 }}>
                    <CircularProgress size={16} thickness={4} />
                    <Typography variant="caption" color="text.disabled">{t('grid.loading_more')}</Typography>
                </Box>
            )}
        </>
    );
}

export const DataGridMobileBody = memo(DataGridMobileBodyInternal) as typeof DataGridMobileBodyInternal;
