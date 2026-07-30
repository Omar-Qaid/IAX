import React, { useState, useEffect, useMemo } from 'react';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { DataGridColumnFactory } from '@shared/components/data-grid/DataGridColumnFactory';
import { currencyService } from '../services/currencyService';
import { useNotifications } from '@shared/hooks/useNotifications';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import RefreshIcon from '@mui/icons-material/Refresh';
import DeleteIcon from '@mui/icons-material/Delete';
import type { Currency } from '@mocks/data/currencies';

export const CurrenciesPage: React.FC = () => {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const { notifySuccess, notifyError } = useNotifications();

  const loadData = async () => {
    try {
      setLoading(true);
      const data = await currencyService.getCurrencies();
      setCurrencies(data);
    } catch {
      notifyError('Failed to load currencies');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const columns = useMemo(
    () => [
      DataGridColumnFactory.createTextColumn('currencyCode', 'Currency Code', { width: 140 }),
      DataGridColumnFactory.createTextColumn('name', 'Currency Name', { flex: 1, minWidth: 200 }),
      DataGridColumnFactory.createTextColumn('symbol', 'Symbol', { width: 100, align: 'center', headerAlign: 'center' }),
      DataGridColumnFactory.createNumberColumn('numberOfDecimals', 'Decimals', { width: 110 }),
      DataGridColumnFactory.createBooleanColumn('active', 'Active Status', { width: 120 }),
    ],
    []
  );

  const handleAdd = () => {
    const newCurr: Currency = {
      id: `curr-${Date.now()}`,
      currencyCode: 'NEW',
      name: 'New Foreign Currency',
      symbol: '$',
      numberOfDecimals: 2,
      active: true,
    };
    setCurrencies([newCurr, ...currencies]);
    notifySuccess('Added temporary currency row. Click Save All to persist.');
  };

  const handleSave = async () => {
    try {
      setLoading(true);
      await currencyService.saveCurrencies(currencies);
      notifySuccess('Currencies saved successfully');
      await loadData();
    } catch {
      notifyError('Failed to save currencies');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedId) return;
    try {
      await currencyService.deleteCurrency(selectedId);
      notifySuccess('Currency removed');
      setSelectedId(null);
      await loadData();
    } catch {
      notifyError('Failed to delete currency');
    }
  };

  return (
    <SimpleListPage
      title="Currencies"
      subtitle="Foundation System Currency & Exchange Rates Reference Data"
      loading={loading}
      actionPane={
        <>
          <ActionPaneGroup label="Maintain">
            <ActionPaneButton label="Add Currency" icon={<AddIcon fontSize="small" />} onClick={handleAdd} />
            <ActionPaneButton label="Save All" icon={<SaveIcon fontSize="small" />} onClick={handleSave} />
            <ActionPaneButton
              label="Delete"
              icon={<DeleteIcon fontSize="small" />}
              disabled={!selectedId}
              onClick={handleDelete}
            />
          </ActionPaneGroup>
          <ActionPaneGroup label="Page">
            <ActionPaneButton label="Refresh" icon={<RefreshIcon fontSize="small" />} onClick={loadData} />
          </ActionPaneGroup>
        </>
      }
      dataGridProps={{
        rows: currencies,
        columns,
        loading,
        pageSize: 10,
        onSelectionChange: (ids) => {
          if (ids.length > 0) setSelectedId(ids[0]!);
        },
      }}
    />
  );
};
