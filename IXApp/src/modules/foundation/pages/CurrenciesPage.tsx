import React, { useMemo } from 'react';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { StatusBadge } from '@shared/components/status/StatusBadge';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { MOCK_CURRENCIES, type Currency } from '@mocks/data/currencies';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function CurrenciesPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const columns = useMemo<ColumnDef<Currency>[]>(() => [
    { field: 'currencyCode', headerName: 'fields.code', width: 110 },
    { field: 'name', headerName: 'fields.currencyName', minWidth: 220, flex: 1 },
    { field: 'symbol', headerName: 'fields.symbol', width: 100 },
    { field: 'numberOfDecimals', headerName: 'fields.decimals', type: 'number', width: 110, align: 'right' },
    { field: 'active', headerName: 'common.status', width: 120, renderCell: ({ row }) => <StatusBadge status={row.active ? 'active' : 'blocked'} /> },
  ], []);

  return (
    <SimpleListPage
      title={t('pages.currencies.title')}
      subtitle={t('pages.currencies.subtitle')}
      dataGridProps={{ rows: MOCK_CURRENCIES, columns, storageKey: 'foundation.currencies', hideAddRowButton: true }}
    />
  );
}
