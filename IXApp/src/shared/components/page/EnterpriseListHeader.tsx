import React from 'react';
import { Box, ButtonBase, Typography } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';

export interface EnterpriseListHeaderProps {
  contextLabel: string;
  viewLabel: string;
  onViewClick?: () => void;
}

export const EnterpriseListHeader: React.FC<EnterpriseListHeaderProps> = ({ contextLabel, viewLabel, onViewClick }) => (
  <Box sx={{ px: { xs: 1, sm: 2.5 }, pt: 0.25, pb: 1.75, fontFamily: '"Segoe UI", Arial, sans-serif' }}>
    <Typography sx={{ color: '#1b1b1b', fontFamily: 'inherit', fontSize: 16, lineHeight: 1.5 }}>
      {contextLabel}
    </Typography>
    <ButtonBase
      onClick={onViewClick}
      disableRipple={!onViewClick}
      sx={{ alignItems: 'center', borderRadius: 0.5, cursor: onViewClick ? 'pointer' : 'default' }}
    >
      <Typography component="h1" sx={{ fontFamily: 'inherit', fontSize: { xs: 24, sm: 29 }, color: '#1b1b1b', fontWeight: 600, lineHeight: 1.3 }}>
        {viewLabel}
      </Typography>
      <KeyboardArrowDownIcon sx={{ ml: 0.25, fontSize: 20, color: '#1b1b1b' }} />
    </ButtonBase>
  </Box>
);
