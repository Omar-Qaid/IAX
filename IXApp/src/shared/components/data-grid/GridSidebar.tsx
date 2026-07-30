import React, { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box, Typography, IconButton, Divider,
} from '@mui/material';
import {
  ViewColumn as ViewColumnIcon,
  FilterList as FilterIcon,
  Close as CloseIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import type { ColumnDef, FilterModel, SelectionMode } from './Types';
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
    <Box sx={{ display: 'flex', height: '100%', borderLeft: '1px solid #e0e0e0', bgcolor: 'background.paper' }}>

      {/* Panel Content */}
      {activeTab ? (
        <Box sx={{ width: 260, display: 'flex', flexDirection: 'column', borderRight: '1px solid #e0e0e0', overflow: 'hidden' }}>

          {/* Panel header with close */}
          <Box sx={{ display: 'flex', alignItems: 'center', px: 1.5, py: 0.75, borderBottom: '1px solid #e0e0e0', minHeight: 40 }}>
            <Typography variant="subtitle2" sx={{ flexGrow: 1, fontWeight: 600, fontSize: '0.8rem' }}>
              {activeTab === 'columns' ? t('grid.choose_columns') : activeTab === 'filters' ? t('grid.filters') : t('grid.features')}
            </Typography>
            <IconButton size="small" sx={{ p: 0.25 }} onClick={() => { setActiveTab(null); onClose(); }}>
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
      <Box sx={{ width: 40, display: 'flex', flexDirection: 'column', alignItems: 'center', pt: 1, bgcolor: '#f8f9fa' }}>
        <SidebarTab
          active={activeTab === 'columns'}
          onClick={() => handleTabClick('columns')}
          icon={<ViewColumnIcon fontSize="small" sx={{ transform: 'rotate(90deg)' }} />}
          label={t('grid.choose_columns')}
        />

        <Divider flexItem sx={{ my: 0.5 }} />

        <SidebarTab
          active={activeTab === 'filters'}
          onClick={() => handleTabClick('filters')}
          icon={<FilterIcon fontSize="small" sx={{ transform: 'rotate(90deg)' }} />}
          label={t('grid.filters')}
          badgeCount={filters.length}
        />

        <Divider flexItem sx={{ my: 0.5 }} />

        <SidebarTab
          active={activeTab === 'features'}
          onClick={() => handleTabClick('features')}
          icon={<SettingsIcon fontSize="small" sx={{ transform: 'rotate(90deg)' }} />}
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
  return (
    <IconButton
      size="small"
      onClick={onClick}
      sx={{
        borderRadius: 0,
        width: '100%',
        py: 1.5,
        color: active ? 'primary.main' : 'text.secondary',
        bgcolor: active ? 'white' : 'transparent',
        position: 'relative'
      }}
    >
      <Box sx={{ writingMode: 'vertical-rl', display: 'flex', alignItems: 'center', gap: 1 }}>
        {icon}
        <Typography variant="caption" sx={{ letterSpacing: 1, fontWeight: 600 }}>{label}</Typography>
      </Box>
      {badgeCount && badgeCount > 0 && (
        <Box sx={{
          position: 'absolute', top: 6, right: 4, width: 14, height: 14,
          bgcolor: 'primary.main', borderRadius: '50%',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}>
          <Typography sx={{ fontSize: '0.6rem', color: 'white', lineHeight: 1 }}>
            {badgeCount}
          </Typography>
        </Box>
      )}
    </IconButton>
  );
}
