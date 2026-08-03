import { useState, type MouseEvent, type ReactNode } from 'react';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { Button, ListItemIcon, ListItemText, Menu, MenuItem } from '@mui/material';
import type { ActionDefinition } from './types';

export interface ActionPaneMenuProps { label: string; icon?: ReactNode; actions: ActionDefinition[]; disabled?: boolean }
export function ActionPaneMenu({ label, icon, actions, disabled }: ActionPaneMenuProps) {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const openMenu = (event: MouseEvent<HTMLButtonElement>) => setAnchorEl(event.currentTarget);
  const closeMenu = () => setAnchorEl(null);
  const visible = actions.filter(action => !action.hidden).sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
  return <>
    <Button size="small" variant="text" startIcon={icon} endIcon={<ExpandMoreIcon />} onClick={openMenu} disabled={disabled || visible.length === 0} aria-haspopup="menu" aria-expanded={Boolean(anchorEl)}>{label}</Button>
    <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={closeMenu}>
      {visible.map(action => <MenuItem key={action.id} disabled={action.disabled || action.loading || !action.onClick} onClick={() => { action.onClick?.(); closeMenu(); }}>
        {action.icon && <ListItemIcon>{action.icon}</ListItemIcon>}<ListItemText primary={action.label} secondary={action.keyboardShortcut} />
      </MenuItem>)}
    </Menu>
  </>;
}
