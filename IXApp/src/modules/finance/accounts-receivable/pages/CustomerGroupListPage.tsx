import React, { useMemo, useState } from 'react';
import { Link } from '@mui/material';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { MOCK_CUSTOMER_GROUPS, type CustomerGroup } from '@mocks/data/customerGroups';

export function CustomerGroupListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [customerGroups, setCustomerGroups] = useState<CustomerGroup[]>(MOCK_CUSTOMER_GROUPS);
  const columns = useMemo<ColumnDef<CustomerGroup>[]>(() => [
    { field: 'groupId', headerName: 'fields.customerGroup', width: 145, pinned: 'left', editable: true, renderCell: ({ row }) => <Link component="button" underline="none" sx={{ color: 'primary.main', fontSize: '0.75rem' }}>{row.groupId}</Link> },
    { field: 'name', headerName: 'fields.description', width: 205, editable: true },
    { field: 'paymentTerms', headerName: 'fields.termsOfPayment', width: 205, editable: true },
    { field: 'invoiceDueInterval', headerName: 'fields.timeBetweenInvoiceDue', width: 210, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'defaultTaxGroup', headerName: 'fields.defaultTaxGroup', width: 150, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'pricesIncludeTax', headerName: 'fields.pricesIncludeTax', width: 130, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'defaultWriteOffReason', headerName: 'fields.defaultWriteOffReason', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'accountingCurrencyExchange', headerName: 'fields.accountingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
    { field: 'reportingCurrencyExchange', headerName: 'fields.reportingCurrencyExchange', width: 190, sortable: false, filterable: false, valueGetter: () => '—' },
  ], []);

  const config: EnterpriseListConfig<CustomerGroup> = {
    contextLabel: t('pages.customerGroups.title'),
    viewLabel: t('pages.customerGroups.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'groupId', label: t('fields.groupId') },
      { field: 'name', label: t('fields.name') },
      { field: 'description', label: t('fields.description') },
      { field: 'paymentTerms', label: t('fields.paymentTerms') },
    ],
    locale: currentLanguage.code,
    crud: { editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete') },
    commands: ['setup', 'forecast', 'productFilters'].map((id) => ({ id, label: t(`customerGroupCommands.${id}`) })),
    utilities: {
      personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0,
    },
    advancedFilter: {
      title: t('filters.title'),
      addLabel: t('actions.add'),
      fieldLabel: t('fields.customerGroup'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (group) => group.groupId,
      matches: (group, value) => group.groupId.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return <SimpleListPage
    variant="enterprise"
    title={t('pages.customerGroups.title')}
    enterpriseConfig={config}
    dataSource={{ type: 'controlled', rows: customerGroups }}
    columns={columns}
    dataGridProps={{
      storageKey: 'accounts-receivable.customer-groups.reference-view',
      hideSidebar: false,
      masterForm: true,
      onNewRow: () => ({ id: `cg-${Date.now()}`, groupId: '', name: '', description: '', defaultCurrency: 'USD', paymentTerms: '', active: true }),
      onRowSave: (values, isNew) => {
        const saved = values as CustomerGroup;
        setCustomerGroups((current) => isNew
          ? [...current, saved]
          : current.map((group) => group.id === saved.id ? { ...group, ...saved } : group));
      },
    }}
  />;
}
