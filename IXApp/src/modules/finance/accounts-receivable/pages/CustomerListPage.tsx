import { localizedName } from '@shared/utilities/localizedName';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { accountContacts } from '@modules/finance/contacts/contactRoutes';
import React, { useEffect, useMemo, useState } from 'react';
import { Typography } from '@mui/material';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { CustomerQuickCreate } from '../components/CustomerQuickCreate';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { customerQuickCreateApi, type CustomerRecord } from '../api/customerQuickCreateApi';
import { useNavigate } from 'react-router-dom';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';
import { PERMISSIONS } from '@core/permissions/permissions';
import { PARTY_BALANCE_ROUTE_PATHS } from '@modules/finance/party-balances/routes/partyBalanceRoutePaths';
import { useNotifications } from '@shared/hooks/useNotifications';
import { FOUNDATION_RECORD_PREVIEWS } from '../../foundation/previews/foundationRecordPreviews';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS } from '../previews/accountsReceivableRecordPreviews';

export function CustomerListPage(): React.ReactElement {
  const company = useCompanyStore(state => state.currentCompany);
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { notifyError, notifySuccess } = useNotifications();
  const customerQuery = useQuery({
    queryKey: ['accounts-receivable', 'customers', company],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
  });
  const customers = customerQuery.data ?? [];
  const [quickCreateOpen, setQuickCreateOpen] = useState(false);
  const [customerToOpen, setCustomerToOpen] = useState<string>();
  useEffect(() => {
    if (!customerToOpen || quickCreateOpen) return;
    setCustomerToOpen(undefined);
    navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(customerToOpen));
  }, [customerToOpen, navigate, quickCreateOpen]);
  const columns = useMemo<ColumnDef<CustomerRecord>[]>(
    () => [
      {
        field: 'accountNumber',
        headerName: 'fields.account',
        width: 150,
        pinned: 'left',
        getDetailHref: (row) => ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(row.id),
      },
      { field: 'name', valueGetter: ({ row }) => localizedName({ name: row.name, nameAlias: row.nameAr }, isRtl), headerName: 'fields.customerName', width: 205 },
      {
        field: 'nameAr',
        headerName: 'fields.arabicName',
        width: 250,
        align: 'right',
        headerAlign: 'right',
      },
      { field: 'invoiceAccount', headerName: 'fields.invoiceAccount', width: 150, preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER } },
      { field: 'customerGroupId', headerName: 'fields.customerGroup', width: 150, preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER_GROUP } },
      { field: 'currencyCode', headerName: 'fields.currency', width: 95, preview: { entity: FOUNDATION_RECORD_PREVIEWS.CURRENCY } },
      { field: 'phone', headerName: 'fields.phone', width: 165 },
      { field: 'taxGroupId', headerName: 'fields.salesTaxGroup', width: 140, preview: { entity: FOUNDATION_RECORD_PREVIEWS.SALES_TAX_GROUP } },
      { field: 'paymTermId', headerName: 'fields.termsOfPayment', width: 145, preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.PAYMENT_TERMS } },
      {
        field: 'status',
        headerName: 'common.status',
        width: 110,
        renderCell: ({ row }) => <StatusBadge status={row.status} />,
      },
    ],
    [isRtl]
  );

  const config: EnterpriseListConfig<CustomerRecord> = {
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'CustTable',
    getAuditRecordId: (customer) => customer.recId,
    contextLabel: t('pages.customers.allCustomers'),
    viewLabel: t('pages.customers.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchByLabel: t('fields.searchBy'),
    searchMode: 'field',
    showFilterOnLoad: true,
    searchFields: [
      { field: 'phone', label: t('fields.phone') },
      { field: 'accountNumber', label: t('fields.account') },
      { field: 'name', label: t('fields.customerName') },
      { field: 'nameAr', label: t('fields.arabicName') },
    ],
    defaultSearchField: 'phone',
    locale: currentLanguage.code,
    crud: {
      editPermission: PERMISSIONS.CUSTOMER_UPDATE, newPermission: PERMISSIONS.CUSTOMER_CREATE, deletePermission: PERMISSIONS.CUSTOMER_DELETE,
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      onEdit: (customer) => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(customer.id)),
      onNew: () => setQuickCreateOpen(true),
      onDelete: async (selectedCustomers) => {
        try {
          await Promise.all(
            selectedCustomers.map((customer) => customerQuickCreateApi.remove(customer))
          );
          await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customers'] });
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    getCommands: customer => [
      {id: 'customer', label: t('customerCommands.customer'), disabled: !customer, onClick: () => customer && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(customer.id))},
      {id: 'view-contacts', label: t('contactPerson.view', 'View contacts'), permission: PERMISSIONS.CONTACT_VIEW, disabled: !customer, onClick: () => customer && navigate(accountContacts('customer', customer.accountNumber))},
      {id: 'transactions', label: t('customerRibbon.groups.transactions', 'Transactions'), permission: PERMISSIONS.CUSTOMER_TRANSACTION_VIEW, disabled: !customer, onClick: () => customer && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.custTrans(customer.accountNumber))},
      {id: 'open-transactions', label: t('partyBalance.openTransactions'), permission: PERMISSIONS.CUSTOMER_TRANSACTION_VIEW, disabled: !customer, onClick: () => customer && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerOpenTransactions(customer.accountNumber))},
      {id: 'balance', label: t('partyBalance.balance'), permission: PERMISSIONS.CUSTOMER_TRANSACTION_VIEW, disabled: !customer, onClick: () => customer && navigate(PARTY_BALANCE_ROUTE_PATHS.customerBalance(customer.accountNumber))},
      {id: 'sales-orders', label: t('pages.salesOrders.title'), permission: PERMISSIONS.SALES_ORDER_VIEW, disabled: !customer, onClick: () => customer && navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customerSalesOrders(customer.accountNumber))},
    ],
    utilities: {
      personalizeLabel: t('utilities.personalize'),
      guideLabel: t('utilities.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('utilities.openWindow'),
      notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'),
      addLabel: t('actions.add'),
      fieldLabel: t('filters.customerAccount'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      fields: [
        { field: 'accountNumber', label: t('filters.customerAccount') },
        { field: 'name', label: t('fields.customerName') },
        { field: 'nameAr', label: t('fields.arabicName') },
        { field: 'customerGroupId', label: t('fields.customerGroup') },
        { field: 'currencyCode', label: t('fields.currency') },
        { field: 'phone', label: t('fields.phone') },
      ],
      getValue: (customer) => customer.accountNumber,
      matches: (customer, value) =>
        customer.accountNumber
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: customer => [{id: 'statistics', label: t('relatedInformation.statistics'), content: <Typography sx={{fontSize: '0.75rem'}}>{customer ? [customer.accountNumber, customer.customerGroupId, customer.currencyCode].filter(Boolean).join(' · ') : t('relatedInformation.selectCustomer')}</Typography>}],
    },
  };

  return (
    <SimpleListPage
      title={t('pages.customers.title')}
      enterpriseConfig={config}
      dataSource={{ type: 'controlled', rows: customers, refresh: () => void customerQuery.refetch() }}
      columns={columns}
      loading={customerQuery.isLoading}
      error={customerQuery.error instanceof Error ? customerQuery.error.message : null}
      onRetry={() => customerQuery.refetch()}
      dataGridProps={{ storageKey: 'accounts-receivable.customers.reference-view' }}
      dialogs={quickCreateOpen && (
        <CustomerQuickCreate
          open={quickCreateOpen}
          nextAccount="—"
          onClose={() => setQuickCreateOpen(false)}
          onSave={async (customer, openAfterSave) => {
            await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customers'] });
            setQuickCreateOpen(false);
            if (openAfterSave) setCustomerToOpen(customer.id);
          }}
        />
      )}
    />
  );
}
