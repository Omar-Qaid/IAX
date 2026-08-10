import React, { useState } from 'react';
import { Tab, Tabs } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageContent } from '@shared/components/page/PageContent';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import { GlobalSettingsForm } from '../components/GlobalSettingsForm';
import { UserSettingsForm } from '../components/UserSettingsForm';
import { useGlobalSettings, useUserSettings } from '../queries/settingsQueries';

type SettingsTab = 'global' | 'user';

export function ApplicationSettingsPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const { hasPermission } = useAuth();
  const [activeTab, setActiveTab] = useState<SettingsTab>('global');
  const globalSettings = useGlobalSettings();
  const userSettings = useUserSettings();

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

  return (
    <PageContainer>
      <PageHeader title={t('pages.settings.title')} subtitle={t('pages.settings.subtitle')} />
      <PageContent>
        <Tabs
          value={activeTab}
          onChange={(_event, value: SettingsTab) => setActiveTab(value)}
          aria-label={t('pages.settings.title')}
          sx={{ mb: 2, borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab value="global" label={t('settings.globalTab', 'Global settings')} />
          <Tab value="user" label={t('settings.userTab', 'My preferences')} />
        </Tabs>
        {activeTab === 'global' ? (
          <GlobalSettingsForm
            settings={globalSettings.data}
            canUpdate={hasPermission(PERMISSIONS.SETTINGS_UPDATE)}
          />
        ) : (
          <UserSettingsForm settings={userSettings.data} />
        )}
      </PageContent>
    </PageContainer>
  );
}
