import React, { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Box, MenuItem, Snackbar, Switch, TextField, Typography } from '@mui/material';
import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';
import SearchIcon from '@mui/icons-material/Search';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { SetupNavigation } from './SetupNavigation';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { deepEqual } from '@shared/utils/deepEqual';
import type { SetupFieldConfig, SetupPageProps, SetupValue } from './types';
import { LookupNavigationLabel } from '@shared/components/lookups/LookupNavigationLabel';
import { RecordPreview } from '@shared/components/record-preview';

function SetupField({
  field,
  value,
  yesLabel,
  noLabel,
  onChange,
}: {
  field: SetupFieldConfig;
  value: SetupValue | undefined;
  yesLabel: string;
  noLabel: string;
  onChange: (value: SetupValue) => void;
}): React.ReactElement {
  const inputSx = {
    width: field.width ?? '100%',
    maxWidth: '100%',
    '& .MuiInputBase-root': { height: 29, borderRadius: 0.5, fontSize: '0.75rem' },
    '& .MuiInputBase-input': { px: 0.75, py: 0.5 },
  };

  return (
    <RecordPreview
      masterRoute={field.masterRoute}
      entity={field.previewEntity}
      recordKey={typeof value === 'string' || typeof value === 'number' ? value : null}
      option={
        field.options?.find((option) => option.value === String(value))
          ? {
              code: String(value),
              name: field.options.find((option) => option.value === String(value))!.label,
            }
          : null
      }
    >
      <Box sx={{ minWidth: 0 }}>
        <Typography
          title={field.label}
          noWrap
          sx={{ mb: 0.25, color: 'text.secondary', fontSize: '0.6875rem', lineHeight: 1.3 }}
        >
          <LookupNavigationLabel label={field.label} masterRoute={field.masterRoute} />
        </Typography>
        {field.type === 'boolean' ? (
          <Box sx={{ display: 'flex', alignItems: 'center', minHeight: 29 }}>
            <Switch
              checked={Boolean(value)}
              disabled={field.disabled}
              onChange={(_, checked) => onChange(checked)}
              size="small"
              slotProps={{ input: { 'aria-label': field.label } }}
              sx={{
                marginInlineStart: -0.75,
                marginInlineEnd: 0.25,
                '& .MuiSwitch-switchBase': { p: '5px' },
                '& .MuiSwitch-thumb': { width: 13, height: 13 },
                '& .MuiSwitch-track': {
                  border: '1px solid',
                  borderColor: 'text.secondary',
                  bgcolor: 'transparent',
                  opacity: 1,
                },
                '& .Mui-checked + .MuiSwitch-track': {
                  borderColor: 'primary.main',
                  bgcolor: 'primary.main',
                  opacity: 1,
                },
              }}
            />
            <Typography sx={{ fontSize: '0.75rem' }}>{value ? yesLabel : noLabel}</Typography>
          </Box>
        ) : field.type === 'select' ? (
          <TextField
            select
            value={value ?? ''}
            disabled={field.disabled}
            onChange={(event) => onChange(event.target.value)}
            slotProps={{ htmlInput: { 'aria-label': field.label } }}
            sx={inputSx}
          >
            {(field.options ?? []).map((option) => (
              <MenuItem key={option.value} value={option.value} sx={{ fontSize: '0.75rem' }}>
                {option.label}
              </MenuItem>
            ))}
          </TextField>
        ) : (
          <TextField
            type={field.type === 'number' ? 'number' : 'text'}
            value={value ?? ''}
            disabled={field.disabled}
            slotProps={{ htmlInput: { min: field.min, max: field.max, 'aria-label': field.label } }}
            onChange={(event) =>
              onChange(field.type === 'number' ? Number(event.target.value) : event.target.value)
            }
            sx={inputSx}
          />
        )}
      </Box>
    </RecordPreview>
  );
}

