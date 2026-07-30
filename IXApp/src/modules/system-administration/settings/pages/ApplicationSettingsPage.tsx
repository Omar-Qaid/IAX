import React from 'react';
import { MasterFormPage } from '@patterns/master-form/MasterFormPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { FastTabs } from '@shared/components/fast-tabs/FastTabs';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { FormRow, FormColumn } from '@shared/components/forms/FormRow';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { AppSelectField } from '@shared/components/fields/AppSelectField';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';
import { useForm } from 'react-hook-form';
import { useNotifications } from '@shared/hooks/useNotifications';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import SaveIcon from '@mui/icons-material/Save';

interface SettingsFormData {
  appName: string;
  defaultCompany: string;
  language: 'en' | 'ar';
  themeMode: 'light' | 'dark';
  customerSequenceFormat: string;
  salesOrderSequenceFormat: string;
  apiBaseUrl: string;
  enableMockApi: boolean;
}

export const ApplicationSettingsPage: React.FC = () => {
  const { notifySuccess } = useNotifications();
  const { themeMode, setThemeMode } = usePreferenceStore();
  const { currentLanguage, changeLanguage } = useAppTranslation();

  const { control, handleSubmit } = useForm<SettingsFormData>({
    defaultValues: {
      appName: 'IXApp Enterprise Solutions',
      defaultCompany: 'USMF',
      language: currentLanguage.code,
      themeMode: themeMode,
      customerSequenceFormat: 'US-####',
      salesOrderSequenceFormat: 'SO-#####',
      apiBaseUrl: 'https://localhost:7001/api',
      enableMockApi: true,
    },
  });

  const onSubmit = (data: SettingsFormData) => {
    setThemeMode(data.themeMode);
    changeLanguage(data.language);
    notifySuccess('Application parameters and preferences updated successfully');
  };

  return (
    <MasterFormPage
      title="Application Settings"
      subtitle="System Administration Global Parameters & User Interface Preferences"
      actionPane={
        <ActionPaneGroup label="Maintain">
          <ActionPaneButton
            label="Save Parameters"
            icon={<SaveIcon fontSize="small" />}
            onClick={handleSubmit(onSubmit)}
            permission="settings.update"
          />
        </ActionPaneGroup>
      }
    >
      <FastTabs>
        <FastTab id="general-settings" title="General System Parameters" summary="Company & Application Branding">
          <FormRow>
            <FormColumn md={6}>
              <AppTextField name="appName" label="Application Title" control={control} required />
            </FormColumn>
            <FormColumn md={6}>
              <AppSelectField
                name="defaultCompany"
                label="Default Legal Entity"
                control={control}
                options={[
                  { value: 'USMF', label: 'Contoso Entertainment USA (USMF)' },
                  { value: 'DEMO', label: 'Demo Entity (DEMO)' },
                  { value: 'DAT', label: 'Default Entity (DAT)' },
                ]}
              />
            </FormColumn>
          </FormRow>
        </FastTab>

        <FastTab id="ui-settings" title="User Interface & Localization" summary="Theme Mode & Direction">
          <FormRow>
            <FormColumn md={6}>
              <AppSelectField
                name="language"
                label="Interface Language"
                control={control}
                options={[
                  { value: 'en', label: 'English (LTR)' },
                  { value: 'ar', label: 'Arabic - العربية (RTL)' },
                ]}
              />
            </FormColumn>
            <FormColumn md={6}>
              <AppSelectField
                name="themeMode"
                label="Theme Palette"
                control={control}
                options={[
                  { value: 'light', label: 'Enterprise Light Theme' },
                  { value: 'dark', label: 'Sleek Dark Theme' },
                ]}
              />
            </FormColumn>
          </FormRow>
        </FastTab>

        <FastTab id="number-sequences" title="Number Sequences" summary="Automated Entity Format Masks">
          <FormRow>
            <FormColumn md={6}>
              <AppTextField name="customerSequenceFormat" label="Customer Account Sequence Format" control={control} />
            </FormColumn>
            <FormColumn md={6}>
              <AppTextField name="salesOrderSequenceFormat" label="Sales Order Sequence Format" control={control} />
            </FormColumn>
          </FormRow>
        </FastTab>

        <FastTab id="api-settings" title="API Configuration & Integration" summary="REST Endpoint Settings">
          <FormRow>
            <FormColumn md={8}>
              <AppTextField name="apiBaseUrl" label="ASP.NET Core REST Web API Base URL" control={control} />
            </FormColumn>
            <FormColumn md={4}>
              <AppBooleanField name="enableMockApi" label="Enable Local Mock API Mode" control={control} />
            </FormColumn>
          </FormRow>
        </FastTab>
      </FastTabs>
    </MasterFormPage>
  );
};
