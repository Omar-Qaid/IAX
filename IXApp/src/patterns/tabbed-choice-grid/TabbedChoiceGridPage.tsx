import React from 'react';
import {
  Alert,
  Box,
  FormControlLabel,
  Radio,
  RadioGroup,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { PageContainer } from '@shared/components/page/PageContainer';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import type { TabbedChoiceGridPageProps } from './types';

export function TabbedChoiceGridPage<T extends { id: string }>({
  title,
  viewLabel,
  chooserLabel,
  tabs,
  rows,
  columns,
  loading,
  error,
  editing = false,
  saving = false,
  canEdit = true,
  canCreate = true,
  canDelete = true,
  selectedRowIds = [],
  initialTabId,
  initialChoiceId,
  storageKey,
  gridProps,
  onContextChange,
  onSelectionChange,
  onEdit,
  onNew,
  onDelete,
  onSave,
  onCancel,
  onRefresh,
}: TabbedChoiceGridPageProps<T>): React.ReactElement {
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const firstTab = tabs.find((tab) => tab.choices.length > 0) ?? tabs[0];
  const [tabId, setTabId] = React.useState(initialTabId ?? firstTab?.id ?? '');
  const activeTab = tabs.find((tab) => tab.id === tabId) ?? firstTab;
  const [choiceId, setChoiceId] = React.useState(
    initialChoiceId ?? activeTab?.choices[0]?.id ?? ''
  );
  React.useEffect(() => {
    if (!activeTab?.choices.some((choice) => choice.id === choiceId))
      setChoiceId(activeTab?.choices[0]?.id ?? '');
  }, [activeTab, choiceId]);
  React.useEffect(() => {
    if (tabId && choiceId) onContextChange?.(tabId, choiceId);
  }, [choiceId, onContextChange, tabId]);
  return (
    <PageContainer
      sx={{
        gap: 0,
        height: '100%',
        minHeight: 0,
        overflow: 'hidden',
        p: 0.75,
        bgcolor: d365.canvas,
      }}
    >
      <ActionPane
        variant="flat"
        endActions={
          <EnterpriseCommandUtilities
            personalizeLabel={t('utilities.personalize')}
            guideLabel={t('utilities.guide')}
            notificationsLabel={t('common.notifications')}
            refreshLabel={t('actions.refresh')}
            openWindowLabel={t('utilities.openWindow')}
            disabled={editing}
            onRefresh={onRefresh}
          />
        }
      >
        <ActionPaneBackButton
          label={t('actions.back')}
          onClick={() => navigate(-1)}
          disabled={editing || saving}
        />
        <EnterpriseCrudActions
          editLabel={t('actions.edit')}
          newLabel={t('actions.new')}
          deleteLabel={t('actions.delete')}
          saveLabel={t('actions.save')}
          cancelLabel={t('actions.cancel')}
          canEdit={canEdit && selectedRowIds.length === 1}
          canNew={canCreate}
          canDelete={canDelete && selectedRowIds.length === 1}
          editing={editing}
          saving={saving}
          onEdit={onEdit}
          onNew={onNew}
          onDelete={onDelete}
          onSave={onSave}
          onCancel={onCancel}
        />
        <OptionsMenu
          record={rows.find((row) => selectedRowIds.includes(row.id)) ?? null}
          tableName={title}
          getRecordId={(record) => record?.id ?? ''}
          title={title}
          disabled={editing}
        />
      </ActionPane>
      <Box sx={{ px: 2, pt: 0.75 }}>
        <Typography sx={{ fontSize: '0.75rem' }}>
          {viewLabel ?? t('common.standardView')}
        </Typography>
        <Typography component="h1" sx={{ fontSize: '1.45rem', fontWeight: 600, lineHeight: 1.35 }}>
          {title}
        </Typography>
        <Tabs
          value={activeTab?.id ?? false}
          onChange={(_, value: string) => {
            setTabId(value);
            setChoiceId(tabs.find((tab) => tab.id === value)?.choices[0]?.id ?? '');
          }}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            mt: 1,
            minHeight: 36,
            '& .MuiTab-root': {
              minHeight: 36,
              py: 0,
              px: 1.25,
              textTransform: 'none',
              fontSize: '0.78rem',
            },
          }}
        >
          {tabs.map((tab) => (
            <Tab
              key={tab.id}
              value={tab.id}
              label={tab.label}
              disabled={editing || tab.choices.length === 0}
            />
          ))}
        </Tabs>
      </Box>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: '244px minmax(0, 1fr)' },
          minHeight: 0,
          flex: 1,
          px: 2,
          pb: 1.5,
        }}
      >
        <Box
          sx={{
            py: 1,
            pe: 1.5,
            overflowY: 'auto',
            borderInlineEnd: { md: `1px solid ${d365.border}` },
          }}
        >
          <Typography sx={{ fontSize: '0.72rem', mb: 0.5 }}>
            {chooserLabel ?? t('actions.select', 'Select')}
          </Typography>
          <RadioGroup value={choiceId} onChange={(event) => setChoiceId(event.target.value)}>
            {(activeTab?.choices ?? []).map((choice) => (
              <FormControlLabel
                key={choice.id}
                value={choice.id}
                disabled={editing}
                control={<Radio size="small" sx={{ p: '4px 8px 4px 0' }} />}
                label={choice.label}
                sx={{
                  m: 0,
                  minHeight: 31,
                  alignItems: 'flex-start',
                  '& .MuiFormControlLabel-label': {
                    fontSize: '0.78rem',
                    lineHeight: 1.3,
                    pt: '5px',
                  },
                }}
              />
            ))}
          </RadioGroup>
        </Box>
        <Box sx={{ minWidth: 0, minHeight: 0, pt: 1 }}>
          {error ? (
            <Alert severity="error">{error}</Alert>
          ) : (
            <DataGrid
              rows={rows}
              columns={columns}
              loading={loading}
              height="100%"
              selectionMode="single"
              selectedIds={selectedRowIds}
              onSelectionChange={(ids) => onSelectionChange?.(ids as (string | number)[])}
              hideToolbar
              hideFooter
              hideFilterRow
              storageKey={storageKey}
              {...gridProps}
            />
          )}
        </Box>
      </Box>
    </PageContainer>
  );
}
