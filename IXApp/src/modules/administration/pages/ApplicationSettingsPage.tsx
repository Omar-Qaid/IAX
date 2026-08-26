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
      { id: 'application', label: t('settings.application') },
      { id: 'system', label: t('settings.system') },
      { id: 'preferences', label: t('settings.preferences') },
    ],
    [t]
  );

  const sections = useMemo<SetupSectionConfig[]>(
    () => [
      {
        id: 'application',
        title: t('settings.application'),
        fields: [
          { name: 'appName', label: t('settings.appName'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'defaultLanguage', label: t('settings.defaultLanguage'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'timeZone', label: t('settings.timeZone'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'currency', label: t('settings.currency'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'dateFormat', label: t('settings.dateFormat'), type: 'text', disabled: !canUpdateGlobal },
          { name: 'decimalPlaces', label: t('settings.decimalPlaces'), type: 'number', min: 0, disabled: !canUpdateGlobal },
        ],
      },
      {
        id: 'system',
        title: t('settings.system'),
        fields: [
          { name: 'paginationSize', label: t('settings.paginationSize'), type: 'number', min: 1, max: 100, disabled: !canUpdateGlobal },
          { name: 'maxUploadSize', label: t('settings.maxUploadSize'), type: 'number', min: 1, disabled: !canUpdateGlobal },
          { name: 'enableAuditLog', label: t('settings.enableAuditLog'), type: 'boolean', disabled: !canUpdateGlobal },
        ],
      },
      {
        id: 'preferences',
        title: t('settings.preferences'),
        fields: [
          { name: 'theme', label: t('settings.theme'), type: 'text' },
          { name: 'language', label: t('settings.language'), type: 'text' },
          { name: 'pageSize', label: t('settings.pageSize'), type: 'number', min: 1, max: 100 },
          { name: 'dashboardLayout', label: t('settings.dashboardLayout'), type: 'text' },
          { name: 'notificationEnabled', label: t('settings.notificationEnabled'), type: 'boolean' },
        ],
      },
    ],
    [canUpdateGlobal, t]
  );

  const retry = () => {
    void Promise.all([globalSettings.refetch(), userSettings.refetch()]);
  };

  if (globalSettings.isLoading || userSettings.isLoading) {
    return <LoadingState message={t('messages.loadingSettings')} />;
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
      optionsLabel={t('common.options')}
      yesLabel={t('common.yes', 'Yes')}
      noLabel={t('common.no', 'No')}
      savedMessage={t('settings.saved')}
      onSave={save}
      headerContent={
        <>
          <Typography sx={{ mt: 0.25, fontSize: 12, color: 'text.secondary' }}>
            {t('pages.settings.subtitle')}
          </Typography>
          {!canUpdateGlobal && (
            <Alert severity="info" sx={{ mt: 1, py: 0 }}>
              {t('settings.globalReadOnly')}
            </Alert>
          )}
        </>
      }
    />
  );
}
