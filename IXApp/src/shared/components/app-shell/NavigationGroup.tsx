import React, { useState } from 'react';
import { List, ListItemButton, ListItemIcon, ListItemText, Collapse, Typography } from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';

export interface NavigationGroupProps {
  title: string;
  icon?: React.ReactNode;
  defaultExpanded?: boolean;
  children: React.ReactNode;
}

export const NavigationGroup: React.FC<NavigationGroupProps> = ({
  title,
  icon,
  defaultExpanded = true,
  children,
}) => {
  const [open, setOpen] = useState(defaultExpanded);

  return (
    <List component="div" disablePadding sx={{ mb: 1 }}>
      <ListItemButton onClick={() => setOpen(!open)} sx={{ py: 0.5, px: 1.5, borderRadius: 1 }}>
        {icon && <ListItemIcon sx={{ minWidth: 28, color: 'text.secondary' }}>{icon}</ListItemIcon>}
        <ListItemText
          primary={
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: 0.5 }}>
              {title}
            </Typography>
          }
        />
        {open ? <ExpandLess fontSize="small" color="action" /> : <ExpandMore fontSize="small" color="action" />}
      </ListItemButton>
      <Collapse in={open} timeout="auto" unmountOnExit>
        <List component="div" disablePadding sx={{ pl: 1 }}>
          {children}
        </List>
      </Collapse>
    </List>
  );
};
