import React, { useMemo } from 'react';
import { Alert, Typography } from '@mui/material';
import { SetupPage } from '@patterns/setup/SetupPage';
import type { SetupNavigationItem, SetupSectionConfig, SetupValues } from '@patterns/setup/types';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { deepEqual } from '@shared/utils/deepEqual';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import {
  useGlobalSettings,
  useUpdateGlobalSettings,
  useUpdateUserSettings,
  useUserSettings,
} from '../queries/settingsQueries';
import { globalSettingsSchema, userSettingsSchema } from '../validation/settingsSchemas';

export function ApplicationSettingsPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const { hasPermission } = useAuth();
  const globalSettings = useGlobalSettings();
  const userSettings = useUserSettings();
  const updateGlobal = useUpdateGlobalSettings();
  const updateUser = useUpdateUserSettings();
  const canUpdateGlobal = hasPermission(PERMISSIONS.SETTINGS_UPDATE);

  const navigationItems = useMemo<SetupNavigationItem[]>(
    () => [
      { id: 'application', label: t('settings.application', 'Application defaults') },
      { id: 'system', label: t('settings.system', 'System behavior') },
      { id: 'preferences', label: t('settings.preferences', 'Personal preferences') },
    ],
    [t]
  );

  const sections = useMemo<SetupSectionConfig[]>(
    () => [
      {
        id: 'application',
        title: t('settings.application', 'Application defaults'),
        fields: [
          { name: 'appName', label: t('settings.appName', 'Application name'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'defaultLanguage', label: t('settings.defaultLanguage', 'Default language'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'timeZone', label: t('settings.timeZone', 'Time zone'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'currency', label: t('settings.currency', 'Currency'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'dateFormat', label: t('settings.dateFormat', 'Date format'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'decimalPlaces', label: t('settings.decimalPlaces', 'Decimal places'), type: 'number', min: 0, disabled: !canUpdateGlobal },
        ],
      },
      {
        id: 'system',
        title: t('settings.system', 'System behavior'),
        fields: [
          { name: 'paginationSize', label: t('settings.paginationSize', 'Default page size'), type: 'number', min: 1, max: 100, disabled: !canUpdateGlobal },
          { name: 'maxUploadSize', label: t('settings.maxUploadSize', 'Maximum upload size (bytes)'), type: 'number', min: 1, disabled: !canUpdateGlobal },
          { name: 'enableAuditLog', label: t('settings.enableAuditLog', 'Enable audit logging'), type: 'boolean', disabled: !canUpdateGlobal },
        ],
      },
      {
        id: 'preferences',
        title: t('settings.preferences', 'Personal preferences'),
        fields: [
          { name: 'theme', label: t('settings.theme', 'Theme'), type: 'text' },
          { name: 'language', label: t('settings.language', 'Language'), type: 'text' },
          { name: 'pageSize', label: t('settings.pageSize', 'Page size'), type: 'number', min: 1, max: 100 },
          { name: 'dashboardLayout', label: t('settings.dashboardLayout', 'Dashboard layout'), type: 'text' },
          { name: 'notificationEnabled', label: t('settings.notificationEnabled', 'Enable notifications'), type: 'boolean' },
        ],
      },
    ],
    [canUpdateGlobal, t]
  );

  const retry = () => {
    void Promise.all([globalSettings.refetch(), userSettings.refetch()]);
  };

  if (globalSettings.isLoading || userSettings.isLoading) {
    return <LoadingState message={t('messages.loadingSettings', 'Loading settings…')} />;
  }

  if (globalSettings.error || userSettings.error || !globalSettings.data || !userSettings.data) {
    const error = globalSettings.error ?? userSettings.error;
    return <ErrorState message={error?.message ?? t('errors.loadFailed')} onRetry={retry} />;
  }

  const global = globalSettings.data;
  const user = userSettings.data;
  const initialValues: SetupValues = {
    appName: global.appName,
    defaultLanguage: global.defaultLanguage,
    timeZone: global.timeZone,
    currency: global.currency,
    dateFormat: global.dateFormat,
    decimalPlaces: global.decimalPlaces,
    paginationSize: global.paginationSize,
    maxUploadSize: global.maxUploadSize,
    enableAuditLog: global.enableAuditLog,
    theme: user.theme,
    language: user.language,
    pageSize: user.pageSize,
    dashboardLayout: user.dashboardLayout,
    notificationEnabled: user.notificationEnabled,
  };

  const save = async (values: SetupValues) => {
    const nextGlobal = globalSettingsSchema.parse({
      ...global,
      appName: String(values.appName),
      defaultLanguage: String(values.defaultLanguage),
      timeZone: String(values.timeZone),
      currency: String(values.currency),
      dateFormat: String(values.dateFormat),
      decimalPlaces: Number(values.decimalPlaces),
      paginationSize: Number(values.paginationSize),
      maxUploadSize: Number(values.maxUploadSize),
      enableAuditLog: Boolean(values.enableAuditLog),
    });
    const nextUser = userSettingsSchema.parse({
      ...user,
      theme: String(values.theme),
      language: String(values.language),
      pageSize: Number(values.pageSize),
      dashboardLayout: String(values.dashboardLayout),
      notificationEnabled: Boolean(values.notificationEnabled),
    });
    const updates: Array<Promise<unknown>> = [];
    if (canUpdateGlobal && !deepEqual(nextGlobal, global)) updates.push(updateGlobal.mutateAsync(nextGlobal));
    if (!deepEqual(nextUser, user)) updates.push(updateUser.mutateAsync(nextUser));
    await Promise.all(updates);
  };

  return (
    <SetupPage
      title={t('pages.settings.title')}
      viewLabel=""
      navigationItems={navigationItems}
      sections={sections}
      initialValues={initialValues}
      saveLabel={t('actions.save')}
      optionsLabel={t('common.options', 'Options')}
      yesLabel={t('common.yes', 'Yes')}
      noLabel={t('common.no', 'No')}
      savedMessage={t('settings.saved', 'Settings saved.')}
      onSave={save}
      headerContent={
        <>
          <Typography sx={{ mt: 0.25, fontSize: 12, color: 'text.secondary' }}>
            {t('pages.settings.subtitle')}
          </Typography>
          {!canUpdateGlobal && (
            <Alert severity="info" sx={{ mt: 1, py: 0 }}>
              {t('settings.globalReadOnly', 'Global settings are read-only for your account.')}
            </Alert>
          )}
        </>
      }
    />
  );
}
