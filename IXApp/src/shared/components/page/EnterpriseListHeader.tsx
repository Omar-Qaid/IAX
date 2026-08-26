import React from 'react';
import { Box, Typography } from '@mui/material';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

export interface EnterpriseListHeaderProps {
  contextLabel: string;
  viewLabel: string;
  onViewClick?: () => void;
}

export const EnterpriseListHeader: React.FC<EnterpriseListHeaderProps> = ({ contextLabel }) => (
  <Box sx={{ px: { xs: 1, sm: 2.5 }, pt: 0.25, pb: 2.625, fontFamily: APP_FONT_FAMILY }}>
    <Typography component="h1" sx={{ color: '#1b1b1b', fontFamily: 'inherit', fontSize: { xs: 24, sm: 29 }, fontWeight: 600, lineHeight: 1.3 }}>
      {contextLabel}
    </Typography>
  </Box>
);
