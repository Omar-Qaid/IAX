import React, { useMemo, useState } from 'react';
import { Box, IconButton, Typography, useMediaQuery, useTheme } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PageContainer } from '@shared/components/page/PageContainer';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneRibbonTrigger } from '@shared/components/action-pane/ActionPaneRibbonTrigger';
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
  ribbons,
  commands,
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
  // The pane overlays the content on small screens, so it starts closed there.
  const compact = useMediaQuery(useTheme().breakpoints.down('lg'), { noSsr: true });
  const [navigationOpen, setNavigationOpen] = useState(defaultNavigationOpen && !compact);
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
  const hasRail = Boolean(advancedFilter || relatedInformation);

  const header = (
    <Box sx={{ pt: '3px' }}>
      {(subtitle || viewLabel) && (
        <Typography color="primary" variant="body2" sx={{ mb: 1 }}>
          {[subtitle, viewLabel].filter(Boolean).join(' | ')}
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
        {status && <Box sx={{ fontSize: d365.fontSize, flexShrink: 0, paddingInlineEnd: 1 }}>{status}</Box>}
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
    <PageContainer
      sx={{
        gap: 0,
        height: '100%',
        minHeight: 0,
        display: 'flex',
        flexDirection: 'column',
        // As list-details: the right rail spans the whole page (beside the action pane).
        position: 'relative',
        overflow: 'hidden',
        paddingInlineEnd: hasRail ? { lg: `${d365.utilityRailWidth}px` } : undefined,
        bgcolor: d365.canvas,
        fontFamily: d365.fontFamily,
        color: d365.text,
      }}
    >
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
          size="small"
          sx={{
            width: 31,
            height: 31,
            mx: 0.5,
            bgcolor: navigationOpen ? d365.primary : 'transparent',
            color: navigationOpen ? '#fff' : d365.primary,
            borderRadius: d365.radius,
            transition: 'background-color 140ms ease, color 140ms ease',
            '&:hover': { bgcolor: navigationOpen ? '#2559d6' : 'action.hover' },
          }}
        >
          <MenuIcon sx={{ fontSize: 17 }} />
        </IconButton>
        {actionPane}
        {ribbons && ribbons.length > 0 && (
          <ActionPaneGroup>
            {ribbons.map((ribbon) => (
              <ActionPaneRibbonTrigger
                key={ribbon.id}
                id={ribbon.id}
                label={ribbon.label}
                groups={ribbon.groups}
                disabled={editing || ribbon.disabled}
                persistenceKey={ribbon.persistenceKey}
              />
            ))}
          </ActionPaneGroup>
        )}
        {commands && commands.length > 0 && (
          <ActionPaneGroup>
            {commands.map((command) => (
              <ActionPaneButton
                key={command.id}
                actionId={command.id}
                label={command.label}
                icon={command.icon}
                disabled={editing || command.disabled}
                onClick={command.onClick}
              />
            ))}
          </ActionPaneGroup>
        )}
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
      <Box
        sx={{
          display: 'flex',
          flex: 1,
          minHeight: 0,
          position: 'relative',
        }}
      >
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
      </Box>
      {hasRail && (
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
    </PageContainer>
  );
}
