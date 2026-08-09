import React, { useState } from 'react';
import { Button, Menu, MenuItem, ListItemText, Typography } from '@mui/material';
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import { useAppStore } from '@app/store/useAppStore';
import { AVAILABLE_COMPANIES } from '@core/constants/appConstants';

export const CompanySelector: React.FC = () => {
  const currentCompany = useAppStore((s) => s.currentCompany);
  const setCompany = useAppStore((s) => s.setCompany);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const activeCompanyObj =
    AVAILABLE_COMPANIES.find((c) => c.code === currentCompany) || AVAILABLE_COMPANIES[0]!;

  const handleOpen = (e: React.MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);
  const handleSelect = (code: string) => {
    setCompany(code);
    handleClose();
  };

  return (
    <>
      <Button
        color="inherit"
        size="small"
        startIcon={<BusinessOutlinedIcon fontSize="small" />}
        endIcon={<KeyboardArrowDownIcon fontSize="small" />}
        onClick={handleOpen}
        sx={{
          textTransform: 'none',
          fontWeight: 600,
          px: 1.5,
          bgcolor: 'action.hover',
          borderRadius: 1,
        }}
      >
        {activeCompanyObj.code}
      </Button>
      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleClose}>
        {AVAILABLE_COMPANIES.map((c) => (
          <MenuItem
            key={c.code}
            selected={c.code === currentCompany}
            onClick={() => handleSelect(c.code)}
          >
            <ListItemText
              primary={<Typography variant="body2" sx={{ fontWeight: 600 }}>{c.code}</Typography>}
              secondary={<Typography variant="caption" color="text.secondary">{c.name}</Typography>}
            />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
};
