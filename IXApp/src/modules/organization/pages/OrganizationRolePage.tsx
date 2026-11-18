import { localizedName } from '@shared/utilities/localizedName';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import React, { useMemo } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { useNotifications } from '@shared/hooks/useNotifications';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface OrganizationRoleRow {
  id: string;
  recordId: number;
  code: string;
  name: string;
  nameAlias?: string | null;
}

const sourceKey = 'organization-roles-simple';
const queryKey = ['simple-list', sourceKey] as const;

export function OrganizationRolePage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<OrganizationRoleRow>[]>(
    () => [
      {
        field: 'code',
        headerName: t('organizationStructure.code'),
        width: 190,
        pinned: 'left',
        editable: true,
      },
      {
        field: 'name',
        valueGetter: ({ row }) => localizedName(row, isRtl),
        headerName: t('organizationStructure.name'),
        minWidth: 300,
        flex: 1,
        editable: true,
      },
      {
        field: 'nameAlias',
        headerName: t('hcmWorkers.fields.nameAlias'),
        minWidth: 260,
        flex: 1,
        editable: true,
      },
    ],
    [t, isRtl]
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<OrganizationRoleRow> = {
    contextLabel: t('organizationStructure.rolesTitle'),
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: t('organizationStructure.code') },
      { field: 'name', label: t('organizationStructure.name') },
      { field: 'nameAlias', label: t('hcmWorkers.fields.nameAlias') },
    ],
    recordTableName: 'OrganizationRole',
    getAuditRecordId: (record) => record.recordId,
    crud: {
      deleteLabel: t('organizationStructure.deactivate'),
      editPermission: 'Organization.Structure.Edit',
      newPermission: 'Organization.Structure.Create',
      deletePermission: 'Organization.Structure.Edit',
      onDelete: async (rows) => {
        await Promise.all(rows.map((row) => api.deactivateRole(row.recordId)));
        await refresh();
        notifySuccess(t('organizationStructure.roleDeactivated'));
      },
    },
    utilities: {
      personalizeLabel: t('organizationStructure.personalize'),
      guideLabel: t('organizationStructure.guide'),
      openWindowLabel: t('organizationStructure.openWindow'),
    },
  };
  return (
    <SimpleListPage
      title={t('organizationStructure.rolesTitle')}
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: sourceKey,
        load: async (signal) =>
          (await api.roles(signal)).map((row) => ({
            ...row,
            id: String(row.id),
            recordId: row.id,
          })),
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'organization.roles.simple-list',
        masterForm: true,
        hideSidebar: false,
        pageSize: 50,
        onNewRow: () => ({
          id: `new-${crypto.randomUUID()}`,
          recordId: 0,
          code: '',
          name: '',
          nameAlias: null,
        }),
        onRowSave: async (values, isNew) => {
          const record = values as OrganizationRoleRow;
          if (!record.code.trim()) throw new Error(t('organizationStructure.codeRequired'));
          if (!record.name.trim()) throw new Error(t('organizationStructure.nameRequired'));
          if (isNew || record.recordId === 0)
            await api.createRole({
              code: record.code.trim(),
              name: record.name.trim(),
              nameAlias: record.nameAlias?.trim() || null,
            });
          else
            await api.updateRole(record.recordId, {
              code: record.code.trim(),
              name: record.name.trim(),
              nameAlias: record.nameAlias?.trim() || null,
            });
          await refresh();
          notifySuccess(t('organizationStructure.roleSaved'));
        },
      }}
    />
  );
}
