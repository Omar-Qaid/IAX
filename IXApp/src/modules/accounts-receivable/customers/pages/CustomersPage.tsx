import React, { useState, useEffect, useMemo } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { DataGridColumnFactory } from '@shared/components/data-grid/DataGridColumnFactory';
import { FastTabs } from '@shared/components/fast-tabs/FastTabs';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { FormRow, FormColumn } from '@shared/components/forms/FormRow';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { AppSelectField } from '@shared/components/fields/AppSelectField';
import { AppCurrencyField } from '@shared/components/fields/AppNumberField';
import { AppDisplayField } from '@shared/components/fields/AppDisplayField';
import { AppDialog } from '@shared/components/dialogs/AppDialog';
import { DeleteConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { customerService } from '../services/customerService';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useForm } from 'react-hook-form';
import { Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import RefreshIcon from '@mui/icons-material/Refresh';
import type { Customer } from '@mocks/data/customers';

interface CustomerFormData {
  accountNumber: string;
  name: string;
  nameAr?: string;
  customerGroupId: string;
  currencyCode: string;
  email?: string;
  phone?: string;
  status: 'active' | 'onHold' | 'blocked';
  creditLimit?: number;
}

export const CustomersPage: React.FC = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedId, setSelectedId] = useState<string | null>('cust-101');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const { notifySuccess, notifyError } = useNotifications();

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await customerService.getCustomers();
      setCustomers(data);
      if (data.length > 0 && !selectedId) {
        setSelectedId(data[0]!.id);
      }
    } catch {
      notifyError('Failed to load customers');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const selectedCustomer = useMemo(
    () => customers.find((c) => c.id === selectedId) || null,
    [customers, selectedId]
  );

  const { control: createControl, handleSubmit: handleCreateSubmit, reset: resetCreate } = useForm<CustomerFormData>({
    defaultValues: {
      accountNumber: '',
      name: '',
      nameAr: '',
      customerGroupId: 'CG-MAJOR',
      currencyCode: 'USD',
      email: '',
      phone: '',
      status: 'active',
      creditLimit: 50000,
    },
  });

  const columns = useMemo(
    () => [
      DataGridColumnFactory.createTextColumn('accountNumber', 'Account Num', { width: 110 }),
      DataGridColumnFactory.createTextColumn('name', 'Customer Name', { width: 220 }),
      DataGridColumnFactory.createTextColumn('customerGroupId', 'Group', { width: 110 }),
      DataGridColumnFactory.createTextColumn('currencyCode', 'Currency', { width: 90 }),
      DataGridColumnFactory.createStatusColumn('status', 'Status', { width: 100 }),
    ],
    []
  );

  const handleCreateNew = async (formData: CustomerFormData) => {
    try {
      const created = await customerService.createCustomer(formData);
      notifySuccess(`Customer ${created.accountNumber} created successfully`);
      setIsCreateOpen(false);
      resetCreate();
      await loadData();
      setSelectedId(created.id);
    } catch {
      notifyError('Failed to create customer');
    }
  };

  const handleDelete = async () => {
    if (!selectedId) return;
    try {
      await customerService.deleteCustomer(selectedId);
      notifySuccess('Customer deleted');
      setIsDeleteOpen(false);
      setSelectedId(null);
      await loadData();
    } catch {
      notifyError('Failed to delete customer');
    }
  };

  return (
    <ListDetailsPage
      title="Customers"
      subtitle="Accounts Receivable Master Record Management"
      loading={loading}
      selectedId={selectedId}
      actionPane={
        <>
          <ActionPaneGroup label="Maintain">
            <ActionPaneButton
              label="New"
              icon={<AddIcon fontSize="small" />}
              onClick={() => setIsCreateOpen(true)}
              permission="customer.create"
            />
            <ActionPaneButton
              label="Edit"
              icon={<EditIcon fontSize="small" />}
              disabled={!selectedCustomer}
              permission="customer.update"
            />
            <ActionPaneButton
              label="Delete"
              icon={<DeleteIcon fontSize="small" />}
              disabled={!selectedCustomer}
              onClick={() => setIsDeleteOpen(true)}
              permission="customer.delete"
            />
          </ActionPaneGroup>
          <ActionPaneGroup label="Page">
            <ActionPaneButton label="Refresh" icon={<RefreshIcon fontSize="small" />} onClick={loadData} />
          </ActionPaneGroup>
        </>
      }
      dataGridProps={{
        rows: customers,
        columns,
        loading,
        pageSize: 15,
        onSelectionChange: (ids) => {
          if (ids.length > 0) setSelectedId(ids[0]!);
        },
      }}
      detailsPane={
        selectedCustomer ? (
          <FastTabs>
            <FastTab id="general" title="General Information" summary={selectedCustomer.name}>
              <FormRow>
                <FormColumn md={6}>
                  <AppDisplayField label="Account Number" value={selectedCustomer.accountNumber} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Customer Name (EN)" value={selectedCustomer.name} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Customer Name (AR)" value={selectedCustomer.nameAr} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Customer Group" value={selectedCustomer.customerGroupId} />
                </FormColumn>
              </FormRow>
            </FastTab>

            <FastTab id="financial" title="Financial & Credit" summary={`Credit Limit: $${selectedCustomer.creditLimit || 0}`}>
              <FormRow>
                <FormColumn md={6}>
                  <AppDisplayField label="Currency Code" value={selectedCustomer.currencyCode} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Credit Limit" value={`$${selectedCustomer.creditLimit?.toLocaleString() || '0'}`} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Record Status" value={selectedCustomer.status} />
                </FormColumn>
              </FormRow>
            </FastTab>

            <FastTab id="contact" title="Contact Information" summary={selectedCustomer.email}>
              <FormRow>
                <FormColumn md={6}>
                  <AppDisplayField label="Primary Email" value={selectedCustomer.email} />
                </FormColumn>
                <FormColumn md={6}>
                  <AppDisplayField label="Primary Phone" value={selectedCustomer.phone} />
                </FormColumn>
              </FormRow>
            </FastTab>
          </FastTabs>
        ) : null
      }
      dialogs={
        <>
          <AppDialog
            open={isCreateOpen}
            onClose={() => setIsCreateOpen(false)}
            title="Create New Customer Record"
            actions={
              <>
                <Button onClick={() => setIsCreateOpen(false)} size="small">
                  Cancel
                </Button>
                <Button onClick={handleCreateSubmit(handleCreateNew)} variant="contained" size="small">
                  Save Customer
                </Button>
              </>
            }
          >
            <FormRow>
              <FormColumn md={6}>
                <AppTextField name="accountNumber" label="Account Number" control={createControl} required />
              </FormColumn>
              <FormColumn md={6}>
                <AppTextField name="name" label="Customer Name" control={createControl} required />
              </FormColumn>
              <FormColumn md={6}>
                <AppTextField name="nameAr" label="Name (Arabic)" control={createControl} />
              </FormColumn>
              <FormColumn md={6}>
                <AppSelectField
                  name="customerGroupId"
                  label="Customer Group"
                  control={createControl}
                  options={[
                    { value: 'CG-MAJOR', label: 'Major Key Accounts' },
                    { value: 'CG-WHOLESALE', label: 'Wholesale Distributors' },
                    { value: 'CG-GOVT', label: 'Government & Public' },
                  ]}
                />
              </FormColumn>
              <FormColumn md={6}>
                <AppCurrencyField name="creditLimit" label="Credit Limit" control={createControl} />
              </FormColumn>
              <FormColumn md={6}>
                <AppTextField name="email" label="Email Address" control={createControl} />
              </FormColumn>
            </FormRow>
          </AppDialog>

          <DeleteConfirmationDialog
            open={isDeleteOpen}
            onClose={() => setIsDeleteOpen(false)}
            onConfirm={handleDelete}
            message={`Are you sure you want to delete customer ${selectedCustomer?.accountNumber}?`}
          />
        </>
      }
    />
  );
};
