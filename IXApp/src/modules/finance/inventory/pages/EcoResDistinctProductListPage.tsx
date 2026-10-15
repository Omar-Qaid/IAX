import React, { useMemo, useState } from 'react';
import { Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { localizedName } from '@shared/utilities/localizedName';
import { ecoResProductApi, type EcoResProductRecord } from '../api/ecoResProductApi';
import { EcoResProductCreateDrawer } from '../components/EcoResProductCreateDrawer';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';

const queryKey = ['inventory', 'shared-products'] as const;
export function EcoResDistinctProductListPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation(); const navigate = useNavigate(); const [creating, setCreating] = useState(false);
  const query = useQuery({ queryKey, queryFn: ({ signal }) => ecoResProductApi.list(signal) });
  const columns = useMemo<ColumnDef<EcoResProductRecord>[]>(() => [
    { field: 'productNumber', headerName: t('product.fields.productNumber', 'Product number'), width: 175, pinned: 'left', renderCell: ({ row }) => <Typography component="button" onClick={() => navigate(INVENTORY_ROUTE_PATHS.distinctProductDetails(row.id))} sx={{ border: 0, p: 0, bgcolor: 'transparent', color: 'primary.main', cursor: 'pointer', font: 'inherit' }}>{row.productNumber}</Typography> },
    { field: 'name', headerName: t('product.fields.productName', 'Product name'), minWidth: 240, flex: 1, valueGetter: ({ row }) => localizedName(row, isRtl) },
    { field: 'nameAlias', headerName: t('product.fields.arabicName', 'Arabic name'), minWidth: 240, flex: 1 },
    { field: 'productType', headerName: t('product.fields.productType', 'Product type'), width: 140 },
    { field: 'productSubtype', headerName: t('product.fields.productSubtype', 'Product subtype'), width: 150 },
  ], [isRtl, navigate, t]);
  const config: EnterpriseListConfig<EcoResProductRecord> = { contextLabel: t('product.title', 'Products'), viewLabel: t('common.standardView'), filterLabel: t('actions.filter'), informationLabel: t('common.information'), searchMode: 'quick', searchFields: [{ field: 'productNumber', label: t('product.fields.productNumber') }, { field: 'name', label: t('product.fields.productName') }, { field: 'nameAlias', label: t('product.fields.arabicName') }], showSearchCommand: true, recordTableName: 'EcoResProduct', commands: [{ id: 'release-products', label: t('product.actions.release', 'Release products'), permission: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, onClick: () => navigate(INVENTORY_ROUTE_PATHS.PRODUCT_RELEASE) }], crud: { editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'), editPermission: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, newPermission: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, deletePermission: PERMISSIONS.INVENTORY_TRANSACTION_DELETE, onEdit: (row) => navigate(INVENTORY_ROUTE_PATHS.distinctProductDetails(row.id)), onNew: () => setCreating(true), onDelete: async (rows) => { await Promise.all(rows.map((row) => ecoResProductApi.delete(row.id))); await queryClient.invalidateQueries({ queryKey }); } }, utilities: { personalizeLabel: t('common.personalize'), guideLabel: t('common.guide'), notificationsLabel: t('common.notifications'), refreshLabel: t('actions.refresh'), openWindowLabel: t('common.openWindow') } };
  return <SimpleListPage title={t('product.title', 'Products')} enterpriseConfig={config} dataSource={{ type: 'controlled', rows: query.data ?? [], loading: query.isLoading, error: query.error instanceof Error ? query.error.message : null, refresh: () => query.refetch() }} columns={columns} dataGridProps={{ storageKey: 'inventory.shared-products.standard-view', onRowDoubleClick: (row) => navigate(INVENTORY_ROUTE_PATHS.distinctProductDetails(row.id)) }} dialogs={<EcoResProductCreateDrawer open={creating} onClose={() => setCreating(false)} onCreated={async () => { setCreating(false); await queryClient.invalidateQueries({ queryKey }); }} />} />;
}
