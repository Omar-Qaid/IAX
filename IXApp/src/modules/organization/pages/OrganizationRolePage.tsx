import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { uiDensity } from '@shared/constants/uiDensity';
import { useNotifications } from '@shared/hooks/useNotifications';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface OrganizationRoleRow {
  id: string;
  recordId: number;
  code: string;
  name: string;
}

const sourceKey = 'organization-roles-simple';
const queryKey = ['simple-list', sourceKey] as const;

export function OrganizationRolePage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<OrganizationRoleRow>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 190, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Name', minWidth: 300, flex: 1, editable: true },
    ],
    []
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<OrganizationRoleRow> = {
    contextLabel: 'Organization roles',
    viewLabel: 'Standard view',
    filterLabel: 'Filter',
    informationLabel: 'Information',
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: 'Code' },
      { field: 'name', label: 'Name' },
    ],
    backCommand: { label: 'Back', onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'OrganizationRole',
    getAuditRecordId: (record) => record.recordId,
    crud: {
      editLabel: 'Edit', newLabel: 'New', deleteLabel: 'Deactivate',
      editPermission: 'Organization.Structure.Edit', newPermission: 'Organization.Structure.Create', deletePermission: 'Organization.Structure.Edit',
      onDelete: async (rows) => {
        await Promise.all(rows.map((row) => api.deactivateRole(row.recordId)));
        await refresh();
        notifySuccess('Organization role deactivated.');
      },
    },
    utilities: { personalizeLabel: 'Personalize', guideLabel: 'Guide', notificationsLabel: 'Notifications', refreshLabel: 'Refresh', openWindowLabel: 'Open in new window' },
  };
  return <SimpleListPage title="Organization roles" enterpriseConfig={config} dataSource={{ type: 'remote', key: sourceKey, load: async (signal) => (await api.roles(signal)).map((row) => ({ ...row, id: String(row.id), recordId: row.id })) }} columns={columns} dataGridProps={{ storageKey: 'organization.roles.simple-list', masterForm: true, hideSidebar: false, pageSize: 50, rowHeight: uiDensity.gridRowHeight, headerHeight: uiDensity.gridRowHeight, onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recordId: 0, code: '', name: '' }), onRowSave: async (values, isNew) => { const record = values as OrganizationRoleRow; if (!record.code.trim()) throw new Error('Code is required.'); if (!record.name.trim()) throw new Error('Name is required.'); if (isNew || record.recordId === 0) await api.createRole({ code: record.code.trim(), name: record.name.trim() }); else await api.updateRole(record.recordId, { code: record.code.trim(), name: record.name.trim() }); await refresh(); notifySuccess('Organization role saved.'); } }} />;
}