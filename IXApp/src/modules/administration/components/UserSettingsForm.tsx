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
import { useUpdateUserSettings } from '../queries/settingsQueries';
import type { UserSettings } from '../types/settingsTypes';
import { userSettingsSchema } from '../validation/settingsSchemas';

export function UserSettingsForm({ settings }: { settings: UserSettings }): React.ReactElement {
  const { t } = useAppTranslation();
  const { notifySuccess, notifyError } = useNotifications();
  const updateSettings = useUpdateUserSettings();
  const { control, handleSubmit, reset, formState } = useForm<UserSettings>({
    defaultValues: settings,
    resolver: zodResolver(userSettingsSchema),
  });

  useEffect(() => reset(settings), [reset, settings]);

  const save = handleSubmit(async (values) => {
    try {
      const updated = await updateSettings.mutateAsync(values);
      reset(updated);
      notifySuccess(t('settings.userSaved', 'User settings saved.'));
    } catch (error) {
      notifyError(error instanceof Error ? error.message : t('errors.generic'));
    }
  });

  return (
    <Box component="form" onSubmit={save} noValidate>
      <UnsavedChangesGuard isDirty={formState.isDirty} />
      {updateSettings.error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {updateSettings.error.message}
        </Alert>
      )}
      <PageSection
        title={t('settings.preferences', 'Personal preferences')}
        description={t(
          'settings.preferencesHelp',
          'Settings associated with your authenticated user.'
        )}
      >
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          <AppTextField
            name="theme"
            label={t('settings.theme', 'Theme')}
            control={control}
            required
          />
          <AppTextField
            name="language"
            label={t('settings.language', 'Language')}
            control={control}
            required
          />
          <AppNumberField
            name="pageSize"
            label={t('settings.pageSize', 'Page size')}
            control={control}
            required
          />
          <AppTextField
            name="dashboardLayout"
            label={t('settings.dashboardLayout', 'Dashboard layout')}
            control={control}
          />
          <AppBooleanField
            name="notificationEnabled"
            label={t('settings.notificationEnabled', 'Enable notifications')}
            control={control}
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
          disabled={!formState.isDirty || updateSettings.isPending}
        >
          {updateSettings.isPending ? t('messages.saving', 'Saving…') : t('actions.save')}
        </Button>
      </Stack>
    </Box>
  );
}
