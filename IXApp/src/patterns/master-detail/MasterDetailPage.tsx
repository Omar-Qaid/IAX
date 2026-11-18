import React, { useMemo, useState } from 'react';
import { Box, IconButton, Typography } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PageContainer } from '@shared/components/page/PageContainer';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { RecordAttachmentsButton } from '@shared/components/documents';
import { RightUtilityRail } from '@shared/components/page/RightUtilityRail';
import { RelatedInformationPanel } from '@shared/components/page/RelatedInformationPanel';
import {
  EnterpriseFilterPanel,
  createEnterpriseFilterCondition,
  matchesEnterpriseFilter,
  type EnterpriseFilterCondition,
} from '@shared/components/data-grid/EnterpriseFilterPanel';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { RecordList } from '@patterns/list-details/ListDetailsPage';
import { ListDetailsLayout } from '@patterns/list-details/ListDetailsLayout';
import type { DetailSectionConfig } from '@patterns/list-details/types';
import type { MasterDetailPageProps, MasterDetailRecord } from './types';

const NO_VALUES = {};
const sidePanelSx = {
  position: { xs: 'absolute', lg: 'static' },
  insetInlineEnd: 0,
  top: 0,
  bottom: 0,
  zIndex: 4,
  height: '100%',
  minHeight: 0,
  display: 'flex',
} as const;

/**
 * D365 details page for a master record with lines (sales order, purchase order,
 * transfer order, journals). Built from the sales order page and composed from
 * the same shared parts as list-details, so both stay in step:
 * action pane (+ Options menu with record info/audit, attachments, utilities),
 * resizable record navigation pane, breadcrumb/title/status, tabs, field-config
 * sections and free content, advanced filter and related information panels
 * on the right rail.
 *
 * The page owns data, selection (usually the route), edit state and saving.
 */
