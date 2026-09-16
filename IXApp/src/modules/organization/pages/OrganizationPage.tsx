import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { uiDensity } from '@shared/constants/uiDensity';
import { useNotifications } from '@shared/hooks/useNotifications';
import { organizationStructureApi as api } from '../api/organizationStructureApi';

interface OrganizationRow {
  id: string;
  recordId: number;
  code: string;
  name: string;
  type: number;
}

const sourceKey = 'organization-units-simple';
const queryKey = ['simple-list', sourceKey] as const;
const today = () => new Date().toISOString().slice(0, 10);
const unitTypes = [
  'Area',
  'Region',
  'Supervisor zone',
  'Showroom',
  'Company',
  'Business unit',
  'Branch',
  'Department',
  'Warehouse',
];

export function OrganizationPage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<OrganizationRow>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 170, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Name', minWidth: 260, flex: 1, editable: true },
      {
        field: 'type',
        headerName: 'Unit type',
        type: 'singleSelect',
        width: 190,
        editable: true,
        valueOptions: unitTypes.map((label, index) => ({ value: index + 1, label })),
      },
    ],
    []
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<OrganizationRow> = {
    contextLabel: 'Organization units',
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
    recordTableName: 'OrganizationUnit',
    getAuditRecordId: (record) => record.recordId,
    crud: {
      editLabel: 'Edit',
      newLabel: 'New',
      deleteLabel: 'Close',
      editPermission: 'Organization.Structure.Edit',
      newPermission: 'Organization.Structure.Create',
      deletePermission: 'Organization.Structure.Edit',
      onDelete: async (rows) => {
        await Promise.all(rows.map((row) => api.close(row.recordId, today())));
        await refresh();
        notifySuccess('Organization unit closed.');
      },
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
      title="Organization units"
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: sourceKey,
        load: async (signal) =>
          (await api.units(today(), signal)).map((row) => ({
            ...row,
            id: String(row.id),
            recordId: row.id,
          })),
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'organization.units.simple-list',
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
          type: 1,
        }),
        onRowSave: async (values, isNew) => {
          const record = values as OrganizationRow;
          if (!record.code.trim()) throw new Error('Code is required.');
          if (!record.name.trim()) throw new Error('Name is required.');
          if (isNew || record.recordId === 0) {
            await api.create({
              code: record.code.trim(),
              name: record.name.trim(),
              nameAR: null,
              type: Number(record.type),
              validFrom: today(),
              validTo: null,
            });
          } else {
            await api.update(record.recordId, {
              code: record.code.trim(),
              name: record.name.trim(),
              type: Number(record.type),
            });
          }
          await refresh();
          notifySuccess('Organization unit saved.');
        },
      }}
    />
  );
}
