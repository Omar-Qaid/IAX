import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { GENERAL_LEDGER_ROUTE_PATHS } from '../../../general-ledger/routes/generalLedgerRoutePaths';
import { vendTransApi, type VendTransRow } from '../api/vendTransApi';

const money = (value: number) =>
  value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const date = (value: string) => (value && !value.startsWith('0001') ? value.slice(0, 10) : '');
const transTypes: Record<number, string> = {
  3: 'Purchase order',
  14: 'Vendor',
  15: 'Payment',
  17: 'Bank',
  24: 'Settlement',
};

/** D365 VendTrans form: the transactions of one vendor (read-only). */
export function VendTrans() {
  const company = useCompanyStore((state) => state.currentCompany);
  const { accountNum = '' } = useParams();
  const navigate = useNavigate();
  const query = useQuery({
    queryKey: ['vendor-transactions', company, accountNum],
    queryFn: ({ signal }) => vendTransApi.list(accountNum, signal),
    enabled: !!accountNum,
  });
  const columns = useMemo<ColumnDef<VendTransRow>[]>(
    () => [
      {
        field: 'transDate',
        headerName: 'Date',
        width: 110,
        valueGetter: ({ row }) => date(row.transDate),
      },
      { field: 'voucher', headerName: 'Voucher', width: 150 },
      { field: 'invoice', headerName: 'Invoice', width: 140 },
      {
        field: 'transType',
        headerName: 'Transaction type',
        width: 140,
        valueGetter: ({ row }) => transTypes[row.transType] ?? String(row.transType),
      },
      { field: 'txt', headerName: 'Description', width: 240 },
      {
        field: 'amountCur',
        headerName: 'Amount in transaction currency',
        width: 170,
        type: 'number',
        valueGetter: ({ row }) => money(row.amountCur),
      },
      { field: 'currencyCode', headerName: 'Currency', width: 90 },
      {
        field: 'amountMst',
        headerName: 'Amount',
        width: 130,
        type: 'number',
        valueGetter: ({ row }) => money(row.amountMst),
      },
      {
        field: 'balance',
        headerName: 'Balance',
        width: 130,
        type: 'number',
        valueGetter: ({ row }) => money(row.amountCur - row.settleAmountCur),
      },
      {
        field: 'dueDate',
        headerName: 'Due date',
        width: 110,
        valueGetter: ({ row }) => date(row.dueDate),
      },
      {
        field: 'closed',
        headerName: 'Closed',
        width: 110,
        valueGetter: ({ row }) => date(row.closed),
      },
    ],
    []
  );
  return (
    <SimpleListPage<VendTransRow>
      title="Vendor transactions"
      contextLabel={`Vendor transactions | ${accountNum}`}
      recordTableName="VendTrans"
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows: query.data ?? [],
        loading: query.isLoading,
        error: query.error?.message,
        refresh: () => void query.refetch(),
      }}
      enterpriseConfig={{
        readOnly: true,
        backCommand: { onClick: () => navigate(-1) },
        getCommands: (row) => [
          {
            id: 'voucher',
            label: 'Voucher',
            requiresSelection: true,
            disabled: !row?.voucher,
            onClick: () =>
              row &&
              navigate(
                `${GENERAL_LEDGER_ROUTE_PATHS.VOUCHER_TRANSACTIONS}?voucher=${encodeURIComponent(row.voucher)}`
              ),
          },
        ],
      }}
      dataGridProps={{ storageKey: `VendTrans:${company}` }}
    />
  );
}
