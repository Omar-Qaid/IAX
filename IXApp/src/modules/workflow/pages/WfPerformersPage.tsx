import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Box, MenuItem, TextField, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { localizedName } from '@shared/utilities/localizedName';
import type { WorkflowMasterRecord } from '../api/workflowMasterApi';
import { wfPerformerApi, wfPerformerTypeApi, type WfPerformerDto } from '../api/wfPerformerApi';
import {
  WfPerformerUsersPanel,
  type WfPerformerUsersPanelHandle,
} from '../components/WfPerformerUsersPanel';
import { WfPerformerRelatedField } from '../components/WfPerformerRelatedField';
import { WfPerformerDatabaseQueryFields } from '../components/WfPerformerDatabaseQueryFields';

type PerformerRecord = WorkflowMasterRecord<WfPerformerDto>;

const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: DetailValue | undefined): string => String(value ?? '');

const emptyPerformer = (): PerformerRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  code: null,
  name: '',
  nameAlias: null,
  description: null,
  sortOrder: 0,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
  performerTypeId: 0,
  relatedField: null,
  isApplicant: false,
  isEmployee: false,
  isManager1: false,
  isManager2: false,
  isManager3: false,
  isManager4: false,
  sqlTable: null,
  sqlField: null,
  sqlWhere: null,
  userIds: [],
  userOptions: [],
});

