import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import type { InventItemGroupPosting, InventItemGroupRecord } from '../api/inventItemGroupApi';

export interface PostingRowDefinition {
  accountType: number;
  label: string;
}

interface PostingRow extends InventItemGroupPosting {
  accountLabel: string;
}

export function InventItemGroupPostingGrid({
  record,
  editing,
  rows: definitions,
  ledgerAccounts,
  storageKey,
  onRecordChange,
}: {
  record: InventItemGroupRecord;
  editing: boolean;
  rows: PostingRowDefinition[];
  ledgerAccounts: InventItemGroupPosting[];
  storageKey: string;
  onRecordChange: (record: InventItemGroupRecord) => void;
}): React.ReactElement {
  const rows: PostingRow[] = definitions.map((definition) => {
    const posting = record.postings.find((item) => item.accountType === definition.accountType);
    return {
      id: posting?.id ?? `account-type-${definition.accountType}`,
      accountType: definition.accountType,
      accountLabel: definition.label,
      ledgerDimension: posting?.ledgerDimension ?? '0',
      mainAccount: posting?.mainAccount ?? '',
    };
  });
  const setAccount = (accountType: number, ledgerDimension: string) => {
    const account = ledgerAccounts.find((item) => item.ledgerDimension === ledgerDimension);
    const next: InventItemGroupPosting = {
      id:
        record.postings.find((item) => item.accountType === accountType)?.id ??
        `new-${crypto.randomUUID()}`,
      accountType,
      ledgerDimension,
      mainAccount: account?.mainAccount ?? '',
    };
    onRecordChange({
      ...record,
      postings: record.postings.some((item) => item.accountType === accountType)
        ? record.postings.map((item) => (item.accountType === accountType ? next : item))
        : [...record.postings, next],
    });
  };
  const columns: ColumnDef<PostingRow>[] = [
    { field: 'accountLabel', headerName: 'Account type', width: 285 },
    {
      field: 'mainAccount',
      headerName: 'Main account',
      minWidth: 220,
      flex: 1,
      renderCell: ({ row }) =>
        editing ? (
          <TextField
            select
            variant="standard"
            value={row.ledgerDimension}
            onChange={(event) => setAccount(row.accountType, event.target.value || '0')}
            fullWidth
            slotProps={{ select: { displayEmpty: true } }}
          >
            <MenuItem value="0">&nbsp;</MenuItem>
            {ledgerAccounts.map((account) => (
              <MenuItem key={account.ledgerDimension} value={account.ledgerDimension}>
                {account.mainAccount}
              </MenuItem>
            ))}
          </TextField>
        ) : (
          row.mainAccount
        ),
    },
  ];
  return (
    <DataGrid
      rows={rows}
      columns={columns}
      height={Math.min(320, 58 + rows.length * 32)}
      rowHeight={32}
      selectionMode="single"
      hideToolbar
      hideFooter
      hideSidebar
      hideFilterRow
      storageKey={storageKey}
    />
  );
}
