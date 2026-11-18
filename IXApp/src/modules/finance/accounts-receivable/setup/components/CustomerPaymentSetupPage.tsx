import React from 'react';
import { useMediaQuery, useTheme } from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import {
  customerPaymentSetupApi,
  type CustomerPaymentSetupKind,
  type CustomerPaymentSetupRecord,
} from '../api/customerPaymentSetupApi';
/** Existing payment setup forms: persist basic fields and preserve all other returned DTO fields. */
export function CustomerPaymentSetupPage({
  kind,
}: {
  kind: CustomerPaymentSetupKind;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const compact = useMediaQuery(useTheme().breakpoints.down('md'));
  const company = useCompanyStore((state) => state.currentCompany);
  const client = useQueryClient();
  const term = kind === 'terms';
  const api = React.useMemo(() => customerPaymentSetupApi(kind), [kind]);
  const prefix = term ? 'paymentTerms' : 'paymentMethods';
  const codeLabel = t(prefix + (term ? '.fields.term' : '.fields.method'));
  const descriptionLabel = t(prefix + '.fields.description');
  const fresh = (): CustomerPaymentSetupRecord => ({
    id: 'new-' + crypto.randomUUID(),
    recId: 0,
    code: '',
    description: '',
    days: 0,
    months: 0,
    source: {},
  });
  const invalidate = async () => {
    await Promise.all([
      client.invalidateQueries({
        queryKey: ['accounts-receivable', 'customer-quick-create', 'lookups'],
      }),
      client.invalidateQueries({
        queryKey: ['accounts-receivable', 'customer-groups', 'field-lookups'],
      }),
    ]);
  };
  const config: EnterpriseListDetailsConfig<CustomerPaymentSetupRecord> = {
    recordTableName: term ? 'PaymTerm' : 'CustPaymModeTable',
    getAuditRecordId: (record) => record.recId,
    dataSource: {
      type: 'remote',
      key: 'customer-payment-setup:' + kind + ':' + company,
      load: api.list,
      create: async (record) => {
        const result = await api.create(record);
        await invalidate();
        return result;
      },
      update: async (record) => {
        const result = await api.update(record);
        await invalidate();
        return result;
      },
      delete: async (record) => {
        await api.delete(record);
        await invalidate();
      },
    },
    createRecord: fresh,
    getPrimaryText: (record) => record.code,
    getSecondaryText: (record) => record.description,
    getValues: (record) => ({ days: record.days, months: record.months }),
    setValues: (record, values) => ({
      ...record,
      days: Number(values.days),
      months: Number(values.months),
    }),
    headerFields: [
      {
        id: 'code',
        label: codeLabel,
        getValue: (record) => record.code,
        setValue: (record, value) => ({ ...record, code: String(value) }),
      },
      {
        id: 'description',
        label: descriptionLabel,
        getValue: (record) => record.description,
        setValue: (record, value) => ({ ...record, description: String(value) }),
      },
    ],
    sections: [
      {
        id: 'general',
        title: t(term ? 'paymentTerms.sections.setup' : 'paymentMethods.sections.general'),
        groups: [
          {
            id: 'period',
            fields: [
              ...(term
                ? [
                    {
                      name: 'months',
                      label: t('paymentTerms.fields.months'),
                      type: 'number' as const,
                    },
                  ]
                : []),
              {
                name: 'days',
                label: t(term ? 'paymentTerms.fields.days' : 'paymentMethods.fields.gracePeriod'),
                type: 'number',
              },
            ],
          },
        ],
      },
    ],
    matchesSearch: (record, query) =>
      (record.code + ' ' + record.description)
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    validate: (record) => ({
      ...(!record.code.trim() ? { code: t('validation.required', { field: codeLabel }) } : {}),
      ...(!record.description.trim()
        ? { description: t('validation.required', { field: descriptionLabel }) }
        : {}),
      ...(!Number.isInteger(record.days)
        ? { days: t('validation.invalidNumber', 'Enter a whole number.') }
        : {}),
      ...(term && !Number.isInteger(record.months)
        ? { months: t('validation.invalidNumber', 'Enter a whole number.') }
        : {}),
    }),
    permissions: term
      ? {
          view: PERMISSIONS.PAYMENT_TERM_VIEW,
          create: PERMISSIONS.PAYMENT_TERM_CREATE,
          edit: PERMISSIONS.PAYMENT_TERM_EDIT,
          delete: PERMISSIONS.PAYMENT_TERM_DELETE,
        }
      : {
          view: PERMISSIONS.PAYMENT_METHOD_VIEW,
          create: PERMISSIONS.PAYMENT_METHOD_CREATE,
          edit: PERMISSIONS.PAYMENT_METHOD_EDIT,
          delete: PERMISSIONS.PAYMENT_METHOD_DELETE,
        },
    presentation: { mode: 'list', listInitiallyVisible: !compact },
  };
  return (
    <ListDetailsPage
      key={kind + ':' + company}
      variant="enterprise"
      title={t(term ? 'pages.paymentTerms.title' : 'pages.paymentMethods.title')}
      config={config}
    />
  );
}
