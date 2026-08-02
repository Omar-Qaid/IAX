import React, { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Badge, Box, Typography, IconButton, Tooltip,
} from '@mui/material';
import {
  ViewColumn as ViewColumnIcon,
  FilterList as FilterIcon,
  Close as CloseIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import type { ColumnDef, FilterModel, SelectionMode } from './types';
import { ColumnsPanel } from './sidebar/ColumnsPanel';
import { FiltersPanel } from './sidebar/FiltersPanel';
import { FeaturesPanel } from './sidebar/FeaturesPanel';

interface GridSidebarProps<T> {
  open: boolean;
  onOpen: () => void;
  onClose: () => void;
  activeTab: 'columns' | 'filters' | 'features' | null;
  setActiveTab: (tab: 'columns' | 'filters' | 'features' | null) => void;
  columns: ColumnDef<T>[];
  setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  selectionMode: SelectionMode;
  setSelectionMode: (mode: SelectionMode) => void;
  onExport: () => void;
  onServerImport?: (file: File) => Promise<void> | void;
  onDownloadTemplate?: () => Promise<void> | void;
  onAutosizeAll: () => void;
  onAutosizeColumn: (field: string) => void;
  onUnAutosizeColumn: (field: string) => void;
  onResetColumns: () => void;
  isAutosized: boolean;
  rowHeight: number;
  setRowHeight: (height: number) => void;
  showColumnBorders: boolean;
  setShowColumnBorders: (show: boolean) => void;
  showCellBorders: boolean;
  setShowCellBorders: (show: boolean) => void;
}
  
export function GridSidebar<T>({
  open, onOpen, onClose, activeTab, setActiveTab, columns, setColumns, filters, setFilters, selectionMode, setSelectionMode,
  onExport, onServerImport, onDownloadTemplate, onAutosizeAll, onAutosizeColumn,
  onUnAutosizeColumn, onResetColumns, isAutosized, rowHeight, setRowHeight,
  showColumnBorders, setShowColumnBorders, showCellBorders, setShowCellBorders,
}: GridSidebarProps<T>) {

  const { t } = useTranslation();

  const OPERATOR_LABELS: Record<FilterModel['operator'], string> = useMemo(() => ({
    contains: t('grid.operators.contains'),
    doesNotContain: t('grid.operators.does_not_contain'),
    equals: t('grid.operators.equals'),
    notEquals: t('grid.operators.not_equals'),
    startsWith: t('grid.operators.starts_with'),
    endsWith: t('grid.operators.ends_with'),
    gt: t('grid.operators.greater_than'),
    lt: t('grid.operators.less_than'),
    in: t('grid.operators.is_one_of'),
    matches: t('grid.operators.matches_regex'),
  }), [t]);


  const [searchTerm, setSearchTerm] = useState('');
  const [pivotMode, setPivotMode] = useState(false);

  const [prevOpen, setPrevOpen] = useState(open);
  if (open !== prevOpen) {
    setPrevOpen(open);
    if (!open && activeTab) {
      setActiveTab(null);
    } else if (open && !activeTab) {
      setActiveTab('columns');
    }
  }

  const handleTabClick = (tab: 'columns' | 'filters' | 'features') => {
    if (activeTab === tab) {
      setActiveTab(null);
      onClose();
    } else {
      setActiveTab(tab);
      if (!open) onOpen();
    }
  };

  return (
    <Box sx={{ display: 'flex', height: '100%', minHeight: 0, borderInlineStart: (theme) => `1px solid ${theme.palette.divider}`, bgcolor: 'background.paper', boxShadow: open ? '-2px 0 8px rgba(0,0,0,0.06)' : 'none', zIndex: 5 }}>

      {/* Panel Content */}
      {activeTab ? (
        <Box sx={{ width: 248, display: 'flex', flexDirection: 'column', borderInlineEnd: (theme) => `1px solid ${theme.palette.divider}`, overflow: 'hidden', bgcolor: 'background.paper' }}>

          {/* Panel header with close */}
          <Box sx={{ display: 'flex', alignItems: 'center', px: 1.5, borderBottom: (theme) => `1px solid ${theme.palette.divider}`, minHeight: 44, bgcolor: (theme) => theme.palette.mode === 'light' ? '#f3f2f1' : 'background.default' }}>
            <Typography variant="subtitle2" sx={{ flexGrow: 1, fontWeight: 600, fontSize: '0.8125rem' }}>
              {activeTab === 'columns' ? t('grid.choose_columns') : activeTab === 'filters' ? t('grid.filters') : t('grid.features')}
            </Typography>
            <IconButton size="small" aria-label={t('common.close')} sx={{ p: 0.5, borderRadius: 0.5 }} onClick={() => { setActiveTab(null); onClose(); }}>
              <CloseIcon sx={{ fontSize: 16 }} />
            </IconButton>
          </Box>

          {activeTab === 'columns' && (
            <ColumnsPanel
              columns={columns}
              setColumns={setColumns}
              searchTerm={searchTerm}
              setSearchTerm={setSearchTerm}
              pivotMode={pivotMode}
              setPivotMode={setPivotMode}
            />
          )}

          {activeTab === 'filters' && (
            <FiltersPanel
              filters={filters}
              setFilters={setFilters}
              columns={columns}
              operatorLabels={OPERATOR_LABELS}
            />
          )}

          {activeTab === 'features' && (
            <FeaturesPanel
              columns={columns}
              selectionMode={selectionMode}
              setSelectionMode={setSelectionMode}
              onExport={onExport}
              onServerImport={onServerImport}
              onDownloadTemplate={onDownloadTemplate}
              onAutosizeAll={onAutosizeAll}
              onAutosizeColumn={onAutosizeColumn}
              onUnAutosizeColumn={onUnAutosizeColumn}
              onResetColumns={onResetColumns}
              isAutosized={isAutosized}
              rowHeight={rowHeight}
              setRowHeight={setRowHeight}
              showColumnBorders={showColumnBorders}
              setShowColumnBorders={setShowColumnBorders}
              showCellBorders={showCellBorders}
              setShowCellBorders={setShowCellBorders}
              setActiveTab={setActiveTab}
            />
          )}
        </Box>
      ) : null}

      {/* Vertical Tab Strip (Right Edge) */}
      <Box component="nav" aria-label={t('grid.features')} sx={{ width: 38, flexShrink: 0, display: 'flex', flexDirection: 'column', alignItems: 'stretch', py: 0.5, gap: 0.25, bgcolor: (theme) => theme.palette.mode === 'light' ? '#f3f2f1' : 'background.default' }}>
        <SidebarTab
          active={activeTab === 'columns'}
          onClick={() => handleTabClick('columns')}
          icon={<ViewColumnIcon sx={{ fontSize: 19 }} />}
          label={t('grid.choose_columns')}
        />

        <SidebarTab
          active={activeTab === 'filters'}
          onClick={() => handleTabClick('filters')}
          icon={<FilterIcon sx={{ fontSize: 19 }} />}
          label={t('grid.filters')}
          badgeCount={filters.length}
        />

        <SidebarTab
          active={activeTab === 'features'}
          onClick={() => handleTabClick('features')}
          icon={<SettingsIcon sx={{ fontSize: 19 }} />}
          label={t('grid.features')}
        />
      </Box>

    </Box>
  );
}

interface SidebarTabProps {
  active: boolean;
  onClick: () => void;
  icon: React.ReactNode;
  label: string;
  badgeCount?: number;
}

function SidebarTab({ active, onClick, icon, label, badgeCount }: SidebarTabProps) {
  return <Tooltip title={label} placement="left">
    <IconButton
      size="small" aria-label={label} aria-pressed={active} onClick={onClick}
      sx={{ borderRadius: 0.5, width: 32, height: 32, mx: '3px', color: active ? 'primary.main' : 'text.secondary', bgcolor: active ? 'background.paper' : 'transparent', border: '1px solid', borderColor: active ? 'primary.main' : 'transparent', '&:hover': { bgcolor: 'background.paper', color: 'primary.main' } }}
    >
      <Badge badgeContent={badgeCount} color="primary" max={99} sx={{ '& .MuiBadge-badge': { minWidth: 14, height: 14, px: 0.25, fontSize: '0.5625rem' } }}>{icon}</Badge>
    </IconButton>
  </Tooltip>;
}