export function MasterDetailPage<T extends MasterDetailRecord = MasterDetailRecord>({
  records,
  selectedId,
  onSelect,
  title,
  subtitle,
  viewLabel,
  status,
  filterLabel,
  emptyLabel,
  actionPane,
  children,
  backAction,
  endActions,
  navigationLabel,
  tabs,
  editing = false,
  loading = false,
  error = null,
  onRetry,
  defaultNavigationOpen = true,
  navigationWidthStorageKey,
  matchesSearch,
  advancedFilter,
  relatedInformation,
  recordTableName,
  getAuditRecordId,
  attachments,
  showOptionsMenu = true,
  utilities,
  onRefresh,
  sections,
  values = NO_VALUES,
  onFieldChange,
  onFieldEdit,
  canEditField,
}: MasterDetailPageProps<T>): React.ReactElement {
  const { t } = useAppTranslation();
  const [navigationOpen, setNavigationOpen] = useState(defaultNavigationOpen);
  const [query, setQuery] = useState('');
  const [filterOpen, setFilterOpen] = useState(false);
  const [informationOpen, setInformationOpen] = useState(false);
  const [draftConditions, setDraftConditions] = useState<EnterpriseFilterCondition[]>([]);
  const [conditions, setConditions] = useState<EnterpriseFilterCondition[]>([]);

  const visible = useMemo(() => {
    const term = query.trim().toLocaleLowerCase();
    return records.filter((record) => {
      const quick =
        !term ||
        (matchesSearch
          ? matchesSearch(record, term)
          : `${record.title} ${record.subtitle ?? ''} ${record.description ?? ''}`.toLocaleLowerCase().includes(term));
      const advanced = conditions.every((condition) => {
        const field = advancedFilter?.fields.find((candidate) => candidate.id === condition.field);
        return !field || matchesEnterpriseFilter(field.getValue(record), condition);
      });
      return quick && advanced;
    });
  }, [records, query, matchesSearch, conditions, advancedFilter]);
  const selected = records.find((record) => record.id === selectedId) ?? null;

  const header = (
    <Box sx={{ pt: '3px' }}>
      {(subtitle || viewLabel) && (
        <Typography sx={{ height: 20, fontSize: d365.fontSize, lineHeight: '18px', color: 'primary.main' }}>
          {subtitle}
          {viewLabel && (
            <Box
              component="span"
              sx={{ color: d365.text, borderInlineStart: `1px solid ${d365.border}`, marginInlineStart: 1, paddingInlineStart: 1 }}
            >
              {viewLabel}
            </Box>
          )}
        </Typography>
      )}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2, mb: 1 }}>
        <Typography
          component="h1"
          noWrap
          sx={{ fontSize: { xs: 18, sm: d365.titleFontSize }, lineHeight: 1.2, fontWeight: 600, color: d365.text }}
        >
          {title}
        </Typography>
        {status && <Box sx={{ fontSize: d365.fontSize, flexShrink: 0 }}>{status}</Box>}
      </Box>
      {!loading && !error && tabs}
    </Box>
  );

  // Field-config sections first, then the page's own content (lines etc.).
  const bodySections: DetailSectionConfig[] = loading
    ? [{ id: 'master-detail-loading', title: '', hideHeader: true, content: <LoadingState /> }]
    : error
      ? [{ id: 'master-detail-error', title: '', hideHeader: true, content: <ErrorState message={error} onRetry={onRetry} /> }]
      : [
          ...(sections ?? []).map((section) => ({ ...section, editing: section.editing ?? editing })),
          ...(children ? [{ id: 'master-detail-content', title: '', hideHeader: true, content: children }] : []),
        ];

  const listPane = (
    <>
      <RecordList<T>
        records={visible}
        selectedId={selectedId ?? null}
        editing={editing}
        query={query}
        filterVisible
        filterLabel={filterLabel}
        getPrimaryText={(record) => record.title}
        getSecondaryText={(record) => record.subtitle ?? ''}
        onQueryChange={setQuery}
        onSelect={(record) => onSelect(record.id)}
      />
      {!visible.length && <Typography sx={{ p: 1.5, fontSize: d365.fontSize }}>{emptyLabel}</Typography>}
    </>
  );

  return (
    <PageContainer sx={{ gap: 0, height: '100%', minHeight: 0, fontFamily: d365.fontFamily, display: 'flex', flexDirection: 'column' }}>
      <ActionPane
        variant="flat"
        endActions={
          <>
            {endActions}
            <EnterpriseCommandUtilities
              personalizeLabel={t('utilities.personalize')}
              guideLabel={t('utilities.guide')}
              notificationsLabel={t('common.notifications')}
              refreshLabel={t('actions.refresh')}
              openWindowLabel={t('utilities.openWindow')}
              notificationCount={0}
              disabled={editing}
              {...utilities}
              attachmentAction={
                attachments ? (
                  <RecordAttachmentsButton refTableId={attachments.refTableId} refRecId={attachments.refRecId} disabled={editing} />
                ) : (
                  false
                )
              }
              onRefresh={onRefresh}
            />
          </>
        }
      >
        {backAction}
        <IconButton
          aria-label={navigationLabel ?? t('actions.toggleList')}
          aria-pressed={navigationOpen}
          onClick={() => setNavigationOpen((open) => !open)}
          sx={{
            bgcolor: navigationOpen ? 'primary.main' : undefined,
            color: navigationOpen ? 'primary.contrastText' : 'primary.main',
            borderRadius: `${d365.radius}px`,
            width: 31,
            height: 31,
            mx: 0.5,
            '&:hover': { bgcolor: navigationOpen ? 'primary.dark' : 'action.hover' },
          }}
        >
          <MenuIcon sx={{ fontSize: 17 }} />
        </IconButton>
        {actionPane}
        {showOptionsMenu && (
          <OptionsMenu<T>
            record={selected}
            tableName={recordTableName ?? title}
            getRecordId={getAuditRecordId}
            title={title}
            disabled={editing}
          />
        )}
      </ActionPane>
      <Box sx={{ display: 'flex', flex: 1, minHeight: 0, position: 'relative' }}>
        <Box sx={{ flex: 1, minWidth: 0, minHeight: 0 }}>
          <ListDetailsLayout
            listPane={listPane}
            header={header}
            sections={bodySections}
            values={values}
            editing={editing}
            yesLabel={t('common.yes', 'Yes')}
            noLabel={t('common.no', 'No')}
            onChange={(name, value) => onFieldChange?.(name, value)}
            onFieldEdit={onFieldEdit}
            canEditField={canEditField}
            listWidth={d365.listWidth}
            listPaneVisible={navigationOpen}
            onListPaneClose={() => setNavigationOpen(false)}
            listWidthStorageKey={navigationWidthStorageKey}
          />
        </Box>
        {filterOpen && advancedFilter && (
          <Box sx={sidePanelSx}>
            <EnterpriseFilterPanel
              title={advancedFilter.title ?? t('filters.title')}
              addLabel={t('actions.add')}
              fieldOptions={advancedFilter.fields.map(({ id, label }) => ({ value: id, label }))}
              conditions={draftConditions}
              operatorOptions={[
                { value: 'contains', label: t('filters.contains') },
                { value: 'equals', label: t('filters.equals') },
                { value: 'startsWith', label: t('filters.startsWith') },
                { value: 'endsWith', label: t('filters.endsWith') },
                { value: 'notEquals', label: t('filters.notEquals') },
                { value: 'doesNotContain', label: t('filters.doesNotContain') },
              ]}
              applyLabel={t('actions.apply')}
              resetLabel={t('actions.reset')}
              onConditionsChange={setDraftConditions}
              onApply={() => setConditions(draftConditions)}
              onReset={() => {
                setDraftConditions([]);
                setConditions([]);
              }}
            />
          </Box>
        )}
        {informationOpen && relatedInformation && (
          <Box sx={sidePanelSx}>
            <RelatedInformationPanel
              title={relatedInformation.title ?? t('relatedInformation.title')}
              sections={relatedInformation.sections}
            />
          </Box>
        )}
        {(advancedFilter || relatedInformation) && (
          <RightUtilityRail
            filterLabel={t('actions.filter')}
            informationLabel={t('common.information')}
            filterActive={filterOpen}
            informationActive={informationOpen}
            onFilter={() => {
              // Start with one empty condition on the first field (created once, stable ID).
              if (!filterOpen && advancedFilter && draftConditions.length === 0)
                setDraftConditions([createEnterpriseFilterCondition(advancedFilter.fields[0]?.id)]);
              setFilterOpen((open) => !open);
            }}
            onInformation={() => setInformationOpen((open) => !open)}
            showInformation={Boolean(relatedInformation)}
            disabled={editing}
          />
        )}
      </Box>
    </PageContainer>
  );
}
