import React from 'react';
import { Stack, Typography } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { PageContent } from '@shared/components/page/PageContent';
import { PageSection } from '@shared/components/page/PageSection';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function ApplicationSettingsPage(): React.ReactElement {
  const { t } = useAppTranslation();
  return (
    <PageContainer>
      <PageHeader title={t('pages.settings.title')} subtitle={t('pages.settings.subtitle')} badge={t('common.readOnly')} />
      <PageContent>
        <PageSection title={t('pages.settings.environment')} description={t('pages.settings.environmentHelp')}>
          <Stack spacing={1}>
            <Typography variant="body2">{t('pages.settings.frontend')}</Typography>
            <Typography variant="body2">{t('pages.settings.api')}</Typography>
            <Typography variant="body2">{t('pages.settings.localization')}</Typography>
          </Stack>
        </PageSection>
      </PageContent>
    </PageContainer>
  );
}
