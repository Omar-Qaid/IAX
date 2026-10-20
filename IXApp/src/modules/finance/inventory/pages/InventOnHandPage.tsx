import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { inventOnHandApi, type InventOnHandRecord } from '../api/inventOnHandApi';

export function InventOnHandPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const query = useQuery({
    queryKey: ['inventory', 'on-hand'],
    queryFn: ({ signal }) => inventOnHandApi.list(signal),
  });
  const columns = useMemo<ColumnDef<InventOnHandRecord>[]>(
    () => [
      { field: 'itemNumber', headerName: t('inventTrans.fields.itemNumber', 'Item number'), width: 145, pinned: 'left' },
      { field: 'productName', headerName: t('product.fields.productName', 'Product name'), width: 210 },
      { field: 'site', headerName: t('inventTrans.fields.site', 'Site'), width: 95 },
      { field: 'warehouse', headerName: t('inventTrans.fields.warehouse', 'Warehouse'), width: 115 },
      { field: 'physicalInventory', headerName: t('inventOnHand.physicalInventory', 'Physical inventory'), width: 140, type: 'number', align: 'right' },
      { field: 'physicalReserved', headerName: t('inventOnHand.physicalReserved', 'Physical reserved'), width: 140, type: 'number', align: 'right' },
      { field: 'availablePhysical', headerName: t('inventOnHand.availablePhysical', 'Available physical'), width: 140, type: 'number', align: 'right' },
      { field: 'orderedReceipts', headerName: t('inventOnHand.orderedReceipts', 'Ordered receipts'), width: 135, type: 'number', align: 'right' },
      { field: 'orderedIssues', headerName: t('inventOnHand.orderedIssues', 'Ordered issues'), width: 125, type: 'number', align: 'right' },
      { field: 'availableOrdered', headerName: t('inventOnHand.availableOrdered', 'Total available'), width: 125, type: 'number', align: 'right' },
      { field: 'postedValue', headerName: t('inventOnHand.postedValue', 'Posted value'), width: 120, type: 'number', align: 'right' },
      { field: 'physicalValue', headerName: t('inventOnHand.physicalValue', 'Physical value'), width: 120, type: 'number', align: 'right' },
    ],
    [t]
  );
  const config: EnterpriseListConfig<InventOnHandRecord> = {
    readOnly: true,
    showFilterOnLoad: true,
    showSearchCommand: true,
    recordTableName: 'InventSum',
    getAuditRecordId: (row) => row.recId,
    contextLabel: t('inventOnHand.title', 'On-hand inventory'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchFields: [
      { field: 'itemNumber', label: t('inventTrans.fields.itemNumber', 'Item number') },
      { field: 'productName', label: t('product.fields.productName', 'Product name') },
      { field: 'site', label: t('inventTrans.fields.site', 'Site') },
      { field: 'warehouse', label: t('inventTrans.fields.warehouse', 'Warehouse') },
    ],
    locale: currentLanguage.code,
    crud: { editLabel: '', newLabel: '', deleteLabel: '' },
    utilities: {
      personalizeLabel: t('utilities.personalize'),
      guideLabel: t('utilities.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('utilities.openWindow'),
    },
  };
  return (
    <SimpleListPage
      title={t('inventOnHand.title', 'On-hand inventory')}
      enterpriseConfig={config}
      dataSource={{ type: 'controlled', rows: query.data ?? [] }}
      columns={columns}
      loading={query.isLoading}
      error={query.error instanceof Error ? query.error.message : null}
      onRetry={() => query.refetch()}
      dataGridProps={{ storageKey: 'inventory.on-hand.standard-view' }}
    />
  );
}
