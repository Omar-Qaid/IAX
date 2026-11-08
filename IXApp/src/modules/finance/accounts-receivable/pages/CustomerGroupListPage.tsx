import React, { useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import {
  customerGroupApi,
  newCustomerGroup,
  type CustomerGroupRecord,
} from '../api/customerGroupApi';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';

// Customer groups are company data: the request carries X-Company, so the cache has to be
// keyed by company too or switching company shows the previous one's rows.
const groupQueryKey = (company: string) =>
  ['accounts-receivable', 'customer-groups', company] as const;
const lookupsQueryKey = (company: string) =>
  ['accounts-receivable', 'customer-groups', 'field-lookups', company] as const;
// Deliberately not company-suffixed: CustomerQuickCreate and CustTablePage cache these
// lookups under this exact key, and invalidation matches by prefix, so the shorter key
// clears theirs as well as any company-scoped variant. (That those two are not themselves
// company-scoped is a separate defect, recorded rather than changed from here.)
const quickCreateLookupsKey = ['accounts-receivable', 'customer-quick-create', 'lookups'] as const;

export function CustomerGroupListPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const company = useCompanyStore((state) => state.currentCompany);
  const groupsQuery = useQuery({
    queryKey: groupQueryKey(company),
    queryFn: ({ signal }) => customerGroupApi.list(signal),
  });
  const lookupsQuery = useQuery({
    queryKey: lookupsQueryKey(company),
    queryFn: ({ signal }) => customerQuickCreateApi.groupFieldLookups(signal),
    staleTime: 5 * 60 * 1000,
  });
  const invalidateGroups = async (): Promise<void> => {
    await queryClient.invalidateQueries({ queryKey: groupQueryKey(company) });
    await queryClient.invalidateQueries({ queryKey: quickCreateLookupsKey });
  };
  // Read inside the memo: `?? []` builds a new array on every render, so hoisting these
  // out would change the dependencies each time and the memo would never hold.
  const lookups = lookupsQuery.data;
  const columns = useMemo<ColumnDef<CustomerGroupRecord>[]>(() => {
    const paymentOptions = lookups?.paymentTerms ?? [];
    const taxOptions = lookups?.salesTaxGroups ?? [];
    return [
      {
        field: 'custGroupId',
        headerName: 'fields.customerGroup',
        width: 145,
        pinned: 'left',
        editable: true,
      },
      { field: 'name', headerName: 'fields.description', width: 205, editable: true },
      {
        field: 'paymTermId',
        headerName: 'fields.termsOfPayment',
        width: 205,
        editable: true,
        type: 'singleSelect',
        valueOptions: paymentOptions,
      },
      {
        field: 'taxGroupId',
        headerName: 'fields.salesTaxGroup',
        width: 150,
        editable: true,
        type: 'singleSelect',
        valueOptions: taxOptions,
      },
      {
        field: 'priceIncludeSalesTax',
        headerName: 'fields.pricesIncludeTax',
        width: 130,
        type: 'boolean',
        valueGetter: ({ row }) => row.priceIncludeSalesTax === 1,
      },
      {
        field: 'accountingCurrencyExchangeRateType',
        headerName: 'fields.accountingCurrencyExchange',
        width: 190,
      },
      {
        field: 'reportingCurrencyExchangeRateType',
        headerName: 'fields.reportingCurrencyExchange',
        width: 190,
      },
    ];
  }, [lookups]);

  const config: EnterpriseListConfig<CustomerGroupRecord> = {
    contextLabel: t('pages.customerGroups.title'),
    viewLabel: t('pages.customerGroups.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'custGroupId', label: t('fields.customerGroup') },
      { field: 'name', label: t('fields.description') },
      { field: 'paymTermId', label: t('fields.paymentTerms') },
      { field: 'taxGroupId', label: t('fields.salesTaxGroup') },
    ],
    locale: currentLanguage.code,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => customerGroupApi.delete(row)));
        } finally {
          // Refetch even when one delete failed, so the grid stops showing rows that went.
          await invalidateGroups();
        }
      },
    },
    commands: ['setup', 'forecast', 'productFilters'].map((id) => ({
      id,
      label: t(`customerGroupCommands.${id}`),
      disabled: true,
    })),
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
      fieldLabel: t('fields.customerGroup'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (group) => group.custGroupId,
      matches: (group, value) =>
        group.custGroupId
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return (
    <SimpleListPage
      title={t('pages.customerGroups.title')}
      enterpriseConfig={config}
      dataSource={{
        type: 'controlled',
        rows: groupsQuery.data ?? [],
        loading: groupsQuery.isLoading || lookupsQuery.isLoading,
        error:
          groupsQuery.error instanceof Error
            ? groupsQuery.error.message
            : lookupsQuery.error instanceof Error
              ? lookupsQuery.error.message
              : null,
        refresh: () => {
          void groupsQuery.refetch();
          void lookupsQuery.refetch();
        },
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'accounts-receivable.customer-groups.reference-view',
        hideSidebar: false,
        masterForm: true,
        onNewRow: newCustomerGroup,
        onRowSave: async (values, isNew) => {
          const record = values as CustomerGroupRecord;
          if (isNew) await customerGroupApi.create(record);
          else await customerGroupApi.update(record);
          await invalidateGroups();
        },
      }}
    />
  );
}
