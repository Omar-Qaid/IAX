import React, { useCallback, useMemo } from 'react';
import { Box, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { FastTabsDrawer } from '@patterns/fast-tabs-drawer';
import type { FastTabSection, FastTabValue } from '@patterns/dialog-fast-tabs/FastTabsDialog';
import type { Customer } from '@mocks/data/customers';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';

interface CustomerQuickCreateProps { open: boolean; nextAccount: string; onClose: () => void; onSave: (customer: Customer, openAfterSave: boolean) => void | Promise<void> }
const initialValues = (accountNumber: string): Record<string, FastTabValue> => ({ accountNumber, type: 'organization', name: '', nameAr: '', customerGroupId: '', currencyCode: 'SAR', termsOfPayment: '', paymentMethod: '', deliveryTerms: '', deliveryMode: '', salesTaxGroup: '', customerCategory: '', mainCrNumber: '', branchCrNumber: '', vatNumber: '', notes: '', zatcaType: 'none', sourceCode: '', country: '', street: '' });

export function CustomerQuickCreate({ open, nextAccount, onClose, onSave }: CustomerQuickCreateProps): React.ReactElement {
  const { t } = useAppTranslation();
  const option = useCallback((value: string, key: string) => ({ value, label: t(key) }), [t]);
  const lookups = useQuery({
    queryKey: ['accounts-receivable', 'customer-quick-create', 'lookups'],
    queryFn: ({ signal }) => customerQuickCreateApi.lookups(signal),
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });
  const lookup = lookups.data;
  const sections = useMemo<FastTabSection[]>(() => [{ id: 'details', title: t('customerQuickCreate.sections.details'), summary: <FastTabSummary values={['--', 'SAR', '--', '--', '--', '--']} />, fields: [
    { name: 'accountNumber', label: t('fields.customerAccount'), disabled: true }, { name: 'salesTaxGroup', label: t('fields.salesTaxGroup'), type: 'select', options: [option('vat15', 'customerQuickCreate.options.vat15')] },
    { name: 'type', label: t('customerQuickCreate.fields.type'), type: 'select', options: [option('organization', 'customerQuickCreate.options.organization'), option('person', 'customerQuickCreate.options.person')] }, { name: 'customerCategory', label: t('customerQuickCreate.fields.category'), type: 'select', required: true, options: [option('retail', 'customerQuickCreate.options.retail'), option('wholesale', 'customerQuickCreate.options.wholesale')] },
    { name: 'name', label: t('fields.customerName'), required: true }, { name: 'mainCrNumber', label: t('customerQuickCreate.fields.mainCrNumber'), width: 100 },
    { name: 'nameAr', label: t('fields.arabicName') }, { name: 'branchCrNumber', label: t('customerQuickCreate.fields.branchCrNumber'), width: 100 },
    { name: 'customerGroupId', label: t('fields.customerGroup'), type: 'select', required: true, options: lookup?.customerGroups ?? [] }, { name: 'vatNumber', label: t('customerQuickCreate.fields.vatNumber') },
    { name: 'currencyCode', label: t('fields.currency'), type: 'select', width: 100, options: lookup?.currencies ?? [{ value: 'SAR', label: 'SAR' }] }, { name: 'notes', label: t('customerQuickCreate.fields.notes'), type: 'multiline', rows: 4 },
    { name: 'termsOfPayment', label: t('fields.termsOfPayment'), type: 'select', options: lookup?.paymentTerms ?? [] }, { name: 'zatcaType', label: t('customerQuickCreate.fields.zatcaType'), type: 'select', options: [option('none', 'common.none')] },
    { name: 'paymentMethod', label: t('customerQuickCreate.fields.paymentMethod'), type: 'select', options: lookup?.paymentMethods ?? [] }, { name: 'sourceCode', label: t('customerQuickCreate.fields.sourceCode') },
    { name: 'deliveryTerms', label: t('customerQuickCreate.fields.deliveryTerms'), type: 'select', options: lookup?.deliveryTerms ?? [] }, { name: 'deliveryMode', label: t('customerQuickCreate.fields.deliveryMode'), type: 'select', options: lookup?.deliveryModes ?? [] },
  ] }, { id: 'address', title: t('customerQuickCreate.sections.address'), fields: [{ name: 'country', label: t('customerQuickCreate.fields.country'), type: 'select', required: true, options: [option('SA', 'customerQuickCreate.options.saudiArabia')] }, { name: 'street', label: t('customerQuickCreate.fields.street') }] }], [lookup, option, t]);
  return <FastTabsDrawer
    open={open}
    resetKey={nextAccount}
    title={t('customerQuickCreate.title')}
    viewLabel={t('common.standardView')}
    sections={sections}
    loading={lookups.isLoading}
    loadError={lookups.error instanceof Error ? lookups.error.message : null}
    initialValues={() => initialValues(nextAccount)}
    validate={(values) => ['name', 'customerGroupId', 'customerCategory', 'country'].reduce<Record<string, string>>((result, name) => { if (!String(values[name] ?? '').trim()) result[name] = t('validation.required', { field: name }); return result; }, {})}
    onSubmit={async (values, mode) => {
      const customer = await customerQuickCreateApi.create({
        name: String(values.name), nameAlias: String(values.nameAr), custGroupId: String(values.customerGroupId),
        currencyCode: String(values.currencyCode), custCategory: String(values.customerCategory),
        paymTermId: String(values.termsOfPayment), paymModeId: String(values.paymentMethod),
        dlvModeId: String(values.deliveryMode), taxGroupId: String(values.salesTaxGroup),
        vatNum: String(values.vatNumber), countryRegionId: String(values.country), memo: String(values.notes),
      });
      await onSave(customer, mode === 'save-and-open');
    }}
    saveLabel={t('actions.save')}
    saveAndOpenLabel={t('customerQuickCreate.saveAndOpen')}
    cancelLabel={t('actions.cancel')}
    closeLabel={t('actions.close')}
    helpLabel={t('common.help')}
    onCancel={onClose}
  />;
}

function FastTabSummary({ values }: { values: string[] }): React.ReactElement {
  return <Box aria-hidden="true" sx={{ display: { xs: 'none', sm: 'flex' }, alignItems: 'center', height: 22 }}>
    {values.map((value, index) => <Typography key={`${value}-${index}`} component="span" sx={{ minWidth: 32, px: 0.75, borderInlineStart: 1, borderColor: 'divider', color: value === 'SAR' ? 'primary.main' : 'text.secondary', textAlign: 'center', fontSize: '0.6875rem', lineHeight: 1 }}>{value}</Typography>)}
  </Box>;
}
