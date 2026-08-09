import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton, Divider, Button } from '@mui/material';
import PaletteIcon from '@mui/icons-material/Palette';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ResizeIcon from '@mui/icons-material/AspectRatio';
import ResetIcon from '@mui/icons-material/SettingsBackupRestore';
import ExportIcon from '@mui/icons-material/GetApp';
import ImportIcon from '@mui/icons-material/Publish';
import TemplateIcon from '@mui/icons-material/Description';
import CompactIcon from '@mui/icons-material/DensitySmall';
import StandardIcon from '@mui/icons-material/DensityMedium';
import ComfortableIcon from '@mui/icons-material/DensityLarge';
import RadioCheckedIcon from '@mui/icons-material/RadioButtonChecked';
import RadioUncheckedIcon from '@mui/icons-material/RadioButtonUnchecked';
import BorderAllIcon from '@mui/icons-material/BorderAll';
import BorderVerticalIcon from '@mui/icons-material/BorderVertical';
import type { ColumnDef, SelectionMode } from '../types';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';

interface FeaturesPanelProps<T> {
  columns: ColumnDef<T>[];
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
  setActiveTab: (tab: 'columns' | 'filters' | 'features') => void;
}

export function FeaturesPanel<T>({
  columns,
  selectionMode,
  setSelectionMode,
  onExport,
  onServerImport,
  onDownloadTemplate,
  onAutosizeAll,
  onAutosizeColumn,
  onUnAutosizeColumn,
  onResetColumns,
  isAutosized,
  rowHeight,
  setRowHeight,
  showColumnBorders,
  setShowColumnBorders,
  showCellBorders,
  setShowCellBorders,
  setActiveTab,
}: FeaturesPanelProps<T>) {
  const { t } = useTranslation();
  const [importing, setImporting] = useState(false);
  const fileInputRef = React.useRef<HTMLInputElement | null>(null);

  const handleImportClick = () => fileInputRef.current?.click();

  const handleFileChosen = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file || !onServerImport) return;
    setImporting(true);
    try {
      await onServerImport(file);
    } finally {
      setImporting(false);
    }
  };

  return (
    <Box sx={{ flexGrow: 1, minHeight: 0, overflow: 'auto', p: 1.5 }}>
      <Box sx={{ mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
          <PaletteIcon sx={{ fontSize: 18, mr: 1, color: 'primary.main' }} />
          <Typography variant="subtitle2" sx={{ fontSize: '0.85rem', fontWeight: 600 }}>
            {t('grid.themes')}
          </Typography>
        </Box>

        <Box sx={{ mb: 2 }}>
          <Box sx={{ pl: 3.5, display: 'flex', flexDirection: 'column', gap: 1 }}>
            {[
              {
                label: t('grid.compact_density'),
                value: 28,
                icon: <CompactIcon sx={{ fontSize: 18 }} />,
              },
              {
                label: t('grid.standard_density'),
                value: 36,
                icon: <StandardIcon sx={{ fontSize: 18 }} />,
              },
              {
                label: t('grid.comfortable_density'),
                value: 48,
                icon: <ComfortableIcon sx={{ fontSize: 18 }} />,
              },
            ].map((d) => (
              <Box
                key={d.label}
                onClick={() => setRowHeight(d.value)}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 2,
                  cursor: 'pointer',
                  p: '8px 12px',
                  borderRadius: 1.5,
                  bgcolor: rowHeight === d.value ? '#f0f4ff' : 'transparent',
                  color: rowHeight === d.value ? 'primary.main' : 'text.primary',
                  '&:hover': { bgcolor: rowHeight === d.value ? '#f0f4ff' : '#f5f5f5' },
                  transition: 'all 0.2s',
                }}
              >
                <Box sx={{ color: rowHeight === d.value ? 'primary.main' : 'text.secondary' }}>
                  {d.icon}
                </Box>
                <Typography
                  variant="body2"
                  sx={{ fontSize: '0.85rem', fontWeight: rowHeight === d.value ? 600 : 400 }}
                >
                  {d.label}
                </Typography>
              </Box>
            ))}
          </Box>
        </Box>

        <Box sx={{ pl: 3.5 }}>
          <Box
            sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <BorderVerticalIcon sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" sx={{ fontSize: '0.75rem' }}>
                {t('grid.show_column_borders')}
              </Typography>
            </Box>
            <AppBooleanField value={showColumnBorders} onChange={(v) => setShowColumnBorders(v)} />
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <BorderAllIcon sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
              <Typography variant="body2" sx={{ fontSize: '0.75rem' }}>
                {t('grid.show_cell_borders')}
              </Typography>
            </Box>
            <AppBooleanField value={showCellBorders} onChange={(v) => setShowCellBorders(v)} />
          </Box>
        </Box>
      </Box>

      <Divider sx={{ mb: 3 }} />

      <Box sx={{ mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
          <CheckCircleIcon sx={{ fontSize: 18, mr: 1, color: 'primary.main' }} />
          <Typography variant="subtitle2" sx={{ fontSize: '0.85rem', fontWeight: 600 }}>
            {t('grid.row_selection')}
          </Typography>
        </Box>
        <Box sx={{ pl: 3.5, display: 'flex', flexDirection: 'column', gap: 0.75 }}>
          {(['single', 'multiple'] as SelectionMode[]).map((mode) => (
            <Box
              key={mode}
              onClick={() => setSelectionMode(mode)}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.5,
                cursor: 'pointer',
                p: '8px 12px',
                borderRadius: 1.5,
                bgcolor: selectionMode === mode ? '#f0f4ff' : 'transparent',
                color: selectionMode === mode ? 'primary.main' : 'text.primary',
                '&:hover': { bgcolor: selectionMode === mode ? '#f0f4ff' : '#f5f5f5' },
                transition: 'all 0.2s',
                border: '1px solid',
                borderColor: selectionMode === mode ? 'primary.100' : 'transparent',
              }}
            >
              <Box
                sx={{
                  color: selectionMode === mode ? 'primary.main' : 'text.secondary',
                  display: 'flex',
                }}
              >
                {selectionMode === mode ? (
                  <RadioCheckedIcon sx={{ fontSize: 18 }} />
                ) : (
                  <RadioUncheckedIcon sx={{ fontSize: 18 }} />
                )}
              </Box>
              <Box>
                <Typography
                  variant="body2"
                  sx={{
                    fontSize: '0.85rem',
                    fontWeight: selectionMode === mode ? 600 : 400,
                    textTransform: 'capitalize',
                  }}
                >
                  {mode === 'single' ? t('grid.single_selection') : t('grid.multiple_selection')}
                </Typography>
                {mode === 'single' && (
                  <Typography
                    variant="caption"
                    sx={{ color: 'text.disabled', display: 'block', fontSize: '0.7rem' }}
                  >
                    {t('grid.single_selection_desc')}
                  </Typography>
                )}
                {mode === 'multiple' && (
                  <Typography
                    variant="caption"
                    sx={{ color: 'text.disabled', display: 'block', fontSize: '0.7rem' }}
                  >
                    {t('grid.multiple_selection_desc')}
                  </Typography>
                )}
              </Box>
            </Box>
          ))}
        </Box>
      </Box>

      <Divider sx={{ mb: 3 }} />

      <Box sx={{ mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
          <ResizeIcon sx={{ fontSize: 18, mr: 1, color: 'primary.main' }} />
          <Typography variant="subtitle2" sx={{ fontSize: '0.85rem', fontWeight: 600 }}>
            {t('grid.column_sizing')}
          </Typography>
        </Box>
        <Box sx={{ pl: 3.5, display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Button
            size="small"
            variant={isAutosized ? 'contained' : 'outlined'}
            fullWidth
            onClick={isAutosized ? onResetColumns : onAutosizeAll}
            sx={{
              textTransform: 'none',
              fontSize: '0.75rem',
              mb: 1,
              ...(isAutosized && {
                bgcolor: 'primary.main',
                '&:hover': { bgcolor: 'primary.dark' },
              }),
            }}
          >
            {isAutosized ? t('grid.un_autosize_all') : t('grid.autosize_all')}
          </Button>

          <Typography
            variant="caption"
            sx={{ color: 'text.secondary', fontWeight: 600, mb: 0.5, display: 'block' }}
          >
            {t('grid.individual_columns')}:
          </Typography>
          <Box sx={{ maxHeight: 200, overflow: 'auto', pr: 0.5 }}>
            {columns
              .filter((c) => !c.hidden)
              .map((col) => (
                <Box
                  key={col.field as string}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    p: '2px 4px',
                    borderRadius: 0.5,
                    '&:hover': { bgcolor: '#f0f0f0' },
                  }}
                >
                  <Typography
                    variant="body2"
                    sx={{
                      fontSize: '0.75rem',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                      flexGrow: 1,
                    }}
                  >
                    {col.headerName}
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <IconButton
                      size="small"
                      title="Autosize"
                      onClick={() => onAutosizeColumn(col.field as string)}
                      sx={{ p: 0.25, color: 'primary.main' }}
                    >
                      <ResizeIcon sx={{ fontSize: 14 }} />
                    </IconButton>
                    <IconButton
                      size="small"
                      title="Reset Sizing"
                      onClick={() => onUnAutosizeColumn(col.field as string)}
                      sx={{ p: 0.25, color: 'text.secondary' }}
                    >
                      <ResetIcon sx={{ fontSize: 14 }} />
                    </IconButton>
                  </Box>
                </Box>
              ))}
          </Box>

          <Button
            size="small"
            variant="text"
            fullWidth
            onClick={() => setActiveTab('columns')}
            sx={{ textTransform: 'none', fontSize: '0.75rem', justifyContent: 'flex-start', mt: 1 }}
          >
            {t('grid.choose_columns')}...
          </Button>
        </Box>
      </Box>

      <Divider sx={{ mb: 3 }} />

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        <Button
          size="small"
          variant="contained"
          fullWidth
          startIcon={<ExportIcon />}
          onClick={onExport}
          sx={{ textTransform: 'none', fontSize: '0.8rem', bgcolor: 'primary.main' }}
        >
          {t('grid.export')}
        </Button>

        {onServerImport && (
          <>
            <Button
              size="small"
              variant="outlined"
              fullWidth
              disabled={importing}
              startIcon={<ImportIcon />}
              onClick={handleImportClick}
              sx={{ textTransform: 'none', fontSize: '0.8rem' }}
            >
              {importing ? t('common.importing') : t('grid.import_excel')}
            </Button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
              style={{ display: 'none' }}
              onChange={handleFileChosen}
            />
          </>
        )}

        {onDownloadTemplate && (
          <Button
            size="small"
            variant="text"
            fullWidth
            startIcon={<TemplateIcon />}
            onClick={() => onDownloadTemplate()}
            sx={{ textTransform: 'none', fontSize: '0.75rem' }}
          >
            {t('grid.download_template')}
          </Button>
        )}

        <Button
          size="small"
          variant="outlined"
          fullWidth
          onClick={onResetColumns}
          sx={{
            textTransform: 'none',
            fontSize: '0.8rem',
            color: 'error.main',
            borderColor: 'error.light',
            '&:hover': { borderColor: 'error.main', bgcolor: 'error.50' },
          }}
        >
          {t('grid.reset_layout')}
        </Button>
      </Box>
    </Box>
  );
}
