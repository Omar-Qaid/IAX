import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert } from '@mui/material';
import { LookupField } from '@shared/components/lookups/LookupField';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig, DetailFieldConfig } from '@patterns/list-details/types';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { taxAccountGroupApi as api, type TaxAccountGroupRecord } from '../api/taxAccountGroupApi';

const fields = [
  ['taxOutgoingLedgerDimension', 'Sales tax payable'],
  ['taxIncomingLedgerDimension', 'Sales tax receivable'],
  ['taxOffsetUseTaxLedgerDimension', 'Use tax expense'],
  ['taxUseTaxLedgerDimension', 'Use tax payable'],
  ['taxReportLedgerDimension', 'Settlement account'],
  ['cashDiscountIncomingLedgerDimension', 'Vendor cash discount'],
  ['cashDiscountOutgoingLedgerDimension', 'Customer cash discount'],
] as const;
const permission = (action: string) => `Tax.LedgerAccountGroups.${action}`;

export function TaxAccountGroupPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const accounts = useQuery({
    queryKey: ['tax-posting-ledger-accounts', company],
    queryFn: ({ signal }) => api.accounts(signal),
  });
  const config = useMemo<EnterpriseListDetailsConfig<TaxAccountGroupRecord>>(() => {
    const field = ([name, label]: (typeof fields)[number]): DetailFieldConfig => ({
      name,
      label: t(`taxAccountGroup.fields.${name}`, label),
      type: 'text',
      width: 220,
      options: [
        { value: '', label: '' },
        ...(accounts.data ?? []).map((account) => ({
          value: String(account.id),
          label: account.name || account.code,
        })),
      ],
      formatValue: (value) =>
        accounts.data?.find((account) => String(account.id) === String(value))?.code ??
        (value ? String(value) : ''),
      disabled: accounts.isError || accounts.isPending,
      renderOwnLabel: true,
      render: ({ value, disabled, onChange }) => (
        <LookupField
          name={name}
          label={t(`taxAccountGroup.fields.${name}`, label)}
          value={value ? Number(value) : null}
          options={accounts.data ?? []}
          previewEntity="taxLedgerDimension"
          displayMode="select"
          searchable
          sideMode="client"
          lazyLoading={false}
          variant="standard"
          fullWidth
          loading={accounts.isFetching}
          disabled={disabled || accounts.isError}
          error={accounts.isError}
          helperText={accounts.error?.message}
          onChange={(next) => onChange(next == null ? '' : String(next))}
        />
      ),
    });
    return {
      recordTableName: 'TaxLedgerAccountGroup',
      dataSource: {
        type: 'remote',
        key: `tax-account-groups:${company}`,
        load: api.list,
        create: api.create,
        update: api.update,
        delete: api.delete,
      },
      createRecord: () => ({
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        taxAccountGroup: '',
        name: '',
        ...Object.fromEntries(fields.map(([key]) => [key, null])),
      }),
      getPrimaryText: (value) => value.taxAccountGroup,
      getSecondaryText: (value) => value.name,
      matchesSearch: (value, query) =>
        `${value.taxAccountGroup} ${value.name}`.toLowerCase().includes(query.toLowerCase()),
      getValues: (value) =>
        Object.fromEntries(
          fields.map(([key]) => [key, value[key] == null ? '' : String(value[key])])
        ),
      setValues: (value, values) => ({
        ...value,
        ...Object.fromEntries(
          fields.map(([key]) => [key, values[key] === '' ? null : Number(values[key])])
        ),
      }),
      headerFields: [
        {
          id: 'taxAccountGroup',
          label: t('taxAccountGroup.fields.code', 'Ledger posting group'),
          width: 118,
          getValue: (x) => x.taxAccountGroup,
          setValue: (x, value) => ({ ...x, taxAccountGroup: String(value) }),
        },
        {
          id: 'name',
          label: t('taxAccountGroup.fields.name', 'Description'),
          width: 220,
          getValue: (x) => x.name,
          setValue: (x, value) => ({ ...x, name: String(value) }),
        },
      ],
      sections: [
        {
          id: 'general',
          title: t('common.general', 'General'),
          defaultExpanded: true,
          gridTemplateColumns: 'repeat(5, minmax(0, 220px))',
          columnGap: '50px',
          detailsPadding: '10px',
          groups: [
            ...fields.slice(0, 4).map((item) => ({ id: item[0], fields: [field(item)] })),
            { id: 'settlement', fields: fields.slice(4).map(field) },
          ],
        },
      ],
      permissions: {
        view: permission('View'),
        create: permission('Create'),
        edit: permission('Edit'),
        delete: permission('Delete'),
      },
      validate: (value) => ({
        ...(!value.taxAccountGroup.trim()
          ? { taxAccountGroup: t('taxAccountGroup.required', 'Enter a ledger posting group.') }
          : {}),
        ...(value.taxAccountGroup.length > 10
          ? { taxAccountGroup: t('taxAccountGroup.codeLength', 'Use at most 10 characters.') }
          : {}),
        ...(value.name.length > 60
          ? { name: t('taxAccountGroup.nameLength', 'Use at most 60 characters.') }
          : {}),
      }),
      viewLabel: t('common.standardView', 'Standard view'),
      presentation: {
        mode: 'list',
        listInitiallyVisible: true,
        listWidth: 280,
        listResizable: true,
        headerMaxWidth: 350,
        recordHeaderMinHeight: 124,
      },
    };
  }, [
    accounts.data,
    accounts.error?.message,
    accounts.isError,
    accounts.isFetching,
    accounts.isPending,
    company,
    t,
  ]);
  return (
    <>
      {accounts.isError && (
        <Alert severity="error">
          {t(
            'taxAccountGroup.lookupError',
            'Ledger accounts could not be loaded. Refresh the page to retry.'
          )}
        </Alert>
      )}
      <ListDetailsPage
        variant="enterprise"
        title={t('taxAccountGroup.title', 'Ledger posting groups')}
        config={config}
      />
    </>
  );
}
