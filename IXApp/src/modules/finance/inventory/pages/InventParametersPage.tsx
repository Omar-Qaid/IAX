import React, { useMemo } from 'react';
import { Alert, Box, CircularProgress } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SetupPage } from '@patterns/setup/SetupPage';
import type { SetupNavigationItem, SetupSectionConfig, SetupValues } from '@patterns/setup/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { inventParametersApi, type InventParametersRecord } from '../api/inventParametersApi';

const journalTypes = { movement: 0, adjustment: 1, counting: 2, transfer: 3 } as const;

export function InventParametersPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const queryKey = ['inventory', 'parameters', company] as const;
  const parameters = useQuery({ queryKey, queryFn: ({ signal }) => inventParametersApi.get(signal) });
  const lookups = useQuery({ queryKey: ['inventory', 'parameters', 'lookups', company], queryFn: ({ signal }) => inventParametersApi.lookups(signal) });

  const navigationItems = useMemo<SetupNavigationItem[]>(() => [
    { id: 'general', label: t('inventParameters.navigation.general', 'General') },
    { id: 'journalNames', label: t('inventParameters.navigation.journalNames', 'Journal names') },
  ], [t]);

  const sections = useMemo<SetupSectionConfig[]>(() => {
    const option = (id: string, name: string, suffix?: string) => ({ value: id, label: name ? `${id} - ${name}${suffix ?? ''}` : `${id}${suffix ?? ''}` });
    const journalOptions = (type: number) => [{ value: '', label: '' }, ...(lookups.data?.journalNames.filter((x) => x.journalType === type).map((x) => option(x.id, x.name)) ?? [])];
    return [
      {
        id: 'general', title: t('inventParameters.sections.general', 'General'), fields: [
          { name: 'defaultUnitId', label: t('inventParameters.fields.defaultUnit', 'Default unit'), type: 'select', options: [{ value: '', label: '' }, ...(lookups.data?.units.map((x) => option(x.id, x.name)) ?? [])] },
          { name: 'multiSiteDefaultInventSiteId', label: t('inventParameters.fields.defaultSite', 'Default site'), type: 'select', options: [{ value: '', label: '' }, ...(lookups.data?.sites.map((x) => option(x.id, x.name)) ?? [])] },
          { name: 'multiSiteDefaultInventLocationId', label: t('inventParameters.fields.defaultWarehouse', 'Default warehouse'), type: 'select', options: [{ value: '', label: '' }, ...(lookups.data?.warehouses.map((x) => option(x.id, x.name, ` (${x.siteId})`)) ?? [])] },
          { name: 'reserveOnOrdered', label: t('inventParameters.fields.reserveOnOrdered', 'Reserve ordered items'), type: 'boolean' },
          { name: 'mandatoryItemTaxGroup', label: t('inventParameters.fields.mandatoryItemTaxGroup', 'Mandatory item sales tax group'), type: 'boolean' },
        ],
      },
      {
        id: 'journalNames', title: t('inventParameters.sections.journalNames', 'Journal names'), fields: [
          { name: 'movementJournalNameId', label: t('inventParameters.fields.movementJournal', 'Movement'), type: 'select', options: journalOptions(journalTypes.movement) },
          { name: 'lossProfitJournalNameId', label: t('inventParameters.fields.adjustmentJournal', 'Inventory adjustment'), type: 'select', options: journalOptions(journalTypes.adjustment) },
          { name: 'transferJournalNameId', label: t('inventParameters.fields.transferJournal', 'Transfer'), type: 'select', options: journalOptions(journalTypes.transfer) },
          { name: 'countJournalNameId', label: t('inventParameters.fields.countingJournal', 'Counting'), type: 'select', options: journalOptions(journalTypes.counting) },
        ],
      },
    ];
  }, [lookups.data, t]);

  if (parameters.isLoading || lookups.isLoading) return <Box sx={{ height: '100%', display: 'grid', placeItems: 'center' }}><CircularProgress size={28} /></Box>;
  const error = parameters.error ?? lookups.error;
  if (error || !parameters.data || !lookups.data) return <Box sx={{ p: 2 }}><Alert severity="error">{error instanceof Error ? error.message : t('errors.generic', 'Unable to load inventory parameters.')}</Alert></Box>;

  const initialValues: SetupValues = { ...parameters.data };
  return <SetupPage key={`${company}-${parameters.data.recId}`} title={t('inventParameters.title', 'Inventory and warehouse management parameters')} viewLabel={t('common.standardView', 'Standard view')} navigationItems={navigationItems} sections={sections} initialValues={initialValues} saveLabel={t('actions.save')} optionsLabel={t('customerCommands.options')} yesLabel={t('common.yes', 'Yes')} noLabel={t('common.no', 'No')} savedMessage={t('inventParameters.saved', 'Inventory parameters saved.')} onSave={async (values) => {
    const saved = await inventParametersApi.save(values as unknown as InventParametersRecord);
    queryClient.setQueryData(queryKey, saved);
  }} />;
}
