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
      notifySuccess(t('settings.saved'));
    } catch (error) {
      notifyError(error instanceof Error ? error.message : t('errors.generic'));
    }
  });

  return (
    <Box component="form" onSubmit={save} noValidate>
      <UnsavedChangesGuard isDirty={formState.isDirty} />
      {!canUpdate && (
        <Alert severity="info" sx={{ mb: 2 }}>
          {t('settings.globalReadOnly')}
        </Alert>
      )}
      {updateSettings.error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {updateSettings.error.message}
        </Alert>
      )}
      <PageSection
        title={t('settings.application')}
        description={t('settings.applicationHelp')}
      >
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          <AppTextField
            name="appName"
            label={t('settings.appName')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="defaultLanguage"
            label={t('settings.defaultLanguage')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="timeZone"
            label={t('settings.timeZone')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="currency"
            label={t('settings.currency')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppTextField
            name="dateFormat"
            label={t('settings.dateFormat')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppNumberField
            name="decimalPlaces"
            label={t('settings.decimalPlaces')}
            control={control}
            disabled={!canUpdate}
            required
          />
        </Box>
      </PageSection>
      <PageSection title={t('settings.system')}>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          <AppNumberField
            name="paginationSize"
            label={t('settings.paginationSize')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppNumberField
            name="maxUploadSize"
            label={t('settings.maxUploadSize')}
            control={control}
            disabled={!canUpdate}
            required
          />
          <AppBooleanField
            name="enableAuditLog"
            label={t('settings.enableAuditLog')}
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
          {updateSettings.isPending ? t('messages.saving') : t('actions.save')}
        </Button>
      </Stack>
    </Box>
  );
}
