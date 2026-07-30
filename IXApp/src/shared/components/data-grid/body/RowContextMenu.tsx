import { Menu, MenuItem, ListItemIcon, ListItemText, Divider } from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  ContentCopy as CopyIcon,
  GetApp as ExportIcon,
  History as HistoryIcon,
  Construction as BuilderIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';

interface RowContextMenuProps<T> {
  contextMenu: { mouseX: number; mouseY: number; row: T | null } | null;
  onClose: () => void;
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
  onViewHistory?: (row: T) => void;
  onShowAllFields?: (row: T) => void;
  onBuild?: (row: T) => void;
  onCopyRow: (row: T) => void;
  onExportRow: (row: T) => void;
}

export function RowContextMenu<T>({
  contextMenu, onClose, onEdit, onDelete, onViewHistory, onShowAllFields, onBuild, onCopyRow, onExportRow
}: RowContextMenuProps<T>) {
  const { t } = useTranslation();
  if (!contextMenu || !contextMenu.row) return null;

  const row = contextMenu.row;

  return (
    <Menu
      open={contextMenu !== null}
      onClose={onClose}
      anchorReference="anchorPosition"
      anchorPosition={{ top: contextMenu.mouseY, left: contextMenu.mouseX }}
      slotProps={{ paper: { sx: { width: 230, borderRadius: 2, boxShadow: '0 4px 20px rgba(0,0,0,0.1)' } } }}
      disableRestoreFocus
      disableAutoFocusItem
      onContextMenu={(e) => { e.preventDefault(); onClose(); }}
    >
      {onEdit && (
        <MenuItem onClick={() => { onEdit(row); onClose(); }} sx={{ py: 1 }}>
          <ListItemIcon><EditIcon fontSize="small" color="primary" /></ListItemIcon>
          <ListItemText primary={t('common.edit')} />
        </MenuItem>
      )}
      {onBuild && (
        <MenuItem onClick={() => { onBuild(row); onClose(); }} sx={{ py: 1 }}>
          <ListItemIcon><BuilderIcon fontSize="small" color="primary" /></ListItemIcon>
          <ListItemText primary={t('workflow.builder', { defaultValue: 'Builder' })} />
        </MenuItem>
      )}
      {onDelete && (
        <MenuItem onClick={() => { onDelete(row); onClose(); }} sx={{ py: 1 }}>
          <ListItemIcon><DeleteIcon fontSize="small" color="error" /></ListItemIcon>
          <ListItemText primary={t('common.delete')} slotProps={{ primary: { color: 'error' } }} />
        </MenuItem>
      )}
      {onViewHistory && (
        <MenuItem onClick={() => { onViewHistory(row); onClose(); }} sx={{ py: 1 }}>
          <ListItemIcon><HistoryIcon fontSize="small" /></ListItemIcon>
          <ListItemText primary={t('common.view_history')} />
        </MenuItem>
      )}
      {onShowAllFields && (
        <MenuItem onClick={() => { onShowAllFields(row); onClose(); }} sx={{ py: 1 }}>
          <ListItemIcon><VisibilityIcon fontSize="small" /></ListItemIcon>
          <ListItemText primary={t('common.show_all_fields', { defaultValue: 'Show all fields' })} />
        </MenuItem>
      )}
      {(onEdit || onBuild || onDelete || onViewHistory || onShowAllFields) && <Divider sx={{ my: 1 }} />}
      <MenuItem onClick={() => { onCopyRow(row); onClose(); }} sx={{ py: 1 }}>
        <ListItemIcon><CopyIcon fontSize="small" /></ListItemIcon>
        <ListItemText primary={t('grid.copy_row')} />
      </MenuItem>
      <MenuItem onClick={() => { onExportRow(row); onClose(); }} sx={{ py: 1 }}>
        <ListItemIcon><ExportIcon fontSize="small" /></ListItemIcon>
        <ListItemText primary={t('grid.export_row')} />
      </MenuItem>
    </Menu>
  );
}