export function WfPerformersPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const [filterRowsVisible, setFilterRowsVisible] = useState(false);
  const [performerTypeFilter, setPerformerTypeFilter] = useState(0);
  const usersPanelRef = useRef<WfPerformerUsersPanelHandle>(null);
  const performerTypes = useQuery({
    queryKey: ['workflow', 'performer-types'],
    queryFn: ({ signal }) => wfPerformerTypeApi.list(signal),
  });
  const typeOptions = useMemo(
    () =>
      (performerTypes.data ?? []).map((item) => ({
        id: item.recId,
        code: item.code ?? '',
        name: item.name ?? item.code ?? String(item.recId),
      })),
    [performerTypes.data]
  );
  const filterTypeOptions = useMemo(
    () => [{ id: 0, code: '', name: t('wfPerformers.filters.allTypes') }, ...typeOptions],
    [t, typeOptions]
  );
  const config: EnterpriseListDetailsConfig<PerformerRecord> = {
    recordTableName: 'WfPerformers',
    onSearch: () => setFilterRowsVisible((visible) => !visible),
    dataSource: {
      type: 'remote',
      key: `workflow-performers-${performerTypeFilter}`,
      load: async (signal) => {
        const performers = await wfPerformerApi.list(signal);
        return performerTypeFilter === 0
          ? performers
          : performers.filter((performer) => performer.performerTypeId === performerTypeFilter);
      },
      create: async (record) => {
        const saved = await wfPerformerApi.create(record);
        const mergeOptions = (item: PerformerRecord): PerformerRecord => ({
          ...item,
          userIds: item.userIds ?? record.userIds ?? [],
          userOptions:
            item.userOptions?.length || !record.userOptions?.length
              ? (item.userOptions ?? [])
              : record.userOptions,
        });
        return Array.isArray(saved) ? saved.map(mergeOptions) : mergeOptions(saved);
      },
      update: async (record) => {
        const saved = await wfPerformerApi.update(record);
        return {
          ...saved,
          userIds: saved.userIds ?? record.userIds ?? [],
          userOptions:
            saved.userOptions?.length || !record.userOptions?.length
              ? (saved.userOptions ?? [])
              : record.userOptions,
        };
      },
      delete: wfPerformerApi.delete,
    },
    createRecord: emptyPerformer,
    numberSequence: { key: 'WfPerformer', field: 'code' },
    getPrimaryText: (record) => localizedName(record, isRtl),
    getSecondaryText: (record) => record.code ?? '',
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''} ${record.nameAlias ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      performerTypeId: record.performerTypeId,
      relatedField: record.relatedField ?? 0,
      description: record.description ?? '',
      sortOrder: record.sortOrder,
      isApplicant: record.isApplicant,
      isEmployee: record.isEmployee,
      isManager1: record.isManager1,
      isManager2: record.isManager2,
      isManager3: record.isManager3,
      isManager4: record.isManager4,
      sqlTable: record.sqlTable ?? '',
      sqlField: record.sqlField ?? '',
      sqlWhere: record.sqlWhere ?? '',
      isActive: record.isActive,
    }),
    setValues: (record, values) => {
      const performerTypeId = numberValue(values.performerTypeId);
      return {
        ...record,
        performerTypeId,
        relatedField:
          performerTypeId === 2 || performerTypeId === 3
            ? numberValue(values.relatedField) || null
            : null,
        description: textValue(values.description) || null,
        sortOrder: numberValue(values.sortOrder),
        isApplicant: performerTypeId === 1 && Boolean(values.isApplicant),
        isEmployee: performerTypeId === 1 && Boolean(values.isEmployee),
        isManager1: performerTypeId === 1 && Boolean(values.isManager1),
        isManager2: performerTypeId === 1 && Boolean(values.isManager2),
        isManager3: performerTypeId === 1 && Boolean(values.isManager3),
        isManager4: performerTypeId === 1 && Boolean(values.isManager4),
        userIds: performerTypeId === 4 ? (record.userIds ?? []) : [],
        userOptions: performerTypeId === 4 ? (record.userOptions ?? []) : [],
        sqlTable: performerTypeId === 5 ? textValue(values.sqlTable) || null : null,
        sqlField: performerTypeId === 5 ? textValue(values.sqlField) || null : null,
        sqlWhere: performerTypeId === 5 ? textValue(values.sqlWhere) || null : null,
        isActive: Boolean(values.isActive),
      };
    },
    headerFields: [
      {
        id: 'code',
        label: t('wfPerformers.fields.code'),
        width: 170,
        disabled: true,
        getValue: (record) => record.code ?? '',
        setValue: (record, value) => ({ ...record, code: textValue(value) || null }),
      },
      {
        id: 'name',
        label: t('wfPerformers.fields.name'),
        width: 'minmax(300px, 500px)',
        getValue: (record) => record.name ?? '',
        getDisplayValue: (record) => localizedName(record, isRtl),
        setValue: (record, value) => ({ ...record, name: textValue(value) }),
      },
      {
        id: 'nameAlias',
        label: t('wfPerformers.fields.nameAlias'),
        width: 'minmax(260px, 440px)',
        getValue: (record) => record.nameAlias ?? '',
        setValue: (record, value) => ({ ...record, nameAlias: textValue(value) || null }),
      },
    ],
    sections: ({ record, editing, onRecordChange }) => [
      {
        id: 'configuration',
        title: t('wfPerformers.sections.configuration'),
        groups: [
          {
            id: 'general',
            title: t('wfPerformers.groups.general'),
            fields: [
              {
                name: 'performerTypeId',
                label: t('wfPerformers.fields.type'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="performerTypeId"
                    label={t('wfPerformers.fields.type')}
                    value={numberValue(value)}
                    onChange={(next) => onChange(Number(next) || 0)}
                    options={typeOptions}
                    disabled={disabled || performerTypes.isLoading}
                    displayMode="select"
                    searchable
                    lazyLoading
                    required
                  />
                ),
              },
              { name: 'description', label: t('wfPerformers.fields.description') },
              { name: 'sortOrder', label: t('wfPerformers.fields.sortOrder'), type: 'number' },
              { name: 'isActive', label: t('wfPerformers.fields.active'), type: 'boolean' },
            ],
          },
          ...(record.performerTypeId === 1
            ? [
                {
                  id: 'organizational',
                  title: t('wfPerformers.groups.organizational'),
                  fields: [
                    {
                      name: 'isApplicant',
                      label: t('wfPerformers.fields.applicant'),
                      type: 'boolean' as const,
                    },
                    {
                      name: 'isEmployee',
                      label: t('wfPerformers.fields.employee'),
                      type: 'boolean' as const,
                    },
                    {
                      name: 'isManager1',
                      label: t('wfPerformers.fields.manager1'),
                      type: 'boolean' as const,
                    },
                    {
                      name: 'isManager2',
                      label: t('wfPerformers.fields.manager2'),
                      type: 'boolean' as const,
                    },
                    {
                      name: 'isManager3',
                      label: t('wfPerformers.fields.manager3'),
                      type: 'boolean' as const,
                    },
                    {
                      name: 'isManager4',
                      label: t('wfPerformers.fields.manager4'),
                      type: 'boolean' as const,
                    },
                  ],
                },
              ]
            : []),
          ...(record.performerTypeId === 2 || record.performerTypeId === 3
            ? [
                {
                  id: 'formControl',
                  title: t('wfPerformers.groups.formControl'),
                  fields: [
                    {
                      name: 'relatedField',
                      label: t('wfPerformers.fields.relatedField'),
                      renderOwnLabel: true,
                      render: ({
                        value,
                        disabled,
                        onChange,
                      }: {
                        value: DetailValue | undefined;
                        disabled: boolean;
                        onChange: (value: DetailValue) => void;
                      }) => (
                        <WfPerformerRelatedField
                          performerTypeId={record.performerTypeId as 2 | 3}
                          value={numberValue(value)}
                          disabled={disabled}
                          onChange={onChange}
                        />
                      ),
                    },
                  ],
                },
              ]
            : []),
          ...(record.performerTypeId === 5
            ? [
                {
                  id: 'query',
                  title: t('wfPerformers.groups.query'),
                  fields: [
                    {
                      name: 'sqlTable',
                      label: t('wfPerformers.fields.sqlTable'),
                      renderOwnLabel: true,
                      render: ({ disabled }: { disabled: boolean }) => (
                        <WfPerformerDatabaseQueryFields
                          sqlTable={record.sqlTable ?? ''}
                          sqlField={record.sqlField ?? ''}
                          disabled={disabled}
                          onChange={(values) => onRecordChange({ ...record, ...values })}
                        />
                      ),
                    },
                    {
                      name: 'sqlWhere',
                      label: t('wfPerformers.fields.sqlWhere'),
                      multiline: true,
                      rows: 3,
                    },
                  ],
                },
              ]
            : []),
        ],
      },
      ...(record.performerTypeId === 4
        ? [
            {
              id: 'users',
              title: t('wfPerformers.sections.users'),
              minHeight: 260,
              content: (
                <WfPerformerUsersPanel
                  ref={usersPanelRef}
                  userIds={record.userIds ?? []}
                  userOptions={record.userOptions ?? []}
                  editing={editing}
                  onChange={(userIds, userOptions) =>
                    onRecordChange({ ...record, userIds, userOptions })
                  }
                  showFilterRow={filterRowsVisible}
                />
              ),
            },
          ]
        : []),
    ],
    presentation: {
      mode: 'list',
      headerContent: (
        <Box
          sx={{
            p: '8px 10px',
            flexShrink: 0,
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography sx={{ mb: 0.5, fontSize: '0.6875rem', color: 'text.secondary' }}>
            {t('wfPerformers.filters.type')}
          </Typography>
          <TextField
            select
            fullWidth
            size="small"
            value={performerTypeFilter}
            onChange={(event) => setPerformerTypeFilter(Number(event.target.value) || 0)}
            disabled={performerTypes.isLoading}
            sx={{
              '& .MuiInputBase-root': { height: 34, fontSize: '0.8125rem' },
              '& .MuiSelect-select': { py: '7px' },
            }}
          >
            {filterTypeOptions.map((option) => (
              <MenuItem key={option.id} value={option.id} sx={{ fontSize: '0.8125rem' }}>
                {option.name}
              </MenuItem>
            ))}
          </TextField>
        </Box>
      ),
    },
    permissions: {
      view: 'Workflow.Performers.View',
      create: 'Workflow.Performers.Create',
      edit: 'Workflow.Performers.Edit',
      delete: 'Workflow.Performers.Delete',
    },
    prepareSave: async (record) =>
      record.performerTypeId === 4
        ? { ...record, ...(await usersPanelRef.current?.savePendingRow()) }
        : record,
    validate: (record): Record<string, string> => {
      const errors: Record<string, string> = {};
      if (!record.name?.trim())
        errors.name = t('validation.required', { field: t('wfPerformers.fields.name') });
      if (record.performerTypeId <= 0)
        errors.performerTypeId = t('validation.required', { field: t('wfPerformers.fields.type') });
      if ((record.performerTypeId === 2 || record.performerTypeId === 3) && !record.relatedField)
        errors.relatedField = t('validation.required', {
          field:
            record.performerTypeId === 2
              ? t('wfPerformers.fields.requestControl')
              : t('wfPerformers.fields.activityControl'),
        });
      if (record.performerTypeId === 5 && !record.sqlTable?.trim())
        errors.sqlTable = t('validation.required', {
          field: t('wfPerformers.fields.sqlTable'),
        });
      if (record.performerTypeId === 5 && !record.sqlField?.trim())
        errors.sqlField = t('validation.required', {
          field: t('wfPerformers.fields.sqlField'),
        });
      return errors;
    },
    advancedFilter: {
      fieldLabel: t('wfPerformers.fields.name'),
      getValue: (record) => localizedName(record, isRtl),
      matches: (record, value) =>
        localizedName(record, isRtl).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('wfPerformers.title')} config={config} />;
}
