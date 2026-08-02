import React from 'react';
import { Box, ButtonBase, Typography } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';

export interface EnterpriseListHeaderProps {
  contextLabel: string;
  viewLabel: string;
  onViewClick?: () => void;
}

export const EnterpriseListHeader: React.FC<EnterpriseListHeaderProps> = ({ contextLabel, viewLabel, onViewClick }) => (
  <Box sx={{ px: { xs: 0.5, sm: 1 }, pt: 0.25, pb: 1 }}>
    <Typography sx={{ color: 'text.primary', fontSize: '0.75rem', lineHeight: 1.5 }}>
      {contextLabel}
    </Typography>
    <ButtonBase
      onClick={onViewClick}
      disableRipple={!onViewClick}
      sx={{ alignItems: 'center', borderRadius: 0.5, cursor: onViewClick ? 'pointer' : 'default' }}
    >
      <Typography component="h1" sx={{ fontSize: { xs: '1.125rem', sm: '1.375rem' }, fontWeight: 600, lineHeight: 1.35 }}>
        {viewLabel}
      </Typography>
      <KeyboardArrowDownIcon sx={{ ml: 0.25, fontSize: 17, color: 'text.secondary' }} />
    </ButtonBase>
  </Box>
);
