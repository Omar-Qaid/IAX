import React, { useMemo, useState } from 'react';
import { Box, Button, FormControlLabel, Pagination, Stack, Switch, TextField, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { inventOnHandApi, type InventOnHandFilters, type InventOnHandRecord } from '../api/inventOnHandApi';

const emptyFilters: InventOnHandFilters = {
  itemId: '', itemGroupId: '', siteId: '', warehouseId: '', includeZero: false,
};
const pageSize = 50;

export function InventOnHandPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [draft, setDraft] = useState<InventOnHandFilters>(emptyFilters);
  const [filters, setFilters] = useState<InventOnHandFilters>(emptyFilters);
  const [page, setPage] = useState(1);
  const query = useQuery({
    queryKey: ['inventory', 'on-hand', filters, page],
    queryFn: ({ signal }) => inventOnHandApi.page(filters, page, pageSize, signal),
  });
  const columns = useMemo<ColumnDef<InventOnHandRecord>[]>(
    () => [
      { field: 'itemNumber', headerName: t('inventTrans.fields.itemNumber', 'Item number'), width: 145, pinned: 'left' },
      { field: 'productName', headerName: t('product.fields.productName', 'Product name'), width: 210 },
      { field: 'inventoryUnit', headerName: t('inventOnHand.inventoryUnit', 'Inventory unit'), width: 110 },
      { field: 'site', headerName: t('inventTrans.fields.site', 'Site'), width: 95 },
      { field: 'warehouse', headerName: t('inventTrans.fields.warehouse', 'Warehouse'), width: 115 },
      { field: 'inventoryStatusId', headerName: t('inventOnHand.inventoryStatus', 'Inventory status'), width: 140 },
      { field: 'inventoryStatusBlocked', headerName: t('inventOnHand.statusBlocked', 'Status blocked'), width: 120, type: 'boolean' },
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
    showFilterOnLoad: false,
    showSearchCommand: false,
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
      filterBar={
        <Box component="form" onSubmit={(event) => {
          event.preventDefault();
          setFilters({
            itemId: draft.itemId.trim(), itemGroupId: draft.itemGroupId.trim(),
            siteId: draft.siteId.trim(), warehouseId: draft.warehouseId.trim(),
            includeZero: draft.includeZero,
          });
          setPage(1);
        }} sx={{ mx: { xs: 1, sm: 2.5 }, mb: 1 }}>
          <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap" alignItems="center">
            {([
              ['itemId', t('inventTrans.fields.itemNumber', 'Item number')],
              ['itemGroupId', t('inventOnHand.itemGroup', 'Item group')],
              ['siteId', t('inventTrans.fields.site', 'Site')],
              ['warehouseId', t('inventTrans.fields.warehouse', 'Warehouse')],
            ] as const).map(([field, label]) => (
              <TextField key={field} size="small" label={label} value={draft[field]}
                onChange={(event) => setDraft((current) => ({ ...current, [field]: event.target.value }))}
                sx={{ width: { xs: '100%', sm: 155 } }} />
            ))}
            <FormControlLabel control={<Switch checked={draft.includeZero}
              onChange={(event) => setDraft((current) => ({ ...current, includeZero: event.target.checked }))} />}
              label={t('inventOnHand.includeZero', 'Include zero balances')} />
            <Button type="submit" variant="contained">{t('actions.search', 'Search')}</Button>
          </Stack>
        </Box>
      }
      dataSource={{ type: 'controlled', rows: query.data?.rows ?? [] }}
      columns={columns}
      loading={query.isLoading}
      error={query.error instanceof Error ? query.error.message : null}
      onRetry={() => query.refetch()}
      dataGridProps={{ storageKey: 'inventory.on-hand.standard-view' }}
      belowGridContent={
        <Stack direction="row" spacing={2} alignItems="center" justifyContent="space-between" sx={{ py: 1, flexWrap: 'wrap' }}>
          <Typography variant="body2">
            {t('inventOnHand.matchingRows', 'Matching rows')}: {query.data?.totalCount ?? 0}
          </Typography>
          <Pagination page={page} count={Math.max(1, Math.ceil((query.data?.totalCount ?? 0) / pageSize))}
            onChange={(_, nextPage) => setPage(nextPage)} disabled={query.isFetching} size="small" />
        </Stack>
      }
    />
  );
}
