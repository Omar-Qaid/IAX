import React, { useEffect } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Box, Button, Stack } from '@mui/material';
import { useForm } from 'react-hook-form';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';
import { AppNumberField } from '@shared/components/fields/AppNumberField';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { PageSection } from '@shared/components/page/PageSection';
import { UnsavedChangesGuard } from '@shared/components/page/UnsavedChangesGuard';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useUpdateGlobalSettings } from '../queries/settingsQueries';
import type { GlobalSettings } from '../types/settingsTypes';
import { globalSettingsSchema } from '../validation/settingsSchemas';

export interface GlobalSettingsFormProps {
  settings: GlobalSettings;
  canUpdate: boolean;
}

export function GlobalSettingsForm({
  settings,
  canUpdate,
}: GlobalSettingsFormProps): React.ReactElement {
  const { t } = useAppTranslation();
  const { notifySuccess, notifyError } = useNotifications();
  const updateSettings = useUpdateGlobalSettings();
  const { control, handleSubmit, reset, formState } = useForm<GlobalSettings>({
    defaultValues: settings,
    resolver: zodResolver(globalSettingsSchema),
  });

  useEffect(() => reset(settings), [reset, settings]);

  const save = handleSubmit(async (values) => {
    try {
      const updated = await updateSettings.mutateAsync(values);
      reset(updated);
      notifySuccess(t('settings.saved', 'Global settings saved.'));
    } catch (error) {
      notifyError(error instanceof Error ? error.message : t('errors.generic'));
    }
  });

  return (
    <Box component="form" onSubmit={save} noValidate>
      <UnsavedChangesGuard isDirty={formState.isDirty} />
      {!canUpdate && (
        <Alert severity="info" sx={{ mb: 2 }}>
          {t('settings.globalReadOnly', 'Global settings are read-only for your account.')}
        </Alert>
      )}
      {updateSettings.error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {updateSettings.error.message}
        </Alert>
      )}
      <PageSection
        title={t('settings.application', 'Application defaults')}
        description={t('settings.applicationHelp', 'Defaults applied across the application.')}
      >
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          <AppTextField
            name="appName"
            label={t('settings.appName', 'Application name')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="defaultLanguage"
            label={t('settings.defaultLanguage', 'Default language')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="timeZone"
            label={t('settings.timeZone', 'Time zone')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="currency"
            label={t('settings.currency', 'Currency')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="dateFormat"
            label={t('settings.dateFormat', 'Date format')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppNumberField
            name="decimalPlaces"
            label={t('settings.decimalPlaces', 'Decimal places')}
            control={control}
            disabled={!canUpdate}
            required
          />
        </Box>
      </PageSection>
      <PageSection title={t('settings.system', 'System behavior')}>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          <AppNumberField
            name="paginationSize"
            label={t('settings.paginationSize', 'Default page size')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppNumberField
            name="maxUploadSize"
            label={t('settings.maxUploadSize', 'Maximum upload size (bytes)')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppBooleanField
            name="enableAuditLog"
            label={t('settings.enableAuditLog', 'Enable audit logging')}
            control={control}
            disabled={!canUpdate}
          />
        </Box>
      </PageSection>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button
          type="button"
          onClick={() => reset(settings)}
          disabled={!formState.isDirty || updateSettings.isPending}
        >
          {t('actions.reset', 'Reset')}
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={!canUpdate || !formState.isDirty || updateSettings.isPending}
        >
          {updateSettings.isPending ? t('messages.saving', 'Saving…') : t('actions.save')}
        </Button>
      </Stack>
    </Box>
  );
}
