import React, { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { localizedName } from '@shared/utilities/localizedName';
import { uiDensity } from '@shared/constants/uiDensity';
import { LookupField } from '@shared/components/lookups/LookupField';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface HierarchyRecord {
  id: string;
  recordId: number;
  code: string;
  name: string;
  nameAlias?: string | null;
  purpose: string;
  rootOrganizationUnitId: number;
  validFrom: string;
  validTo: string | null;
}

export function OrganizationHierarchyPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const navigate = useNavigate();
  const [creating, setCreating] = useState(false);
  const asOf = new Date().toISOString().slice(0, 10);
  const sourceKey = `organization-hierarchies-simple-${company}`;
  const columns = useMemo<ColumnDef<HierarchyRecord>[]>(
    () => [
      { field: 'code', headerName: t('organizationStructure.code'), width: 170, editable: true },
      {
        field: 'name',
        headerName: t('organizationStructure.name'),
        minWidth: 220,
        flex: 1,
        editable: true,
        valueGetter: ({ row }) => localizedName(row, isRtl),
      },
      {
        field: 'nameAlias',
        headerName: t('hcmWorkers.fields.nameAlias'),
        minWidth: 220,
        flex: 1,
        editable: true,
      },
      {
        field: 'purpose',
        headerName: t('organizationStructure.purpose'),
        width: 180,
        editable: true,
      },
      {
        field: 'rootOrganizationUnitId',
        headerName: t('organizationStructure.rootUnit'),
        width: 240,
        editable: creating,
        renderEditCell: ({ value, onChange, disabled }) => (
          <LookupField
            name="rootOrganizationUnitId"
            label={t('organizationStructure.rootUnit')}
            value={Number(value) || undefined}
            onChange={(id) => onChange(Number(id) || 0)}
            disabled={disabled}
            required
            searchable
            lazyLoading
            sideMode="server"
            pageSize={20}
            searchDebounceMs={250}
            queryKey={['organization-hierarchy-root-lookup', company, asOf]}
            fetchPage={async ({ pageNumber, pageSize, search }) => {
              const units = await queryClient.fetchQuery({
                queryKey: ['organization-structure', company, 'units', asOf],
                queryFn: ({ signal }) => api.units(asOf, signal),
                staleTime: 60_000,
              });
              const term = search.trim().toLocaleLowerCase();
              const filtered = units.filter((unit) =>
                `${unit.code} ${unit.name} ${unit.nameAlias ?? ''}`.toLocaleLowerCase().includes(term)
              );
              const start = (pageNumber - 1) * pageSize;
              return { data: filtered.slice(start, start + pageSize).map((unit) => ({ id: unit.id, code: unit.code, name: unit.name, nameAlias: unit.nameAlias })), pageNumber,
                totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)), totalRecords: filtered.length };
            }}
          />
        ),
      },
      {
        field: 'validFrom',
        headerName: t('organizationStructure.validFrom'),
        width: 150,
        type: 'date',
        editable: creating,
      },
      {
        field: 'validTo',
        headerName: t('organizationStructure.validTo'),
        width: 150,
        type: 'date',
        editable: creating,
      },
    ],
    [t, isRtl, creating, company, asOf]
  );
  const config: EnterpriseListConfig<HierarchyRecord> = {
    contextLabel: t('organizationStructure.hierarchiesTitle'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: t('organizationStructure.code') },
      { field: 'name', label: t('organizationStructure.name') },
      { field: 'nameAlias', label: t('hcmWorkers.fields.nameAlias') },
      { field: 'purpose', label: t('organizationStructure.purpose') },
    ],
    backCommand: {
      label: t('actions.back'),
      onClick: () => navigate('/organization-administration/organization-hierarchies'),
    },
    showSearchCommand: true,
    recordTableName: 'OrganizationHierarchy',
    getAuditRecordId: (row) => row.recordId,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      editPermission: 'Organization.Structure.Edit',
      newPermission: 'Organization.Structure.Create',
    },
    utilities: {
      personalizeLabel: t('organizationStructure.personalize'),
      guideLabel: t('organizationStructure.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('organizationStructure.openWindow'),
    },
  };
  return (
    <SimpleListPage<HierarchyRecord>
      key={company}
      title={t('organizationStructure.hierarchiesTitle')}
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: sourceKey,
        load: async (signal) =>
          (await api.hierarchies(signal)).map((row) => ({
            ...row,
            id: String(row.id),
            recordId: row.id,
            rootOrganizationUnitId: 0,
            validFrom: '',
            validTo: null,
          })),
      }}
      columns={columns.filter(
        (column) =>
          creating ||
          !['rootOrganizationUnitId', 'validFrom', 'validTo'].includes(String(column.field))
      )}
      dataGridProps={{
        storageKey: 'organization.hierarchies.simple-list',
        masterForm: true,
        hideSidebar: false,
        pageSize: 50,
        rowHeight: uiDensity.gridRowHeight,
        headerHeight: uiDensity.gridRowHeight,
        onRowDoubleClick: (row) =>
          navigate(
            '/organization-administration/organization-hierarchies/' + row.recordId + '/nodes'
          ),
        onEditingChange: (editing) => {
          if (!editing) setCreating(false);
        },
        onNewRow: () => {
          setCreating(true);
          return {
            id: `new-${crypto.randomUUID()}`,
            recordId: 0,
            code: '',
            name: '',
            nameAlias: null,
            purpose: '',
            rootOrganizationUnitId: 0,
            validFrom: asOf,
            validTo: null,
          };
        },
        onRowSave: async (values, isNew) => {
          const row = values as HierarchyRecord;
          if (!row.code.trim()) throw new Error(t('organizationStructure.codeRequired'));
          if (!row.name.trim()) throw new Error(t('organizationStructure.nameRequired'));
          if (!row.purpose.trim()) throw new Error(t('organizationStructure.purposeRequired'));
          if ((row.nameAlias?.length ?? 0) > 200) throw new Error(t('organizationUnits.nameError'));
          const payload = {
            code: row.code.trim(),
            name: row.name.trim(),
            nameAlias: row.nameAlias?.trim() || null,
            purpose: row.purpose.trim(),
          };
          if (isNew || row.recordId === 0) {
            if (Number(row.rootOrganizationUnitId) <= 0)
              throw new Error(t('organizationStructure.rootUnitRequired'));
            if (!row.validFrom) throw new Error(t('organizationStructure.validFromRequired'));
            if (row.validTo && row.validTo <= row.validFrom)
              throw new Error(t('organizationStructure.dateError'));
            await api.createHierarchyWithRootNode({
              ...payload,
              organizationUnitId: Number(row.rootOrganizationUnitId),
              validFrom: row.validFrom,
              validTo: row.validTo || null,
            });
          } else await api.updateHierarchy(row.recordId, payload);
          await queryClient.invalidateQueries({ queryKey: ['simple-list', sourceKey] });
          await queryClient.invalidateQueries({
            queryKey: ['organization-structure', company, 'hierarchies'],
          });
        },
      }}
    />
  );
}
