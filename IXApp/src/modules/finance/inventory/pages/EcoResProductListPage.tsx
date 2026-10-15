import React, { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import { releasedProductApi, type ReleasedProductRecord } from '../api/releasedProductApi';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';
import { ReleasedProductCreateDrawer } from '../components/ReleasedProductCreateDrawer';
import { useCompanyStore } from '@core/company/useCompanyStore';

export function EcoResProductListPage(): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const navigate = useNavigate();
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const [creating, setCreating] = useState(false);
  const query = useQuery({
    queryKey: ['inventory', 'released-products', currentCompany],
    queryFn: ({ signal }) => releasedProductApi.list(signal),
  });

  const columns = useMemo<ColumnDef<ReleasedProductRecord>[]>(
    () => [
      {
        field: 'itemNumber',
        headerName: 'releasedProduct.fields.itemNumber',
        width: 135,
        pinned: 'left',
        renderCell: ({ row }) => (
          <Typography
            component="button"
            type="button"
            onClick={() => navigate(INVENTORY_ROUTE_PATHS.releasedProductDetails(row.id))}
            sx={{
              border: 0,
              padding: 0,
              background: 'transparent',
              color: 'primary.main',
              font: 'inherit',
              cursor: 'pointer',
              '&:hover': { textDecoration: 'underline' },
            }}
          >
            {row.itemNumber}
          </Typography>
        ),
      },
      {
        field: 'name',
        headerName: 'releasedProduct.fields.productName',
        width: 185,
        valueGetter: ({ row }) => localizedName({ name: row.name, nameAlias: row.nameAliasLocalized }, isRtl),
      },
      {
        field: 'nameAliasLocalized',
        headerName: 'releasedProduct.fields.arabicName',
        width: 185,
        align: 'right',
        headerAlign: 'right',
      },
      { field: 'productType', headerName: 'releasedProduct.fields.productType', width: 120 },
      { field: 'productSubtype', headerName: 'releasedProduct.fields.productSubtype', width: 150 },
      {
        field: 'salesPrice',
        headerName: 'releasedProduct.fields.salesPrice',
        width: 105,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
        renderCell: ({ row }) => row.salesPrice.toLocaleString(currentLanguage.code, {
          minimumFractionDigits: 2, maximumFractionDigits: 2,
        }),
      },
      {
        field: 'purchasePrice',
        headerName: 'releasedProduct.fields.purchasePrice',
        width: 115,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
        renderCell: ({ row }) => row.purchasePrice.toLocaleString(currentLanguage.code, {
          minimumFractionDigits: 2, maximumFractionDigits: 2,
        }),
      },
      {
        field: 'costPrice',
        headerName: 'releasedProduct.fields.costPrice',
        width: 105,
        type: 'number',
        align: 'right',
        headerAlign: 'right',
        renderCell: ({ row }) => row.costPrice.toLocaleString(currentLanguage.code, {
          minimumFractionDigits: 2, maximumFractionDigits: 2,
        }),
      },
    ],
    [currentLanguage.code, isRtl]
  );

  const config: EnterpriseListConfig<ReleasedProductRecord> = {
    contextLabel: t('releasedProduct.title', 'Released products'), viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'), informationLabel: t('common.information'), searchMode: 'quick',
    searchFields: [{ field: 'itemNumber', label: t('releasedProduct.fields.itemNumber') }, { field: 'name', label: t('releasedProduct.fields.productName') }, { field: 'nameAliasLocalized', label: t('releasedProduct.fields.arabicName') }],
    showSearchCommand: true, recordTableName: 'InventTable',
    crud: { editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'), editPermission: PERMISSIONS.INVENTORY_TRANSACTION_EDIT, newPermission: PERMISSIONS.INVENTORY_TRANSACTION_CREATE, onEdit: (row) => navigate(INVENTORY_ROUTE_PATHS.releasedProductDetails(row.id)), onNew: () => setCreating(true) },
    utilities: { personalizeLabel: t('common.personalize'), guideLabel: t('common.guide'), notificationsLabel: t('common.notifications'), refreshLabel: t('actions.refresh'), openWindowLabel: t('common.openWindow') },
  };

  return (
    <SimpleListPage
      title={t('releasedProduct.title', 'Released products')}
      enterpriseConfig={config}
      recordTableName="InventTable"
      dataSource={{ type: 'controlled', rows: query.data ?? [] }}
      columns={columns}
      loading={query.isLoading}
      error={query.error instanceof Error ? query.error.message : null}
      onRetry={() => query.refetch()}
      dataGridProps={{ storageKey: 'inventory.released-products.standard-view' }}
      dialogs={<ReleasedProductCreateDrawer open={creating} onClose={() => setCreating(false)} onCreated={async () => { setCreating(false); await queryClient.invalidateQueries({ queryKey: ['inventory', 'released-products', currentCompany] }); }} />}
    />
  );
}
