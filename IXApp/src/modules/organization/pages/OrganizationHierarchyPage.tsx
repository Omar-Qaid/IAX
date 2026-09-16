import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { uiDensity } from '@shared/constants/uiDensity';
import { useNotifications } from '@shared/hooks/useNotifications';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface HierarchyRow {
  id: string;
  recordId: number;
  code: string;
  name: string;
  purpose: string;
}

const sourceKey = 'organization-hierarchies';
const queryKey = ['simple-list', sourceKey] as const;

export function OrganizationHierarchyPage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<HierarchyRow>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 170, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Name', minWidth: 260, flex: 1, editable: true },
      { field: 'purpose', headerName: 'Purpose', minWidth: 240, flex: 1, editable: true },
    ],
    []
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<HierarchyRow> = {
    contextLabel: 'Organization hierarchies',
    viewLabel: 'Standard view',
    filterLabel: 'Filter',
    informationLabel: 'Information',
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: 'Code' },
      { field: 'name', label: 'Name' },
      { field: 'purpose', label: 'Purpose' },
    ],
    backCommand: { label: 'Back', onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'OrganizationHierarchy',
    getAuditRecordId: (record) => record.recordId,
    crud: {
      editLabel: 'Edit',
      newLabel: 'New',
      deleteLabel: 'Delete',
      editPermission: 'Organization.Structure.Edit',
      newPermission: 'Organization.Structure.Create',
    },
    utilities: {
      personalizeLabel: 'Personalize',
      guideLabel: 'Guide',
      notificationsLabel: 'Notifications',
      refreshLabel: 'Refresh',
      openWindowLabel: 'Open in new window',
    },
  };

  return (
    <SimpleListPage
      title="Organization hierarchies"
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: sourceKey,
        load: async (signal) =>
          (await api.hierarchies(signal)).map((row) => ({
            ...row,
            id: String(row.id),
            recordId: row.id,
          })),
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'organization.hierarchies.simple-list',
        masterForm: true,
        hideSidebar: false,
        pageSize: 50,
        rowHeight: uiDensity.gridRowHeight,
        headerHeight: uiDensity.gridRowHeight,
        onNewRow: () => ({
          id: `new-${crypto.randomUUID()}`,
          recordId: 0,
          code: '',
          name: '',
          purpose: '',
        }),
        onRowSave: async (values, isNew) => {
          const record = values as HierarchyRow;
          if (!record.code.trim()) throw new Error('Code is required.');
          if (!record.name.trim()) throw new Error('Name is required.');
          if (!record.purpose.trim()) throw new Error('Purpose is required.');
          const payload = {
            code: record.code.trim(),
            name: record.name.trim(),
            purpose: record.purpose.trim(),
          };
          if (isNew || record.recordId === 0) await api.createHierarchy(payload);
          else await api.updateHierarchy(record.recordId, payload);
          await refresh();
          notifySuccess('Organization hierarchy saved.');
        },
      }}
    />
  );
}
