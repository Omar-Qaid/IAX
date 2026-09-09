import React, { useMemo, useState } from 'react';
import { Link, Typography } from '@mui/material';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { CustomerQuickCreate } from '../components/CustomerQuickCreate';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { customerQuickCreateApi, type CustomerRecord } from '../api/customerQuickCreateApi';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useNotifications } from '@shared/hooks/useNotifications';

export function CustomerListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { notifyError, notifySuccess } = useNotifications();
  const customerQuery = useQuery({
    queryKey: ['accounts-receivable', 'customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
  });
  const customers = customerQuery.data ?? [];
  const [quickCreateOpen, setQuickCreateOpen] = useState(false);
  const columns = useMemo<ColumnDef<CustomerRecord>[]>(() => [
    { field: 'accountNumber', headerName: 'fields.account', width: 150, pinned: 'left', renderCell: ({ row }) => <Link component="button" underline="none" sx={{ fontSize: '0.75rem', color: 'primary.main' }}>{row.accountNumber}</Link> },
    { field: 'name', headerName: 'fields.customerName', width: 205 },
    { field: 'nameAr', headerName: 'fields.arabicName', width: 250, align: 'right', headerAlign: 'right' },
    { field: 'invoiceAccount', headerName: 'fields.invoiceAccount', width: 150, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'customerGroupId', headerName: 'fields.customerGroup', width: 150 },
    { field: 'currencyCode', headerName: 'fields.currency', width: 95 },
    { field: 'phone', headerName: 'fields.phone', width: 165 },
    { field: 'extension', headerName: 'fields.extension', width: 105, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'salesTaxGroup', headerName: 'fields.salesTaxGroup', width: 140, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'termsOfPayment', headerName: 'fields.termsOfPayment', width: 145, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'email', headerName: 'fields.email', width: 230 },
    { field: 'status', headerName: 'common.status', width: 110, renderCell: ({ row }) => <StatusBadge status={row.status} /> },
  ], []);

  const commandIds = ['customer', 'sell', 'invoice', 'collect', 'service', 'market', 'commerce', 'general', 'creditManagement', 'options'] as const;
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
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      onEdit: (customer) => navigate(ROUTE_PATHS.ACCOUNTS_RECEIVABLE.customer(customer.id)),
      onNew: () => setQuickCreateOpen(true),
      onDelete: async (selectedCustomers) => {
        try {
          await Promise.all(selectedCustomers.map((customer) => customerQuickCreateApi.remove(customer)));
          await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customers'] });
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    commands: commandIds.map((id) => ({ id, label: t(`customerCommands.${id}`) })),
    utilities: {
      personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 1,
    },
    advancedFilter: {
      title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: t('filters.customerAccount'), operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'), resetLabel: t('actions.reset'),
      fields: [
        { field: 'accountNumber', label: t('filters.customerAccount') },
        { field: 'name', label: t('fields.customerName') },
        { field: 'nameAr', label: t('fields.arabicName') },
        { field: 'customerGroupId', label: t('fields.customerGroup') },
        { field: 'currencyCode', label: t('fields.currency') },
        { field: 'phone', label: t('fields.phone') },
      ],
      getValue: (customer) => customer.accountNumber,
      matches: (customer, value) => customer.accountNumber.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: (customer) => [
        { id: 'primaryAddress', label: t('relatedInformation.primaryAddress'), defaultExpanded: true, content: <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary', whiteSpace: 'pre-line' }}>{customer ? t('relatedInformation.noAddressFor', { customer: customer.name }) : t('relatedInformation.selectCustomer')}</Typography> },
        { id: 'recentActivity', label: t('relatedInformation.recentActivity') },
        { id: 'relationships', label: t('relatedInformation.relationships') },
        { id: 'statistics', label: t('relatedInformation.statistics') },
        { id: 'creditStatistics', label: t('relatedInformation.creditStatistics') },
        { id: 'contacts', label: t('relatedInformation.contacts') },
        { id: 'recurringInvoice', label: t('relatedInformation.recurringInvoice') },
        { id: 'classificationBalances', label: t('relatedInformation.classificationBalances') },
        { id: 'insuranceGuarantees', label: t('relatedInformation.insuranceGuarantees') },
      ],
    },
  };

  return <SimpleListPage
    variant="enterprise"
    title={t('pages.customers.title')}
    enterpriseConfig={config}
    dataSource={{ type: 'controlled', rows: customers }}
    columns={columns}
    loading={customerQuery.isLoading}
    error={customerQuery.error instanceof Error ? customerQuery.error.message : null}
    onRetry={() => customerQuery.refetch()}
    dataGridProps={{ storageKey: 'accounts-receivable.customers.reference-view' }}
    dialogs={<CustomerQuickCreate open={quickCreateOpen} nextAccount="—" onClose={() => setQuickCreateOpen(false)} onSave={async () => { await queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'customers'] }); setQuickCreateOpen(false); }} />}
  />;
}
