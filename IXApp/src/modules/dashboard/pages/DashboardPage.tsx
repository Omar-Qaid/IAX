import React from 'react';
import { Grid } from '@mui/material';
import { WorkspacePage } from '@patterns/workspace/WorkspacePage';
import { WorkspaceSection } from '@patterns/workspace/WorkspaceSection';
import { WorkspaceTile } from '@patterns/workspace/WorkspaceTile';
import { MOCK_CUSTOMERS } from '@mocks/data/customers';
import { MOCK_SALES_ORDERS } from '@mocks/data/salesOrders';
import { MOCK_CURRENCIES } from '@mocks/data/currencies';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function DashboardPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const openOrders = MOCK_SALES_ORDERS.filter((order) => order.status === 'open').length;
  const orderValue = MOCK_SALES_ORDERS.reduce((total, order) => total + order.orderTotal, 0);

  return (
    <WorkspacePage title={t('pages.dashboard.title')} subtitle={t('pages.dashboard.subtitle')}>
      <WorkspaceSection title={t('pages.dashboard.overview')} subtitle={t('pages.dashboard.overviewHelp')}>
        <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
          <WorkspaceTile title={t('pages.dashboard.customers')} value={MOCK_CUSTOMERS.length} subtitle={t('pages.dashboard.customerAccounts')} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
          <WorkspaceTile title={t('pages.dashboard.openOrders')} value={openOrders} subtitle={t('pages.dashboard.totalOrders', { count: MOCK_SALES_ORDERS.length })} color="info" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, lg: 4 }}>
          <WorkspaceTile title={t('pages.dashboard.orderValue')} value={orderValue.toLocaleString(currentLanguage.code === 'ar' ? 'ar-SA' : 'en-US', { style: 'currency', currency: 'USD' })} subtitle={t('pages.dashboard.configuredCurrencies', { count: MOCK_CURRENCIES.length })} color="success" />
        </Grid>
      </WorkspaceSection>
    </WorkspacePage>
  );
}