export function SetupPage({
  title,
  navigationItems,
  sections,
  initialValues,
  saveLabel,
  yesLabel,
  noLabel,
  savedMessage,
  headerContent,
  readOnly = false,
  onSave,
}: SetupPageProps): React.ReactElement {
  const navigate = useNavigate();
  const [values, setValues] = useState(initialValues);
  const [savedValues, setSavedValues] = useState(initialValues);
  const [saving, setSaving] = useState(false);
  const [showSaved, setShowSaved] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [searchOpen, setSearchOpen] = useState(false);
  const [search, setSearch] = useState('');
  const query = search.trim().toLocaleLowerCase();
  const visibleSections = useMemo(
    () =>
      sections.flatMap((section) => {
        if (!query || section.title.toLocaleLowerCase().includes(query)) return [section];
        const fields = section.fields.filter((field) =>
          field.label.toLocaleLowerCase().includes(query)
        );
        return fields.length ? [{ ...section, fields }] : [];
      }),
    [sections, query]
  );
  const [activeId, setActiveId] = useState(navigationItems[0]?.id ?? sections[0]?.id ?? '');
  const dirty = useMemo(() => !deepEqual(values, savedValues), [savedValues, values]);
  const { t } = useAppTranslation();
  useUnsavedChanges(dirty, t('messages.unsavedChanges', 'You have unsaved changes.'));

  const selectSection = (id: string) => {
    setActiveId(id);
    document
      .getElementById(`setup-section-${id}`)
      ?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };
  const save = async () => {
    if (!onSave || readOnly) return;
    setSaving(true);
    setSaveError(null);
    try {
      await onSave(values);
      setSavedValues(values);
      setShowSaved(true);
    } catch (error) {
      setSaveError(error instanceof Error ? error.message : t('errors.generic', 'Unable to save.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box
      sx={{
        height: '100%',
        minHeight: 0,
        display: 'flex',
        flexDirection: 'column',
        bgcolor: (theme) => enterpriseSurfaces(theme).canvas,
        p: 0.75,
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
          />
        }
      >
        <ActionPaneBackButton
          label={t('actions.back')}
          onClick={() => navigate(-1)}
          disabled={saving}
        />
        <ActionPaneGroup>
          <ActionPaneButton
            label={saveLabel}
            icon={<SaveOutlinedIcon />}
            disabled={readOnly || !onSave || !dirty || saving}
            onClick={save}
          />
        </ActionPaneGroup>
        <ActionPaneGroup>
          <ActionPaneButton
            label={t('actions.search', 'Search')}
            icon={<SearchIcon />}
            onClick={() => {
              setSearchOpen((open) => !open);
              setSearch('');
            }}
          />
          <OptionsMenu
            record={values}
            tableName={title}
            getRecordId={() => 1}
            title={title}
            disabled={saving}
          />
        </ActionPaneGroup>
      </ActionPane>

      <Box sx={{ px: 2, pt: 0.5, pb: 1.25 }}>
        <Typography component="h1" sx={{ fontSize: '1.35rem', fontWeight: 600, lineHeight: 1.35 }}>
          {title}
        </Typography>
        {headerContent}
        {!onSave && (
          <Alert severity="info" sx={{ mt: 1 }}>
            {t('workspaceHelp.previewOnly')}
          </Alert>
        )}
        {searchOpen && (
          <TextField
            autoFocus
            size="small"
            label={t('actions.search', 'Search')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ mt: 1 }}
          />
        )}
      </Box>

      <Box
        sx={{
          mx: 2,
          mb: 0.5,
          minHeight: 0,
          flex: 1,
          display: 'flex',
          flexDirection: { xs: 'column', md: 'row' },
          overflow: 'hidden',
          bgcolor: 'background.paper',
          border: 1,
          borderColor: 'divider',
          borderRadius: 1,
          boxShadow: '0 1px 4px rgba(0,0,0,.12)',
        }}
      >
        <SetupNavigation
          items={
            query
              ? navigationItems.filter((item) =>
                  visibleSections.some((section) => section.id === item.id)
                )
              : navigationItems
          }
          activeId={activeId}
          onSelect={selectSection}
        />
        <Box
          sx={{ width: 7, display: { xs: 'none', md: 'block' }, bgcolor: 'divider', flexShrink: 0 }}
        />
        <Box sx={{ minWidth: 0, flex: 1, overflowY: 'auto', px: 1.25, py: 0.25 }}>
          {visibleSections.length === 0 && (
            <Typography role="status" sx={{ p: 2 }}>
              {t('common.noResults', 'No results found.')}
            </Typography>
          )}
          {visibleSections.map((section) => (
            <FastTab
              key={section.id}
              id={`setup-section-${section.id}`}
              title={section.title}
              appearance="enterprise"
              defaultExpanded={section.defaultExpanded ?? true}
              expanded={query ? true : undefined}
              sx={{ scrollMarginTop: 8 }}
              detailsSx={{ px: 1, py: 1.25 }}
            >
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: {
                    xs: '1fr',
                    sm: 'repeat(2, minmax(160px, 1fr))',
                    lg: 'repeat(4, minmax(170px, 1fr))',
                    xl: 'repeat(6, minmax(160px, 1fr))',
                  },
                  columnGap: 3,
                  rowGap: 1,
                }}
              >
                {section.fields.map((field) => (
                  <SetupField
                    key={field.name}
                    field={onSave && !readOnly ? field : { ...field, disabled: true }}
                    value={values[field.name]}
                    yesLabel={yesLabel}
                    noLabel={noLabel}
                    onChange={(value) =>
                      setValues((current) => ({ ...current, [field.name]: value }))
                    }
                  />
                ))}
              </Box>
            </FastTab>
          ))}
        </Box>
      </Box>
      <Snackbar open={showSaved} autoHideDuration={2500} onClose={() => setShowSaved(false)}>
        <Alert severity="success" variant="filled">
          {savedMessage ?? saveLabel}
        </Alert>
      </Snackbar>
      <Snackbar
        open={Boolean(saveError)}
        autoHideDuration={5000}
        onClose={() => setSaveError(null)}
      >
        <Alert severity="error" variant="filled">
          {saveError}
        </Alert>
      </Snackbar>
    </Box>
  );
}
