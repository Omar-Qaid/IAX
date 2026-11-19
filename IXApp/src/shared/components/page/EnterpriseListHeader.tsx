import React from 'react';
import { Box } from '@mui/material';
import { DocumentHeader } from './DocumentHeader';
import { documentTypographySx } from './documentLayoutStyles';

export interface EnterpriseListHeaderProps {
  contextLabel: string;
  pageTitle?: string;
  viewLabel: string;
  onViewClick?: () => void;
}

export const EnterpriseListHeader: React.FC<EnterpriseListHeaderProps> = ({
  contextLabel,
  pageTitle,
  viewLabel,
}) => (
  <Box
    sx={{
      px: { xs: 1, sm: 2.5 },
      pt: 0.25,
      color: 'text.primary',
      textAlign: 'start',
      ...documentTypographySx,
    }}
  >
    <DocumentHeader
      title={contextLabel}
      subtitle={pageTitle ?? contextLabel}
      viewLabel={viewLabel}
    />
  </Box>
);
