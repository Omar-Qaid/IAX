import React, { useState, useEffect, useMemo } from 'react';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { DataGridColumnFactory } from '@shared/components/data-grid/DataGridColumnFactory';
import { customerGroupService } from '../services/customerGroupService';
import { useNotifications } from '@shared/hooks/useNotifications';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import RefreshIcon from '@mui/icons-material/Refresh';
import type { CustomerGroup } from '@mocks/data/customerGroups';

export const CustomerGroupsPage: React.FC = () => {
  const [groups, setGroups] = useState<CustomerGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const { notifySuccess, notifyError } = useNotifications();

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await customerGroupService.getCustomerGroups();
      setGroups(data);
    } catch {
      notifyError('Failed to load customer groups');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const columns = useMemo(
    () => [
      DataGridColumnFactory.createTextColumn('groupId', 'Group ID', { width: 130 }),
      DataGridColumnFactory.createTextColumn('name', 'Group Name', { width: 200 }),
      DataGridColumnFactory.createTextColumn('description', 'Description', { flex: 1, minWidth: 250 }),
      DataGridColumnFactory.createTextColumn('defaultCurrency', 'Default Currency', { width: 140 }),
      DataGridColumnFactory.createTextColumn('paymentTerms', 'Payment Terms', { width: 130 }),
      DataGridColumnFactory.createBooleanColumn('active', 'Active', { width: 100 }),
    ],
    []
  );

  const handleAdd = () => {
    const newGroup: CustomerGroup = {
      id: `cg-${Date.now()}`,
      groupId: `CG-NEW-${groups.length + 1}`,
      name: 'New Customer Group',
      description: 'Enter description',
      defaultCurrency: 'USD',
      paymentTerms: 'Net 30',
      active: true,
    };
    setGroups([newGroup, ...groups]);
    notifySuccess('New temporary row added. Click Save to persist.');
  };

  const handleSave = async () => {
    try {
      setLoading(true);
      await customerGroupService.saveCustomerGroups(groups);
      notifySuccess('Customer groups saved successfully');
      await loadData();
    } catch {
      notifyError('Failed to save customer groups');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SimpleListPage
      title="Customer Groups"
      subtitle="Accounts Receivable Classification & Terms Setup"
      loading={loading}
      actionPane={
        <>
          <ActionPaneGroup label="Maintain">
            <ActionPaneButton label="Add" icon={<AddIcon fontSize="small" />} onClick={handleAdd} />
            <ActionPaneButton label="Save All" icon={<SaveIcon fontSize="small" />} onClick={handleSave} />
          </ActionPaneGroup>
          <ActionPaneGroup label="Page">
            <ActionPaneButton label="Refresh" icon={<RefreshIcon fontSize="small" />} onClick={loadData} />
          </ActionPaneGroup>
        </>
      }
      dataGridProps={{
        rows: groups,
        columns,
        loading,
        pageSize: 10,
      }}
    />
  );
};
