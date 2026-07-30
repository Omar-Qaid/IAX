import React from 'react';
import { Box } from '@mui/material';
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined';
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import AttachMoneyOutlinedIcon from '@mui/icons-material/AttachMoneyOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import { NavigationGroup } from './NavigationGroup';
import { NavigationItem } from './NavigationItem';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const ModuleNavigation: React.FC = () => {
  const { t } = useAppTranslation();

  return (
    <Box sx={{ px: 1, py: 1.5 }}>
      <NavigationItem
        label={t('nav.dashboard') || 'Dashboard'}
        path={ROUTE_PATHS.DASHBOARD}
        icon={<DashboardOutlinedIcon fontSize="small" />}
      />

      <NavigationGroup title={t('nav.accountsReceivable') || 'Accounts Receivable'}>
        <NavigationItem
          label={t('nav.allCustomers') || 'All Customers'}
          path={ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS}
          icon={<PeopleAltOutlinedIcon fontSize="small" />}
        />
        <NavigationItem
          label={t('nav.customerGroups') || 'Customer Groups'}
          path={ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS}
          icon={<GroupsOutlinedIcon fontSize="small" />}
        />
        <NavigationItem
          label={t('nav.allSalesOrders') || 'Sales Orders'}
          path={ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS}
          icon={<ShoppingCartOutlinedIcon fontSize="small" />}
        />
      </NavigationGroup>

      <NavigationGroup title={t('nav.foundation') || 'Foundation'}>
        <NavigationItem
          label={t('nav.currencies') || 'Currencies'}
          path={ROUTE_PATHS.FOUNDATION.CURRENCIES}
          icon={<AttachMoneyOutlinedIcon fontSize="small" />}
        />
      </NavigationGroup>

      <NavigationGroup title={t('nav.systemAdmin') || 'System Administration'}>
        <NavigationItem
          label={t('nav.settings') || 'Application Settings'}
          path={ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS}
          icon={<SettingsOutlinedIcon fontSize="small" />}
        />
      </NavigationGroup>
    </Box>
  );
};
