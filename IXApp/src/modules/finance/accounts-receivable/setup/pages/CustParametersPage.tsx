import React from 'react';
import { Alert, Box, Button, CircularProgress } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SetupPage } from '@patterns/setup/SetupPage';
import type { SetupSectionConfig } from '@patterns/setup/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  customerParametersApi,
  parameterTextFields,
  parameterBooleanFields,
  parameterValues,
  type CustomerParametersRecord,
} from '../api/customerParametersApi';

export function CustParametersPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <CompanyParameters key={company} company={company} />;
}

function CompanyParameters({ company }: { company: string }): React.ReactElement {
  const { t } = useAppTranslation();
  const queryKey = ['accounts-receivable', 'customer-parameters', company] as const;
  const parameters = useQuery({
    queryKey,
    queryFn: ({ signal }) => customerParametersApi.get(company, signal),
  });
  if (parameters.isLoading)
    return (
      <Box sx={{ p: 2 }}>
        <CircularProgress aria-label={t('common.loading', 'Loading')} />
      </Box>
    );
  if (parameters.error || !parameters.data)
    return (
      <Box sx={{ p: 2 }}>
        <Alert severity={parameters.error ? 'error' : 'info'}>
          {parameters.error instanceof Error
            ? parameters.error.message
            : t('customerParameterSettings.notConfigured')}
          <Button onClick={() => void parameters.refetch()}>
            {t('actions.refresh', 'Refresh')}
          </Button>
        </Alert>
      </Box>
    );
  return <ParametersForm key={parameters.data.recId} company={company} record={parameters.data} />;
}

function ParametersForm({
  company,
  record,
}: {
  company: string;
  record: CustomerParametersRecord;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const { hasPermission } = useAuth();
  const canEdit = hasPermission(PERMISSIONS.CUSTOMER_PARAMETERS_EDIT);
  const cache = useQueryClient();
  const latest = React.useRef(record);
  const sections = React.useMemo<SetupSectionConfig[]>(() => {
    const text = (names: readonly string[]) =>
      names.map((name) => ({
        name,
        label: t(`customerParameterSettings.fields.${name}`),
        type: 'text' as const,
      }));
    return [
      {
        id: 'defaults',
        title: t('customerParameterSettings.defaults'),
        fields: [
          ...text(parameterTextFields.slice(0, 7)),
          {
            name: 'creditLimit',
            label: t('customerParameterSettings.fields.creditLimit'),
            type: 'number',
          },
          {
            name: 'taxOnPrepayment',
            label: t('customerParameterSettings.fields.taxOnPrepayment'),
            type: 'boolean',
          },
        ],
      },
      {
        id: 'invoicing',
        title: t('customerParameterSettings.invoicing'),
        fields: text(parameterTextFields.slice(7, 11)),
      },
      {
        id: 'pricing',
        title: t('customerParameterSettings.pricing'),
        fields: [
          ...parameterBooleanFields
            .slice(1)
            .map((name) => ({
              name,
              label: t(`customerParameterSettings.fields.${name}`),
              type: 'boolean' as const,
            })),
          ...text(parameterTextFields.slice(11)),
        ],
      },
    ];
  }, [t]);
  return (
    <SetupPage
      title={t('pages.customerParameters.title')}
      viewLabel={t('pages.customerParameters.standardView')}
      optionsLabel={t('customerCommands.options')}
      navigationItems={sections.map((section) => ({ id: section.id, label: section.title }))}
      sections={sections}
      initialValues={parameterValues(record)}
      saveLabel={t('actions.save')}
      savedMessage={t('customerParameters.saved')}
      yesLabel={t('common.yes', 'Yes')}
      noLabel={t('common.no', 'No')}
      readOnly={!canEdit}
      headerContent={
        !canEdit && <Alert severity="info">{t('customerParameterSettings.readOnly')}</Alert>
      }
      onSave={async (values) => {
        if (!canEdit) throw new Error(t('customerParameterSettings.readOnly'));
        for (const field of parameterTextFields) {
          const max = field === 'currencyCode' ? 3 : 10;
          if (String(values[field] ?? '').length > max)
            throw new Error(
              t('customerParameterSettings.tooLong', {
                field: t(`customerParameterSettings.fields.${field}`),
                max,
              })
            );
        }
        if (!Number.isFinite(Number(values.creditLimit)))
          throw new Error(t('customerParameterSettings.invalidCreditLimit'));
        const saved = await customerParametersApi.save(latest.current, values);
        latest.current = saved;
        cache.setQueryData(['accounts-receivable', 'customer-parameters', company], saved);
      }}
    />
  );
}
