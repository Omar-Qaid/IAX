import React from 'react';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { Box, useTheme, useMediaQuery } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import {
  TabularDetailPanel,
  type TabularDetailPanelProps,
} from '@patterns/list-details/TabularDetailPanel';
import type { EnterpriseListDetailsConfig, ListDetailRecord } from '@patterns/list-details/types';

export function ListDetailsListGridPage<T extends ListDetailRecord>({
  title,
  config,
}: {
  title: string;
  config: EnterpriseListDetailsConfig<T>;
}): React.ReactElement {
  const mobile = useMediaQuery(useTheme().breakpoints.down('md'));
  const { t } = useAppTranslation();
  return (
    <ListDetailsPage
      variant="enterprise"
      title={title}
      config={{
        ...config,
        viewLabel: config.viewLabel ?? t('common.standardView'),
        presentation: {
          listWidth: 264,
          headerMaxWidth: 314,
          recordHeaderMinHeight: 124,
          listInitiallyVisible: !mobile,
          ...config.presentation,
          mode: 'list',
        },
      }}
    />
  );
}

export function ListGridDetailsPanel<T extends ListDetailRecord>({
  details,
  gridWidth = 334,
  ...grid
}: TabularDetailPanelProps<T> & {
  details: React.ReactNode;
  gridWidth?: number;
}): React.ReactElement {
  return (
    <Box
      data-testid="list-grid-details"
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: 'minmax(0, 1fr)', lg: `${gridWidth}px minmax(0, 1fr)` },
        gap: '17px',
        minWidth: 0,
        alignItems: 'start',
        '& [role="gridcell"]': { borderBottom: 0, borderInlineEnd: 0, px: '7px' },
        '& [role="row"][aria-selected="true"] > [role="gridcell"]:first-of-type': {
          borderInlineStart: '3px solid #315efb',
        },
      }}
    >
      <TabularDetailPanel {...grid} height={218} rowHeight={31} />
      <Box sx={{ minWidth: 0, pt: { xs: 0, lg: '30px' } }}>{details}</Box>
    </Box>
  );
}
